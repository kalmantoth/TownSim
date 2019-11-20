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

public enum Season { SPRING, SUMMER, AUTUMN, WINTER }

public static class Utils
{
     public static float CalculateDistance(GameObject from, GameObject to)
     {
          return Vector3.Distance(from.transform.position, to.transform.position);
     }
     
     public static InventoryType SpecifyInventoryType(ItemType itemType)
     {
          switch (itemType)
          {
               case ItemType.WOOD:
               case ItemType.STONE:
                    return InventoryType.RESOURCE;
               case ItemType.RAW_FISH:
               case ItemType.RAW_MEAT:
               case ItemType.POTATO:
               case ItemType.WHEAT:
               case ItemType.BAKED_POTATO:
               case ItemType.BREAD:
               case ItemType.BERRY:
               case ItemType.COOKED_FISH:
               case ItemType.COOKED_MEAT:
                    return InventoryType.FOOD;
               default:
                    return InventoryType.EMPTY;
          }
     }

     public static ItemType SpecifyDonePairOfUndoneFood(ItemType foodType, bool reverse = false)
     {
          if (!reverse)
          {
               switch (foodType)
               {
                    case ItemType.RAW_FISH:
                         return ItemType.COOKED_FISH;
                    case ItemType.RAW_MEAT:
                         return ItemType.COOKED_MEAT;
                    case ItemType.POTATO:
                         return ItemType.BAKED_POTATO;
                    case ItemType.WHEAT:
                         return ItemType.BREAD;
                    default:
                         return ItemType.NOTHING;
               }
          }
          else
          {
               switch (foodType)
               {
                    case ItemType.COOKED_FISH:
                         return ItemType.RAW_FISH;
                    case ItemType.COOKED_MEAT:
                         return ItemType.RAW_MEAT;
                    case ItemType.BAKED_POTATO:
                         return ItemType.POTATO;
                    case ItemType.BREAD:
                         return ItemType.WHEAT;
                    default:
                         return ItemType.NOTHING;
               }
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
          if (gameObject.transform.Find("WorkerTargetPoint") != null)
          {
               return gameObject.transform.Find("WorkerTargetPoint").gameObject;
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

     public static bool IsMouseInsideOfTheScreen()
     {
          if (Input.mousePosition.x < 0 || Input.mousePosition.y < 0 || Input.mousePosition.x > Screen.width || Input.mousePosition.y > Screen.height)
          {
               return false;
          }
          else return true;
     }

}

public static class GlobVars
{
     public static int POPULATION = 0, WOOD = 0, STONE = 0 , FOOD = 0, GOLD = 0, RAW_FOOD = 0;
     public static GameObject infoPanelGameObject = null;
     public static int ingameClock = 0;
     public static float ingameClockInFloat = 0f;
     private static List<GameObject> workers = new List<GameObject>();
     private static List<GameObject> selectedWorkers = new List<GameObject>();
     public static List<GameObject> storageBuildings = new List<GameObject>(); // should change it to private WIP
     private static List<Item> unlockedItems = new List<Item>();
     private static List<Item> storedItems = new List<Item>();
     public static bool isTradingPanelOpen;
     public static Season season = Season.SUMMER;
     public static bool firstDayOfSeason;
     public static bool tradingIsEnabled = false;
     
     public static Item[] GetStoredItems()
     {
          return storedItems.ToArray();
     }

     public static void SetStoredItems()
     {
          storedItems.AddRange(unlockedItems);
     }

     public static void RecountStoredItems()
     {
          foreach(Item item in storedItems)
          {
               item.currentQuantity = 0;
               item.maxQuantity = 0;
          }

          foreach (GameObject storageBuilding in storageBuildings)
          {
               BuildingScript bS = storageBuilding.GetComponent<BuildingScript>();

               foreach (Item item in storedItems)
               {
                    foreach (Item buildingStorageItem in bS.inventory.items)
                    {
                         if(item.itemType == buildingStorageItem.itemType && buildingStorageItem.maxQuantity > 0)
                         {
                              item.currentQuantity += buildingStorageItem.currentQuantity;
                              item.maxQuantity += buildingStorageItem.maxQuantity;
                         }
                    }
               }
          }
     }

     
     public static int GetGlobalStoredItemQuantity(ItemType itemType)
     {
          int quantity = 0;
          foreach (GameObject building in storageBuildings)
          {
               BuildingScript bs = building.GetComponent<BuildingScript>();
               if (bs.inventory != null)
               {
                    quantity += bs.inventory.GetItemCurrentQuantity(itemType);
               }


          }
          return quantity;
     }

     public static void AddUnlockedItem(Item item)
     {
          unlockedItems.Add(item);
          //Debug.Log(item.itemName + " has been unlocked...");
     }

     public static Item[] GetUnlockedItems()
     {
          return unlockedItems.ToArray();
     }
     
     public static GameObject[] GetWorkers()
     {
          return workers.ToArray();
     }
     
     public static void AddWorkerToWorkers(GameObject worker)
     {
          if(!workers.Contains(worker)) workers.Add(worker);
     }

     public static void UnselectWorkers()
     {
          foreach (GameObject worker in workers)
          {
               if(worker.GetComponent<WorkerScript>().unitIsSelected)
               {
                    worker.GetComponent<WorkerScript>().UnselectWorker();
               }
          }
     }

     public static GameObject[] GetSelectedWorkers()
     {
          return selectedWorkers.ToArray();
     }

     public static void AddWorkerToSelectedWorkers(GameObject worker)
     {
          if (!selectedWorkers.Contains(worker)) selectedWorkers.Add(worker);
     }

     public static void RemoveWorkerFromSelectedWorkers(GameObject worker)
     {
          selectedWorkers.Remove(worker);
     }

     public static ItemType FirstEdibleFood()
     {
          foreach (GameObject building in storageBuildings)
          {
               if(building.GetComponent<BuildingScript>().inventory != null && (building.GetComponent<BuildingScript>().inventory.inventoryType == InventoryType.ALL || building.GetComponent<BuildingScript>().inventory.inventoryType == InventoryType.FOOD))
               {
                    return building.GetComponent<BuildingScript>().inventory.GetFirstFoodWhich(false);
               }

          }
          return ItemType.NOTHING;
     }

     public static void ModifyStoredItemQuantity(ItemType itemType, int modValue)
     {
          int value = modValue; // vigyázni mert decraseről állítom simára tehát + - konverzióra figyelni kell

          foreach (GameObject building in storageBuildings)
          {
               BuildingScript bScript = building.GetComponent<BuildingScript>();
               if (bScript.inventory != null && (bScript.inventory.inventoryType == Utils.SpecifyInventoryType(itemType) || bScript.inventory.inventoryType == InventoryType.ALL))
               {
                    if (value == 0) break;

                    if (bScript.inventory.GetItemCurrentQuantity(itemType) + value >= 0)
                    {
                         bScript.inventory.ModifyInventory(itemType, value);
                         Debug.Log(bScript.gameObject.name + " modified " + value + " " + itemType);
                         value -= value;
                         Debug.Log("Left item quantity to modify " + value + " " + itemType);
                    }
                    else
                    {
                         value = value - bScript.inventory.GetItemCurrentQuantity(itemType);
                         bScript.inventory.ModifyInventory(itemType, -bScript.inventory.GetItemCurrentQuantity(itemType));
                         Debug.Log(bScript.gameObject.name + " modified " + bScript.inventory.GetItemCurrentQuantity(itemType) + " " + itemType);
                         Debug.Log("Left item quantity to modify " + value + " " + itemType);
                    }

                    RecountStoredItemsToTopUIBar();
                    RecountStoredItems();
               }
          }
     }

     public static void RecountStoredItemsToTopUIBar() // Should rework this accord to the new RecountStoredItems() WIP TODO
     {
          int wood, stone, food, rawFood;
          wood = stone = food = rawFood = 0;
          foreach (GameObject building in storageBuildings)
          {
               BuildingScript bScript = building.GetComponent<BuildingScript>();

               if (bScript.buildingType == BuildingType.STORAGE || bScript.buildingType == BuildingType.TOWNHALL)
               {
                    wood += bScript.inventory.GetItemCurrentQuantity(ItemType.WOOD);
                    stone += bScript.inventory.GetItemCurrentQuantity(ItemType.STONE);
               }

               if (bScript.buildingType == BuildingType.GRANARY || bScript.buildingType == BuildingType.TOWNHALL)
               {
                    food += bScript.inventory.GetItemCurrentQuantity(ItemType.BERRY);
                    food += bScript.inventory.GetItemCurrentQuantity(ItemType.COOKED_MEAT);
                    food += bScript.inventory.GetItemCurrentQuantity(ItemType.COOKED_FISH);
                    food += bScript.inventory.GetItemCurrentQuantity(ItemType.BAKED_POTATO);
                    food += bScript.inventory.GetItemCurrentQuantity(ItemType.BREAD);
                    rawFood += bScript.inventory.GetItemCurrentQuantity(ItemType.RAW_MEAT);
                    rawFood += bScript.inventory.GetItemCurrentQuantity(ItemType.RAW_FISH);
                    rawFood += bScript.inventory.GetItemCurrentQuantity(ItemType.POTATO);
                    rawFood += bScript.inventory.GetItemCurrentQuantity(ItemType.WHEAT);

               }
          }

          //Debug.Log(wood + stone + food + rawFood);
          WOOD = wood;
          STONE = stone;
          FOOD = food;
          RAW_FOOD = rawFood;
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
     public GameObject TradingPanel;
     public GameObject InfoPanel;
     public Button BuildingPanelVisibilityButton;
     public Button TradingPanelVisibilityButton;
     public Button InfoPanelVisibilityButton;
     public Dropdown BuildingTypeDropdown;
     public Button TownLevelUpButton;
     
     public int yearPassInMinutes;

     private DateTime gameTime;
     private float dateIncreaseTimer, dateIncreaseTimerInitial , foodConsumptionTimer , foodConsumptionTimerInitial;
     private float animalSpawnTimer;
     private float ingameClockInFloat;

     private bool isBuildingModeOn;

     public GameObject[] animalSpawnPoints;
     public GameObject[] animalSpawnDestinations;

     private GameObject buildingGrid;

     private Random rnd;

     private int townLevel;


     private bool isMouseSelecting;
     private Vector3 mousePosition1;
     

     private Transform mapTopRightCorner;
     private Transform mapBottomLeftCorner;
     private Bounds mapBounds;

     public bool showFpsMeter;
     private float deltaTime;

     void Awake()
     {
          // Application optimizing
          Application.targetFrameRate = 60;
          deltaTime = 0.0f;
          showFpsMeter = false;



          // Map Corner settings
          mapTopRightCorner = this.gameObject.transform.Find("MapTopRightCorner").gameObject.transform;
          mapBottomLeftCorner = this.gameObject.transform.Find("MapBottomLeftCorner").gameObject.transform;
          mapBounds = Utils.GetBoundsOfTwoTransformPosition(mapTopRightCorner.position, mapBottomLeftCorner.position);
          

          // Timer settings
          gameTime = new DateTime(1525, 6, 1);
          dateIncreaseTimerInitial = (yearPassInMinutes * 60f) / (365f);
          //Debug.Log(dateIncreaseTimerInitial);
          dateIncreaseTimer = dateIncreaseTimerInitial;



          ingameClockInFloat = 0f;

          foodConsumptionTimer = foodConsumptionTimerInitial = dateIncreaseTimerInitial * 7; // Every week the food decrease

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
          TradingPanelVisibilityButton.onClick.AddListener(OpenTradingPanel);
          InfoPanelVisibilityButton.onClick.AddListener(OpenInfoPanel);




          FindAllPlacedStorageBuilding();

          isMouseSelecting = false;

          InitAnimalSpawnSetting();

          // Set all default items unlocked
          foreach (System.Reflection.FieldInfo field in typeof(DefaultItems).GetFields())
          {
               GlobVars.AddUnlockedItem((Item)field.GetValue(null));
          }

          GlobVars.SetStoredItems();
          GlobVars.RecountStoredItems();
     }
     
     void Update()
     {
          deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
          

          KeyPressManager();
          UpdateIngameTime();
          //CheckTownUpgradePossibility();
          
          ManageFoodConsumption();
          GlobVars.RecountStoredItemsToTopUIBar();
          GlobVars.RecountStoredItems();
          UpdateTopUIBar();
          SpawnAnimals();

          

          

          if (!EventSystem.current.IsPointerOverGameObject())
          {
               PlaceBuilding();
               ManageWorkers();
               RectangleUnitSelect();
               CameraMovementManagement();
          }
          
          UpdateSeason();
          UpdateInfoPanelText();
          

     }
     private void LateUpdate()
     {
          
          if (GlobVars.firstDayOfSeason) ChangeGroundBySeason();
          
     }




     private void OnGUI()
     {
          if (isMouseSelecting)
          {
               // Create a rect from both mouse positions
               Rect rectangle = Utils.GetScreenRect(mousePosition1, Input.mousePosition);
               Utils.DrawScreenRect(rectangle, new Color(0.8f, 0.8f, 0.95f, 0.25f));
               Utils.DrawScreenRectBorder(rectangle, 2, new Color(0.8f, 0.8f, 0.95f));
          }


          if (showFpsMeter)
          {
               int w = Screen.width, h = Screen.height;
               GUIStyle style = new GUIStyle();
               Rect rect = new Rect(0, 0, w, h * 2 / 100);
               style.alignment = TextAnchor.UpperLeft;
               style.fontSize = h * 2 / 100;
               style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
               float msec = deltaTime * 1000.0f;
               float fps = 1.0f / deltaTime;
               string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
               GUI.Label(rect, text, style);
          }
          

     }

     public void OpenBuildingPanel()
     {
          if (BuildingPanel.GetComponent<Animator>().GetBool("PanelOpen"))
               BuildingPanel.GetComponent<Animator>().SetBool("PanelOpen", false);
          else
               BuildingPanel.GetComponent<Animator>().SetBool("PanelOpen", true);
     }

     public void OpenTradingPanel()
     {
          if (TradingPanel.GetComponent<Animator>().GetBool("PanelOpen"))
          {
               TradingPanel.GetComponent<Animator>().SetBool("PanelOpen", false);
               GlobVars.isTradingPanelOpen = false;
          }
          else
          {
               TradingPanel.GetComponent<Animator>().SetBool("PanelOpen", true);
               GlobVars.isTradingPanelOpen = true;
          }
     }

     public void OpenInfoPanel()
     {
          if (InfoPanel.GetComponent<Animator>().GetBool("PanelOpen"))
               InfoPanel.GetComponent<Animator>().SetBool("PanelOpen", false);
          else
               InfoPanel.GetComponent<Animator>().SetBool("PanelOpen", true);
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


     public void InitAnimalSpawnSetting()
     {
          int counter = 0;
          Transform AnimalSpawnPointsContainer = GameObject.Find("/AnimalController/AnimalSpawnPoints").transform;
          animalSpawnPoints = new GameObject[AnimalSpawnPointsContainer.childCount];
          foreach (Transform spawnPoint in AnimalSpawnPointsContainer)
          {
               animalSpawnPoints[counter] = spawnPoint.gameObject;
               counter++;
          }

          counter = 0;
          Transform AnimalSpawnDestinationsContainer = GameObject.Find("/AnimalController/AnimalSpawnDestinations").transform;
          animalSpawnDestinations = new GameObject[AnimalSpawnDestinationsContainer.childCount];
          foreach (Transform spawnDestination in AnimalSpawnDestinationsContainer)
          {
               animalSpawnDestinations[counter] = spawnDestination.gameObject;
               counter++;
          }

          Debug.Log(animalSpawnPoints.Length + " animalSpawnPoints and " + animalSpawnDestinations.Length + " animalSpawnDestinations are initalized...");
     }


     private void SpawnAnimals()
     {
          animalSpawnTimer -= Time.deltaTime;
          

          if (animalSpawnTimer <= 0)
          {
               animalSpawnTimer = rnd.Next(90, 180);

               int randomedSpawnPointNumber = rnd.Next(0, animalSpawnPoints.Length);
               int randomedDestinationNumber = rnd.Next(0, animalSpawnDestinations.Length);
              
               GameObject newDeer = Instantiate(Resources.Load("Deer"), animalSpawnPoints[randomedSpawnPointNumber].transform.position, Quaternion.identity) as GameObject;
               newDeer.GetComponent<AnimalScript>().target = animalSpawnDestinations[randomedDestinationNumber];
               newDeer.GetComponent<AnimalScript>().agent.SetDestination(animalSpawnDestinations[randomedDestinationNumber].transform.position);
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
               RaycastHit hitInfo = new RaycastHit();
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
          else if (BuildingTypeDropdown.value == 5 && GlobVars.WOOD >= 50 && GlobVars.FOOD >= 25)
          {
               newBuilding = Instantiate(Resources.Load("MillBakery"), finalPosition, Quaternion.identity) as GameObject;
               DecreaseResource(ItemType.WOOD, 50); DecreaseResource(ItemType.STONE, 25);
          }
          else if (BuildingTypeDropdown.value == 6 && GlobVars.WOOD >= 125 && GlobVars.FOOD >= 50)
          {
               newBuilding = Instantiate(Resources.Load("TradingPost"), finalPosition, Quaternion.identity) as GameObject;
               DecreaseResource(ItemType.WOOD, 125); DecreaseResource(ItemType.STONE, 50);
          }
          else
          {
               Debug.Log("Not enough resource to build.");
          }
          
          if(newBuilding != null)
          {
               GlobVars.RecountStoredItemsToTopUIBar();
               newBuilding.transform.SetParent(buildingGrid.GetComponent<Transform>());
               FindAllPlacedStorageBuilding();
          }
     }

     private void SetBuildingTypeScrollDownMenuOptions()
     {
          BuildingTypeDropdown.options.Add(new Dropdown.OptionData("House (Wood: 30, Stone: 10)"));
          BuildingTypeDropdown.options.Add(new Dropdown.OptionData("Storage (Wood: 15, Stone: 10)"));
          BuildingTypeDropdown.options.Add(new Dropdown.OptionData("Granary (Wood: 20, Stone: 5)"));
          BuildingTypeDropdown.options.Add(new Dropdown.OptionData("Campfire (Wood: 10, Stone: 5)"));
          BuildingTypeDropdown.options.Add(new Dropdown.OptionData("Farm (Wood: 5, Food: 30)"));
          BuildingTypeDropdown.options.Add(new Dropdown.OptionData("Mill and Bakery (Wood: 50, Stone: 25)"));
          BuildingTypeDropdown.options.Add(new Dropdown.OptionData("Trading Post (Wood: 125, Stone: 50)"));
     }


     public void FindAllPlacedStorageBuilding()
     {
          // Itarate trought the Buildings gameobject's children and add it to the building list
          foreach (Transform building in GameObject.Find("/BaseGrid/Buildings").transform)
          {
               if (building.GetComponent<BuildingScript>() && building.GetComponent<BuildingScript>().inventory != null)
               {
                    if (!GlobVars.storageBuildings.Contains(building.gameObject))
                    {
                         Debug.Log(building.GetComponent<BuildingScript>().buildingName + " has been added to the building list");
                         GlobVars.storageBuildings.Add(building.gameObject);
                    }

               }
          }
     }
     
     private void DecreaseResource(ItemType itemType, int decreaseValue)
     {
          int value = decreaseValue;

          foreach (GameObject building in GlobVars.storageBuildings)
          {

               if (building.GetComponent<BuildingScript>().inventory != null && (building.GetComponent<BuildingScript>().inventory.inventoryType == Utils.SpecifyInventoryType(itemType) || building.GetComponent<BuildingScript>().inventory.inventoryType == InventoryType.ALL))
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

                    GlobVars.RecountStoredItemsToTopUIBar();
               }
          }
     }

     private void DecreaseGlobalFood(int decreaseValue) // Iterate trough the inventory to find any kind of food and decrase it by the give value
     {
          int value = decreaseValue;

          foreach (GameObject building in GlobVars.storageBuildings)
          {
               if (building.GetComponent<BuildingScript>().inventory != null && (building.GetComponent<BuildingScript>().inventory.inventoryType == InventoryType.FOOD || building.GetComponent<BuildingScript>().inventory.inventoryType == InventoryType.ALL))
               {

                    if(building.GetComponent<BuildingScript>().inventory.FindNonEmptyEdibleItemTypes() != null)
                    {
                         foreach (ItemType edibleFoodType in building.GetComponent<BuildingScript>().inventory.FindNonEmptyEdibleItemTypes())
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

                              GlobVars.RecountStoredItemsToTopUIBar();
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

               if(GlobVars.FOOD > 0)
               {
                    Debug.Log("Global food decrased by " + GlobVars.POPULATION);
                    DecreaseGlobalFood(GlobVars.POPULATION);
               }
               
               
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
          if (GlobVars.GetWorkers().Length != 0)
          {
               foreach (GameObject worker in GlobVars.GetWorkers())
               {

                    // Setting unit AI Agent destination target
                    if (Input.GetMouseButtonDown(1) && worker.GetComponent<WorkerScript>().unitIsSelected)
                    {
                         Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                         RaycastHit hit = new RaycastHit(); ;


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
                         worker.GetComponent<WorkerScript>().SetCooldownModifier(1.75f);
                    }
                    else
                    {
                         worker.GetComponent<WorkerScript>().SetCooldownModifier(1f);
                    }
               }
          }

          // Count workers to TOP UI
          GlobVars.POPULATION = GlobVars.GetWorkers().Length;

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
          if (Application.isFocused)
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



               if ((Input.mousePosition.x > Screen.width - mouseMovingScreenBoundary && Input.mousePosition.x < Screen.width + mouseMovingScreenBoundary) || Input.GetKey(KeyCode.D))
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
     }

     private void KeyPressManager()
     {
          if (Input.GetKeyDown(KeyCode.F2))
          {
               showFpsMeter = !showFpsMeter;
          }
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

          RectangleUnitSelectHandler();
          
     }

     private void RectangleUnitSelectHandler()
     {
          if (isMouseSelecting && Vector3.Distance(mousePosition1, Input.mousePosition) > 10f)
          {
               //Debug.Log("Vector3.Distance(mousePosition1, Input.mousePosition): " + Vector3.Distance(mousePosition1, Input.mousePosition).ToString());
               Bounds viewportBounds = Utils.GetViewportBounds(cam, mousePosition1, Input.mousePosition);
               foreach (GameObject worker in GlobVars.GetWorkers())
               {
                    WorkerScript ws = worker.GetComponent<WorkerScript>();
                    if (!ws.workerIsHidden)
                    {
                         if (viewportBounds.Contains(cam.WorldToViewportPoint(worker.transform.position)))
                         {
                              ws.SelectWorker();
                         }
                         else
                         {
                              ws.UnselectWorker();
                         }
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
          GameDateText.text = "Date: " + gameTime.Year + "." + gameTime.Month + "." + gameTime.Day + " (" + GlobVars.season + ") "; //+ "      Passed Time: " + GlobVars.ingameClock;   " sec ( " + GlobVars.ingameClockInFloat +  " )";
          SupplyText.text = "Pop: " + GlobVars.POPULATION + "   Wood: " + GlobVars.WOOD + "   Stone: " + GlobVars.STONE + "   Food: " + GlobVars.FOOD + "   Gold: " + GlobVars.GOLD;
          TownLevelText.text = "Town level " + townLevel;
     }

     
}
