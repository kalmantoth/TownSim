using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using Random = System.Random;



public enum WorkerStatusType { IDLE = 1, MOVING = 2 , WOOD_CHOPPING = 3, STONE_MINING = 4, FISHING = 5, FARMING = 6, CONSTRUCING = 7, GATHERING = 8, HUNTING = 9, ITEMDEPOSIT = 10, ITEMDRAW = 11, COOKING = 12 }
public enum BuildingType { HOUSE, TOWNHALL, STORAGE, CAMPFIRE, GRANARY, FARM, MILLBAKERY, TRADINGPOST}
public enum FarmType { NOTHING, WHEAT, POTATO}
public enum AnimalType { DEER }
public enum Season { SPRING, SUMMER, AUTUMN, WINTER }

public static class Utils
{
     public static float CalculateDistance(GameObject from, GameObject to)
     {
          return Vector3.Distance(from.transform.position, to.transform.position);
     }
     
     public static ItemGroup SpecifyItemGroup(ItemType itemType)
     {
          switch (itemType)
          {
               case ItemType.WOOD:
               case ItemType.STONE:
               case ItemType.GOLD:
                    return ItemGroup.RESOURCE;
               case ItemType.RAW_FISH:
               case ItemType.RAW_MEAT:
               case ItemType.POTATO:
               case ItemType.WHEAT:
               case ItemType.BAKED_POTATO:
               case ItemType.BREAD:
               case ItemType.BERRY:
               case ItemType.COOKED_FISH:
               case ItemType.COOKED_MEAT:
                    return ItemGroup.FOOD;
               default:
                    return ItemGroup.NOTHING;
          }
     }

     public static ItemType SpecifyCookedPairOfRawFood(ItemType rawFoodType)
     {
          switch (rawFoodType)
          {
               case ItemType.RAW_FISH:
                    return ItemType.COOKED_FISH;
               case ItemType.RAW_MEAT:
                    return ItemType.COOKED_MEAT;
               case ItemType.POTATO:
                    return ItemType.BAKED_POTATO;
               default:
                    return ItemType.NOTHING;
          }
     }


     static Texture2D _whiteTexture;
     public static Texture2D WhiteTexture
     {
          get
          {
               if (_whiteTexture == null)
               {
                    _whiteTexture = new Texture2D(1, 1);
                    _whiteTexture.SetPixel(0, 0, Color.white);
                    _whiteTexture.Apply();
               }

               return _whiteTexture;
          }
     }

     public static void DrawScreenRect(Rect rect, Color color)
     {
          GUI.color = color;
          GUI.DrawTexture(rect, WhiteTexture);
          GUI.color = Color.white;
     }

     public static void DrawScreenRectBorder(Rect rect, float thickness, Color color)
     {
          // Top
          Utils.DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
          // Left
          Utils.DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
          // Right
          Utils.DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
          // Bottom
          Utils.DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
     }

     public static Rect GetScreenRect(Vector3 screenPosition1, Vector3 screenPosition2)
     {
          // Move origin from bottom left to top left
          screenPosition1.y = Screen.height - screenPosition1.y;
          screenPosition2.y = Screen.height - screenPosition2.y;
          // Calculate corners
          Vector3 topLeft = Vector3.Min(screenPosition1, screenPosition2);
          Vector3 bottomRight = Vector3.Max(screenPosition1, screenPosition2);
          // Create Rect
          return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
     }

     public static Bounds GetViewportBounds(Camera camera, Vector3 screenPosition1, Vector3 screenPosition2)
     {
          Vector3 v1 = Camera.main.ScreenToViewportPoint(screenPosition1);
          Vector3 v2 = Camera.main.ScreenToViewportPoint(screenPosition2);
          Vector3 min = Vector3.Min(v1, v2);
          Vector3 max = Vector3.Max(v1, v2);
          min.z = camera.nearClipPlane;
          max.z = camera.farClipPlane;

          Bounds bounds = new Bounds();
          bounds.SetMinMax(min, max);
          return bounds;
     }

     public static GameObject SetWorkerTargetPoint(GameObject gameObject)
     {
          if (gameObject.transform.FindChild("WorkerTargetPoint") != null)
          {
               return gameObject.transform.FindChild("WorkerTargetPoint").gameObject;
          }
          else
          {
               return gameObject;
          }
     }


     public static Bounds GetOrthographicBounds(Camera camera)
     {
          if (!camera.orthographic)
          {
               Debug.Log("The give camera is not orthographic");
          }

          float screenAspect = (float)Screen.width / (float)Screen.height;
          float cameraHeight = camera.orthographicSize * 2;
          Bounds bounds = new Bounds(
              camera.transform.position,
              new Vector3(cameraHeight * screenAspect, cameraHeight, 0));
          return bounds;
     }

     public static Bounds GetBoundsOfTwoTransformPosition(Vector3 transformPosition1, Vector3 transformPosition2)
     {
          Vector3 min = Vector3.Min(transformPosition1, transformPosition2);
          Vector3 max = Vector3.Max(transformPosition1, transformPosition2);

          Bounds bounds = new Bounds();
          bounds.SetMinMax(min, max);
          return bounds;
     }

     public static bool BoundsIsEncapsulated(Bounds Encapsulator, Bounds Encapsulating)
     {
          return Encapsulator.Contains(Encapsulating.min) && Encapsulator.Contains(Encapsulating.max);
     }

}

public static class GlobVars
{
     public static int POPULATION = 0, WOOD = 0, STONE = 0 , FOOD = 0, GOLD = 0, RAW_FOOD = 0;
     public static GameObject infoPanelGameObject = null;
     public static int ingameClock = 0;
     public static float ingameClockInFloat = 0f;
     public static List<GameObject> workerList = new List<GameObject>();
     public static List<GameObject> buildingList = new List<GameObject>();
     public static int selectedWorkerCount = 0;
     public static Season season = Season.SUMMER;
     public static bool firstDayOfSeason;



     public static void AddWorkerToWorkerList(GameObject worker)
     {
          workerList.Add(worker);
     }

     public static void UnselectWorkers()
     {
          foreach (GameObject worker in workerList)
          {
               if(worker.GetComponent<WorkerScript>().unitIsSelected)
               {
                    worker.GetComponent<WorkerScript>().UnselectWorker();
               }
          }
     }

     public static ItemType FirstEdibleFood()
     {
          foreach (GameObject building in buildingList)
          {
               if(building.GetComponent<BuildingScript>().inventory != null && (building.GetComponent<BuildingScript>().inventory.itemGroup == ItemGroup.ALL || building.GetComponent<BuildingScript>().inventory.itemGroup == ItemGroup.FOOD))
               {
                    return building.GetComponent<BuildingScript>().inventory.GetFirstFoodWhich(false);
               }

          }
          return ItemType.NOTHING;
     }

}

public class GameMasterScript : MonoBehaviour
{
     
     public Camera cam;
     public Camera assistCam;

     public Text SupplyText;
     public Text GameDateText;
     public Text TownLevelText;
     public Text InfoPanelTargetDataText;
     public Text GoalText;
     public GameObject BuildingPanel;
     public Button BuildingPanelVisibilityButton;
     public Dropdown BuildingTypeDropdown;
     public Button TownLevelUpButton;
     
     public int yearPassInMinutes;

     private DateTime gameTime;
     private float dateIncreaseTimer, dateIncreaseTimerInitial , foodConsumptionTimer , foodConsumptionTimerInitial;
     private float animalSpawnTimer;
     private float ingameClockInFloat;

     private bool isBuildingModeOn;

     public GameObject animalSpawnPoint1;
     public GameObject animalSpawnPoint2;
     public GameObject animalDestination1;
     public GameObject animalDestination2;

     private GameObject buildingGrid;

     private Random rnd;

     private int townLevel;


     private bool isMouseSelecting;
     private Vector3 mousePosition1;
     

     private Transform mapTopRightCorner;
     private Transform mapBottomLeftCorner;
     private Bounds mapBounds;

     void Awake()
     {
          // Map Corner settings
          mapTopRightCorner = this.gameObject.transform.FindChild("MapTopRightCorner").gameObject.transform;
          mapBottomLeftCorner = this.gameObject.transform.FindChild("MapBottomLeftCorner").gameObject.transform;
          mapBounds = Utils.GetBoundsOfTwoTransformPosition(mapTopRightCorner.position, mapBottomLeftCorner.position);
          

          // Timer settings
          gameTime = new DateTime(1525, 6, 1);
          dateIncreaseTimerInitial = (yearPassInMinutes * 60f) / (365f);
          //Debug.Log(dateIncreaseTimerInitial);
          dateIncreaseTimer = dateIncreaseTimerInitial;



          ingameClockInFloat = 0f;

          foodConsumptionTimer = foodConsumptionTimerInitial = 10f;

          // First spawn of animal
          animalSpawnTimer = 1f;

          // Building mode settings
          isBuildingModeOn = false;

          

          // Default UI settings

          SupplyText.text = "SUPPLIES   Wood: " + GlobVars.WOOD + "   Stone: " + GlobVars.STONE;
          GameDateText.text = "Date: " + gameTime.Year + "." + gameTime.Month + "." + gameTime.Day;
          TownLevelText.text = "Town level 0";
          InfoPanelTargetDataText.text = "";

          SetBuildingTypeScrollDownMenuOptions();

          // CHEAT MODE - LOTS OF SUPPLIES
          // GlobVars.WOOD = GlobVars.STONE = GlobVars.FOOD = GlobVars.GOLD = 10000;

          buildingGrid = GameObject.Find("Buildings");

          rnd = new Random();

          townLevel = 0;
          TownLevelUpButton.gameObject.SetActive(false);
          TownLevelUpButton.onClick.AddListener(TownLevelUp);

          BuildingPanelVisibilityButton.onClick.AddListener(OpenBuildingPanel);



          

          FindAllPlacedBuilding();

          isMouseSelecting = false;


     }

     


     
     void Update()
     {
          UpdateIngameTime();
          CameraMovementManagement();


          //CheckTownUpgradePossibility();

          
          //ManageFoodConsumption();
          RecountStoredResources();
          UpdateTopUIBar();
          SpawnAnimals();

          RectangleUnitSelect();

          

          if (!EventSystem.current.IsPointerOverGameObject())
          {
               PlaceBuilding();
               ManageWorkers();
               UpdateInfoPanelText();
          }
          

          if (GlobVars.ingameClockInFloat % 0.01f == 0)
          {
          }

     }
     private void LateUpdate()
     {
          UpdateSeason();
          ChangeGroundBySeason();
     }

     private void OnGUI()
     {
          if (isMouseSelecting)
          {
               // Create a rect from both mouse positions
               var rect = Utils.GetScreenRect(mousePosition1, Input.mousePosition);
               Utils.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
               Utils.DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));
          }
     }


     public void OpenBuildingPanel()
     {
          if (BuildingPanel.GetComponent<Animator>().GetBool("PanelOpen"))
          {
               BuildingPanel.GetComponent<Animator>().SetBool("PanelOpen", false);
          }
          else
          {
               BuildingPanel.GetComponent<Animator>().SetBool("PanelOpen", true);
          }

     }

     public void CheckTownUpgradePossibility()
     {
          if (GlobVars.WOOD >= 200 && GlobVars.STONE >= 200)
          {
               TownLevelUpButton.gameObject.SetActive(true);
          }
          
     }


     public void TownLevelUp()
     {
          townLevel += 1;
          GlobVars.WOOD -= 200;
          GlobVars.STONE -= 200;
          TownLevelUpButton.gameObject.SetActive(false);
          GoalText.text = "GOAL: None.";
     }


     private void SpawnAnimals()
     {
          animalSpawnTimer -= Time.deltaTime;
          

          if (animalSpawnTimer <= 0)
          {
               int randomedAnimalSpawnTime = rnd.Next(90, 180);

               animalSpawnTimer = randomedAnimalSpawnTime;

               int randomedSpawnPointNumber = rnd.Next(0, 2);
               int randomedDestinationNumber = rnd.Next(0, 2);
              

               GameObject[] animalSpawnPointArray = { animalSpawnPoint1, animalSpawnPoint2 };
               GameObject[] animalDestinationArray =  { animalDestination1 , animalDestination2};
               

               GameObject newDeer = Instantiate(Resources.Load("Deer"), animalSpawnPointArray[randomedSpawnPointNumber].transform.position, Quaternion.identity) as GameObject;
               newDeer.GetComponent<AnimalScript>().target = animalDestinationArray[randomedDestinationNumber].transform.gameObject;
               newDeer.GetComponent<AnimalScript>().agent.SetDestination(animalDestinationArray[randomedDestinationNumber].transform.position);
               newDeer.name = "Deer";
               newDeer.transform.SetParent(GameObject.Find("Animals").GetComponent<Transform>());
               Debug.Log("New Deer is spawned.");
               

          }
     }


     private void PlaceBuilding()
     {

          if (BuildingPanel.GetComponent<Animator>().GetBool("PanelOpen")) isBuildingModeOn = true;
          else isBuildingModeOn = false;


          if (Input.GetMouseButtonDown(0) && isBuildingModeOn == true)
          {
               RaycastHit hitInfo;
               Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
               
               if (Physics.Raycast(ray, out hitInfo) && hitInfo.transform.tag != "Object")
               {
                    Debug.Log("The given point's position is " + hitInfo.point.ToString());
                    PlaceBuildingNear(hitInfo.point);
               }
          }
     }

     private void PlaceBuildingNear(Vector3 clickPoint)
     {
          Vector3 finalPosition = buildingGrid.GetComponent<GridScript>().GetNearestPointOnGrid(clickPoint);
          Debug.Log("The corrigated point's position is " + finalPosition.ToString());

          GameObject newBuilding = null;
          if (BuildingTypeDropdown.value == 0 && GlobVars.WOOD >= 30 && GlobVars.STONE >= 10)
          {
               newBuilding = Instantiate(Resources.Load("House"), finalPosition, Quaternion.identity) as GameObject;
               DecreaseResource(ItemType.WOOD, 30); DecreaseResource(ItemType.STONE, 10);
               SpawnWorker(finalPosition);
          }
          else if (BuildingTypeDropdown.value == 1 && GlobVars.WOOD >= 15 && GlobVars.STONE >= 10)
          {
               newBuilding = Instantiate(Resources.Load("Storage"), finalPosition, Quaternion.identity) as GameObject;
               DecreaseResource(ItemType.WOOD, 15); DecreaseResource(ItemType.STONE, 10);
          }
          else if (BuildingTypeDropdown.value == 2 && GlobVars.WOOD >= 20 && GlobVars.STONE >= 5)
          {
               newBuilding = Instantiate(Resources.Load("Granary"), finalPosition, Quaternion.identity) as GameObject;
               DecreaseResource(ItemType.WOOD, 20); DecreaseResource(ItemType.STONE, 5);
          }
          else if (BuildingTypeDropdown.value == 3 && GlobVars.WOOD >= 10 && GlobVars.STONE >= 5)
          {
               newBuilding = Instantiate(Resources.Load("Campfire"), finalPosition, Quaternion.identity) as GameObject;
               DecreaseResource(ItemType.WOOD, 10); DecreaseResource(ItemType.STONE, 5);
          }
          else if (BuildingTypeDropdown.value == 4 && GlobVars.WOOD >= 15 && GlobVars.FOOD >= 30)
          {
               newBuilding = Instantiate(Resources.Load("Farm"), finalPosition, Quaternion.identity) as GameObject;
               DecreaseResource(ItemType.WOOD, 5); DecreaseGlobalFood(30);
          }
          else
          {
               Debug.Log("Not enough resource to build.");
          }
          
          if(newBuilding != null)
          {
               RecountStoredResources();
               newBuilding.transform.SetParent(buildingGrid.GetComponent<Transform>());
               FindAllPlacedBuilding();
          }
     }

     private void SetBuildingTypeScrollDownMenuOptions()
     {
          BuildingTypeDropdown.options.Add(new Dropdown.OptionData("House (Wood: 30, Stone: 10)"));
          BuildingTypeDropdown.options.Add(new Dropdown.OptionData("Storage (Wood: 15, Stone: 10)"));
          BuildingTypeDropdown.options.Add(new Dropdown.OptionData("Granary (Wood: 20, Stone: 5)"));
          BuildingTypeDropdown.options.Add(new Dropdown.OptionData("Campfire (Wood: 10, Stone: 5)"));
          BuildingTypeDropdown.options.Add(new Dropdown.OptionData("Farm (Wood: 5, Food: 30)"));
     }


     public void FindAllPlacedBuilding()
     {
          // Itarate trought the Buildings gameobject's children and add it to the building list
          foreach (Transform building in GameObject.Find("/BaseGrid/Buildings").transform)
          {
               if (building.GetComponent<BuildingScript>())
               {
                    if (!GlobVars.buildingList.Contains(building.gameObject))
                    {
                         Debug.Log(building.GetComponent<BuildingScript>().buildingName + " has been added to the building list");
                         GlobVars.buildingList.Add(building.gameObject);
                    }

               }
          }
     }
     
     public void RecountStoredResources()
     {
          int wood, stone, food, rawFood;
          wood = stone = food = rawFood = 0;
          foreach (GameObject building in GlobVars.buildingList)
          {
               if (building.GetComponent<BuildingScript>().buildingType == BuildingType.STORAGE || building.GetComponent<BuildingScript>().buildingType == BuildingType.TOWNHALL)
               {
                    wood += building.GetComponent<BuildingScript>().inventory.GetItemCurrentQuantity(ItemType.WOOD);
                    stone += building.GetComponent<BuildingScript>().inventory.GetItemCurrentQuantity(ItemType.STONE);
               }

               if (building.GetComponent<BuildingScript>().buildingType == BuildingType.GRANARY || building.GetComponent<BuildingScript>().buildingType == BuildingType.TOWNHALL)
               {
                    food += building.GetComponent<BuildingScript>().inventory.GetItemCurrentQuantity(ItemType.BERRY);
                    food += building.GetComponent<BuildingScript>().inventory.GetItemCurrentQuantity(ItemType.COOKED_MEAT);
                    food += building.GetComponent<BuildingScript>().inventory.GetItemCurrentQuantity(ItemType.COOKED_FISH);
                    food += building.GetComponent<BuildingScript>().inventory.GetItemCurrentQuantity(ItemType.BAKED_POTATO);
                    food += building.GetComponent<BuildingScript>().inventory.GetItemCurrentQuantity(ItemType.BREAD);
                    rawFood += building.GetComponent<BuildingScript>().inventory.GetItemCurrentQuantity(ItemType.RAW_MEAT);
                    rawFood += building.GetComponent<BuildingScript>().inventory.GetItemCurrentQuantity(ItemType.RAW_FISH);
                    rawFood += building.GetComponent<BuildingScript>().inventory.GetItemCurrentQuantity(ItemType.POTATO);
                    rawFood += building.GetComponent<BuildingScript>().inventory.GetItemCurrentQuantity(ItemType.WHEAT);

               }
          }
          GlobVars.WOOD = wood;
          GlobVars.STONE = stone;
          GlobVars.FOOD = food;
          GlobVars.RAW_FOOD = rawFood;
     }
     

     private void DecreaseResource(ItemType itemType, int decreaseValue)
     {
          int value = decreaseValue;

          foreach (GameObject building in GlobVars.buildingList)
          {

               if (building.GetComponent<BuildingScript>().inventory != null && (building.GetComponent<BuildingScript>().inventory.itemGroup == Utils.SpecifyItemGroup(itemType) || building.GetComponent<BuildingScript>().inventory.itemGroup == ItemGroup.ALL))
               {
                    if (value == 0) break;

                    if(building.GetComponent<BuildingScript>().inventory.GetItemCurrentQuantity(itemType) - value >= 0)
                    {
                         building.GetComponent<BuildingScript>().inventory.ModifyInventory(itemType, -value);
                         Debug.Log(building.GetComponent<BuildingScript>().gameObject.name + " decrased " + value + " " + itemType);
                         value -= value;
                         Debug.Log("Left resource " + value + " " + itemType);
                    }
                    else
                    {
                         value = value - building.GetComponent<BuildingScript>().inventory.GetItemCurrentQuantity(itemType);
                         building.GetComponent<BuildingScript>().inventory.ModifyInventory(itemType, -building.GetComponent<BuildingScript>().inventory.GetItemCurrentQuantity(itemType));
                         Debug.Log(building.GetComponent<BuildingScript>().gameObject.name + " decrased " + building.GetComponent<BuildingScript>().inventory.GetItemCurrentQuantity(itemType) + " " + itemType);
                         Debug.Log("Left resource " + value + " " + itemType);
                    }

                    RecountStoredResources();
               }
          }
     }

     // Szeretném az EHETŐ ételek közül csökkenteni az adott mértékkel

     private void DecreaseGlobalFood(int decreaseValue) // Iterate trough the inventory to find any kind of food and decrase it by the give value
     {
          int value = decreaseValue;

          foreach (GameObject building in GlobVars.buildingList)
          {
               if (building.GetComponent<BuildingScript>().inventory != null && (building.GetComponent<BuildingScript>().inventory.itemGroup == ItemGroup.FOOD || building.GetComponent<BuildingScript>().inventory.itemGroup == ItemGroup.ALL))
               {

                    if(building.GetComponent<BuildingScript>().inventory.findNonEmptyEdibleItemTypes() != null)
                    {
                         foreach (ItemType edibleFoodType in building.GetComponent<BuildingScript>().inventory.findNonEmptyEdibleItemTypes())
                         {
                              if (value == 0) break;
                              
                              if (building.GetComponent<BuildingScript>().inventory.GetItemCurrentQuantity(edibleFoodType) - value >= 0)
                              {
                                   building.GetComponent<BuildingScript>().inventory.ModifyInventory(edibleFoodType, -value);
                                   //Debug.Log(building.GetComponent<BuildingScript>().gameObject.name + " decrased " + value + " " + edibleFoodType);
                                   value -= value;
                                   //Debug.Log("Left resource " + value + " " + edibleFoodType);
                              }
                              else
                              {
                                   value = value - building.GetComponent<BuildingScript>().inventory.GetItemCurrentQuantity(edibleFoodType);
                                   building.GetComponent<BuildingScript>().inventory.ModifyInventory(edibleFoodType, -building.GetComponent<BuildingScript>().inventory.GetItemCurrentQuantity(edibleFoodType));
                                   //Debug.Log(building.GetComponent<BuildingScript>().gameObject.name + " decrased " + building.GetComponent<BuildingScript>().inventory.GetItemstackCurrentQuantity(edibleFoodType) + " " + edibleFoodType);
                                   //Debug.Log("Left resource " + value + " " + edibleFoodType);
                              }

                              RecountStoredResources();
                         }
                    }
               }
          }
     }

     private void ManageFoodConsumption()
     {
          foodConsumptionTimer -= Time.deltaTime;

          if (foodConsumptionTimer <= 0 && GlobVars.POPULATION > 0)
          {
               foodConsumptionTimer = foodConsumptionTimerInitial;

               Debug.Log("Global food decrased by " + GlobVars.POPULATION);
               //int calculatedFoodDecrease = GlobVars.POPULATION;
               //GlobVars.FOOD -= calculatedFoodDecrease;
               DecreaseGlobalFood(GlobVars.POPULATION);
               //if (GlobVars.FOOD < 0) GlobVars.FOOD = 0;
               
          }
     }



     private void SpawnWorker(Vector3 position)
     {
          GameObject newWorker = Instantiate(Resources.Load("Worker"), position, Quaternion.identity) as GameObject;
          newWorker.name = "Worker";
          newWorker.transform.SetParent(GameObject.Find("Workers").GetComponent<Transform>());
          //Debug.Log("New Worker is spawned.");
     }

     private void ManageWorkers()
     {
          // Worker unit orders
          if (GlobVars.workerList.Count != 0)
          {
               foreach (GameObject worker in GlobVars.workerList)
               {

                    // Setting unit AI Agent destination target
                    if (Input.GetMouseButtonDown(1) && worker.GetComponent<WorkerScript>().unitIsSelected)
                    {
                         Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                         RaycastHit hit;


                         if (Physics.Raycast(ray, out hit))
                         {
                              Debug.Log("Hitted game object: " + hit.collider.gameObject.name);

                              worker.GetComponent<WorkerScript>().agent.velocity = Vector3.zero; ;
                              worker.GetComponent<WorkerScript>().targetClickPosition = hit.point;
                              worker.GetComponent<WorkerScript>().SetTarget(hit.collider.gameObject); //Setting new target manually
                              
                         }
                    }

                    if (GlobVars.FOOD == 0)
                    {
                         worker.GetComponent<WorkerScript>().SetCooldownModifier(3f);
                    }
                    else
                    {
                         worker.GetComponent<WorkerScript>().SetCooldownModifier(1f);
                    }
               }
          }

          // Count workers to TOP UI
          GlobVars.POPULATION = GlobVars.workerList.Count;

     }

     private void ChangeGroundBySeason()
     {
          GameObject.Find("GroundSpring").GetComponent<TilemapRenderer>().enabled = false;
          GameObject.Find("GroundSummer").GetComponent<TilemapRenderer>().enabled = false;
          GameObject.Find("GroundAutumn").GetComponent<TilemapRenderer>().enabled = false;
          GameObject.Find("GroundWinter").GetComponent<TilemapRenderer>().enabled = false;

          if (GlobVars.season == Season.SPRING)
          {
               GameObject.Find("GroundSpring").GetComponent<TilemapRenderer>().enabled = true;
          }
          else if (GlobVars.season == Season.SUMMER)
          {
               GameObject.Find("GroundSummer").GetComponent<TilemapRenderer>().enabled = true;
          }
          else if (GlobVars.season == Season.AUTUMN)
          {
               GameObject.Find("GroundAutumn").GetComponent<TilemapRenderer>().enabled = true;
          }
          else if (GlobVars.season == Season.WINTER)
          {
               GameObject.Find("GroundWinter").GetComponent<TilemapRenderer>().enabled = true;
          }
     }

     private void UpdateIngameTime()
     {
          GlobVars.ingameClock = (int)(ingameClockInFloat += Time.deltaTime);
          GlobVars.ingameClockInFloat = (float)Math.Round(ingameClockInFloat, 2);
          dateIncreaseTimer -= Time.deltaTime;
          
          if (dateIncreaseTimer <= 0.0f)
          {
               dateIncreaseTimer = dateIncreaseTimerInitial;
               gameTime = gameTime.AddDays(1);
          }
     }

     private void UpdateSeason()
     {
          if (gameTime.Month >= 3 && gameTime.Month < 6) GlobVars.season = Season.SPRING;
          else if (gameTime.Month >= 6 && gameTime.Month < 9) GlobVars.season = Season.SUMMER;
          else if (gameTime.Month >= 9 && gameTime.Month < 12) GlobVars.season = Season.AUTUMN;
          else GlobVars.season = Season.WINTER;

          if (gameTime.Month == 3 || gameTime.Month == 6 || gameTime.Month == 9 || gameTime.Month == 12){
               if(gameTime.Day == 1)
               {
                    GlobVars.firstDayOfSeason = true;
               }
               else
               {
                    GlobVars.firstDayOfSeason = false;
               }
          }
          
     }


     

     private void CameraMovementManagement()
     {
          Bounds orthographicBounds = Utils.GetOrthographicBounds(cam);

          int mouseMovingScreenBoundary = 10;
          float camMovingSpeed = cam.orthographicSize * 0.7f; 
          float camZoomSpeed = 20f;

          if (Input.GetKey(KeyCode.LeftShift))
          {
               camMovingSpeed *= 2;
               camZoomSpeed *= 2;
          }
          

          if ((Input.mousePosition.x > Screen.width - mouseMovingScreenBoundary && Input.mousePosition.x < Screen.width + mouseMovingScreenBoundary ) || Input.GetKey(KeyCode.D))
          {
               assistCam.CopyFrom(cam);
               assistCam.transform.position += new Vector3(Time.deltaTime * camMovingSpeed, 0.0f, 0.0f);
               if (Utils.BoundsIsEncapsulated(mapBounds, Utils.GetOrthographicBounds(assistCam)))
                    cam.transform.position = assistCam.transform.position;
          }
          if ((Input.mousePosition.x < 0 + mouseMovingScreenBoundary && Input.mousePosition.x > 0 - mouseMovingScreenBoundary) || Input.GetKey(KeyCode.A))
          {
               assistCam.CopyFrom(cam);
               assistCam.transform.position += new Vector3(-(Time.deltaTime * camMovingSpeed), 0.0f, 0.0f);
               if (Utils.BoundsIsEncapsulated(mapBounds, Utils.GetOrthographicBounds(assistCam)))
                    cam.transform.position = assistCam.transform.position;
          }
          if ((Input.mousePosition.y > Screen.height - mouseMovingScreenBoundary && Input.mousePosition.y < Screen.height + mouseMovingScreenBoundary) || Input.GetKey(KeyCode.W))
          {
               assistCam.CopyFrom(cam);
               assistCam.transform.position += new Vector3(0.0f, Time.deltaTime * camMovingSpeed, 0.0f);
               if (Utils.BoundsIsEncapsulated(mapBounds, Utils.GetOrthographicBounds(assistCam)))
                    cam.transform.position = assistCam.transform.position;
          }
          if (Input.mousePosition.y < 0 + mouseMovingScreenBoundary && Input.mousePosition.y > 0 - mouseMovingScreenBoundary || Input.GetKey(KeyCode.S))
          {
               assistCam.CopyFrom(cam);
               assistCam.transform.position += new Vector3(0.0f, -(Time.deltaTime * camMovingSpeed), 0.0f);
               if (Utils.BoundsIsEncapsulated(mapBounds, Utils.GetOrthographicBounds(assistCam)))
                    cam.transform.position = assistCam.transform.position;
          }

          assistCam.CopyFrom(cam);
          assistCam.orthographicSize += -(Input.GetAxis("Mouse ScrollWheel") * camZoomSpeed);
          if (Utils.BoundsIsEncapsulated(mapBounds, Utils.GetOrthographicBounds(assistCam)))
               cam.orthographicSize += -(Input.GetAxis("Mouse ScrollWheel") * camZoomSpeed);


          if (cam.orthographicSize > 50f) cam.orthographicSize = 50;
          else if (cam.orthographicSize < 27f) cam.orthographicSize = 27f;

          
     }

     private void RectangleUnitSelect()
     {
          
          if (Input.GetMouseButtonDown(0))
          {
               isMouseSelecting = true;
               mousePosition1 = Input.mousePosition;
          }
          // If we let go of the left mouse button, end selection
          if (Input.GetMouseButtonUp(0))
               isMouseSelecting = false;

          rectangleUnitSelectHandler();
     }

     private void rectangleUnitSelectHandler()
     {
          if (isMouseSelecting && Vector3.Distance(mousePosition1, Input.mousePosition) > 5f)
          {
               //Debug.Log("Rectangle selection is active");
               Bounds viewportBounds = Utils.GetViewportBounds(cam, mousePosition1, Input.mousePosition);
               foreach (GameObject worker in GlobVars.workerList)
               {
                    if (viewportBounds.Contains(cam.WorldToViewportPoint(worker.transform.position)))
                    {
                         worker.GetComponent<WorkerScript>().SelectWorker();
                    }
                    else
                    {
                         worker.GetComponent<WorkerScript>().UnselectWorker();
                    }
               }
               
          }
     }

     private void UpdateInfoPanelText()
     {
          if (GlobVars.infoPanelGameObject != null)
          {
               switch (LayerMask.LayerToName(GlobVars.infoPanelGameObject.layer))
               {
                    case "Resources":
                         InfoPanelTargetDataText.text = GlobVars.infoPanelGameObject.GetComponent<ResourceScript>().ToString();
                         break;
                    case "Worker":
                         InfoPanelTargetDataText.text = GlobVars.infoPanelGameObject.GetComponent<WorkerScript>().ToString();
                         break;
                    case "Animals":
                         InfoPanelTargetDataText.text = GlobVars.infoPanelGameObject.GetComponent<AnimalScript>().ToString();
                         break;
                    case "Buildings":
                         InfoPanelTargetDataText.text = GlobVars.infoPanelGameObject.GetComponent<BuildingScript>().ToString();
                         break;
                    default:
                         InfoPanelTargetDataText.text = "No information is given.";
                         break;
               }
          }
     }



     private void UpdateTopUIBar()
     {
          GameDateText.text = "Date: " + gameTime.Year + "." + gameTime.Month + "." + gameTime.Day + " (" + GlobVars.season + ") " + "      Passed Time: " + GlobVars.ingameClock + " sec ( " + GlobVars.ingameClockInFloat +  " )";
          SupplyText.text = "Pop: " + GlobVars.POPULATION + "  Wood: " + GlobVars.WOOD + "  Stone: " + GlobVars.STONE + "  Food: " + GlobVars.FOOD + " / " + +GlobVars.RAW_FOOD +"(raw)" ;
          TownLevelText.text = "Town level " + townLevel;
     }

     
}
