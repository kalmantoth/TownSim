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
using UnityEngine.SceneManagement;

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

     public static string UppercaseFirst(string str)
     {
          if (string.IsNullOrEmpty(str))
               return string.Empty;
          return char.ToUpper(str[0]) + str.Substring(1).ToLower();
     }
}

public static class GlobVars
{
     public static int POPULATION = -1, WOOD = -1, STONE = -1, FOOD = -1, RAW_FOOD = -1, GOLD = 0 ;
     public static GameObject infoPanelGameObject = null;
     public static int ingameClock = 0;
     public static Season season = Season.SUMMER;
     public static bool firstDayOfSeason;
     private static List<GameObject> workers = new List<GameObject>();
     private static List<GameObject> selectedWorkers = new List<GameObject>();
     private static List<GameObject> storageBuildings = new List<GameObject>(); 
     private static List<Item> unlockedItems = new List<Item>();
     private static List<Item> storedItems = new List<Item>();

     public static void InitGlobVars()
     {
          POPULATION =  WOOD = STONE = FOOD = RAW_FOOD = -1;
          GOLD = 0;
          infoPanelGameObject = null;
          ingameClock = 0;
          season = Season.SUMMER;
          firstDayOfSeason = true;
          workers = new List<GameObject>();
          selectedWorkers = new List<GameObject>();
          storageBuildings = new List<GameObject>(); 
          unlockedItems = new List<Item>();
          storedItems = new List<Item>();
     }

     public static GameObject[] GetStorageBuildings()
     {
          return storageBuildings.ToArray();
     }

     public static bool AddBuildingToStorageBuildings(GameObject building)
     {
          if (!storageBuildings.Contains(building))
          {
               storageBuildings.Add(building);
               return true;
          }
          else return false;

     }

     public static Item[] GetStoredItems()
     {
          return storedItems.ToArray();
     }

     public static void SetStoredItems()
     {
          if(storedItems.Count == 0)
          {
               storedItems.AddRange(unlockedItems);
          }
          
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
                              //if(item.currentQuantity != 0) Debug.Log("Refreshed Globaly stored item: " + item.itemName + " quantity: " + item.currentQuantity + " / " + item.maxQuantity);
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
          int value = modValue; 

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

     public static void RecountStoredItemsToTopUIBar() 
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

     // UI elemen definitions
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

     public GameObject buildingGrid;
     public int yearPassInMinutes;
     public int townLevel;


     private bool showFpsMeter;
     private float deltaTimeFpsMeter;

     private DateTime gameDate;
     private float dateIncreaseTimer, dateIncreaseTimerInitial, foodConsumptionTimer, foodConsumptionTimerInitial, animalSpawnTimer;
     private float ingameClockInFloat;

     private GameObject[] animalSpawnPoints, animalSpawnDestinations;
     private bool isAnimalSpawnOn;

     private bool isBuildingModeOn;

     private int goalPopulation;

     private Random rnd;


     private bool isMouseSelecting;
     private Vector3 mousePosition1;
     private Transform mapTopRightCorner, mapBottomLeftCorner;
     private Bounds mapBounds;



     void Awake()
     {
          // Application optimizing
          Application.targetFrameRate = 60;
          deltaTimeFpsMeter = 0.0f;
          showFpsMeter = false;

          // Map Corner settings
          mapTopRightCorner = this.gameObject.transform.Find("MapTopRightCorner").gameObject.transform;
          mapBottomLeftCorner = this.gameObject.transform.Find("MapBottomLeftCorner").gameObject.transform;
          mapBounds = Utils.GetBoundsOfTwoTransformPosition(mapTopRightCorner.position, mapBottomLeftCorner.position);

          // Ingame time and Timer settings
          gameDate = new DateTime(1525, 6, 1);
          dateIncreaseTimerInitial = (yearPassInMinutes * 60f) / (365f);
          dateIncreaseTimer = dateIncreaseTimerInitial;
          ingameClockInFloat = 0f;
          foodConsumptionTimer = foodConsumptionTimerInitial = dateIncreaseTimerInitial * 7; // By this timer every week the food decrease 

          // First spawn of animal
          isAnimalSpawnOn = false;
          animalSpawnTimer = 1f;
          InitAnimalSpawnSettings();

          // Building mode settings
          isBuildingModeOn = false;
          
          // Rectangle unit selection settings
          isMouseSelecting = false;

          // Town level settings
          TownLevelUpButton.gameObject.SetActive(false);
          TownLevelUpButton.onClick.AddListener(LevelUpTown);
          TownLevelManager();

          // Default UI settings
          SupplyText.text = "SUPPLIES   Wood: " + GlobVars.WOOD + "   Stone: " + GlobVars.STONE;
          GameDateText.text = "Date: " + gameDate.Year + "." + gameDate.Month + "." + gameDate.Day;
          TownLevelText.text = "Town level 0";
          InfoPanelTargetDataText.text = "";

          // UI button event listeners
          BuildingPanelVisibilityButton.onClick.AddListener(OpenBuildingPanel);
          TradingPanelVisibilityButton.onClick.AddListener(OpenTradingPanel);
          InfoPanelVisibilityButton.onClick.AddListener(OpenInfoPanel);

          rnd = new Random();

          // Item settings
          // Set all default items unlocked
          foreach (System.Reflection.FieldInfo field in typeof(DefaultItems).GetFields())
          {
               GlobVars.AddUnlockedItem((Item)field.GetValue(null));
          }

     }

     private void Start()
     {
          FindAllPlacedStorageBuilding();
          GlobVars.SetStoredItems();
          GlobVars.RecountStoredItems();
     }

     void Update()
     {
          deltaTimeFpsMeter += (Time.unscaledDeltaTime - deltaTimeFpsMeter) * 0.1f;

          KeyPressManager();
          UpdateIngameTime();

          ManageFoodConsumption();
          GlobVars.RecountStoredItems();
          GlobVars.RecountStoredItemsToTopUIBar();
          UpdateTopUIBar();
          SpawnAnimals();
          CheckTownUpgradePossibility();


          if (!EventSystem.current.IsPointerOverGameObject())
          {
               PlaceBuilding();
               ManageWorkers();
               RectangleUnitSelect();
               CameraMovementManagement();
          }


     }

     private void LateUpdate()
     {
          UpdateSeason();
          UpdateInfoPanelText();
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
               style.fontSize = h * 2 / 100 /2;
               style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
               float msec = deltaTimeFpsMeter * 1000.0f;
               float fps = 1.0f / deltaTimeFpsMeter;
               string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
               GUI.Label(rect, text, style);
          }


     }

     public void TownLevelManager()
     {
          if (townLevel == 1)
          {
               BuildingTypeDropdown.options.Add(new Dropdown.OptionData("House (Wood: 30, Stone: 10)"));
               goalPopulation = 5;
          }
          else if (townLevel == 2)
          {
               BuildingTypeDropdown.options.Add(new Dropdown.OptionData("Campfire (Wood: 10, Stone: 5)"));
               isAnimalSpawnOn = true;
               goalPopulation = 10;
          }
          else if (townLevel == 3)
          {
               BuildingTypeDropdown.options.Add(new Dropdown.OptionData("Storage (Wood: 15, Stone: 10)"));
               BuildingTypeDropdown.options.Add(new Dropdown.OptionData("Granary (Wood: 20, Stone: 5)"));
               goalPopulation = 20;
          }
          else if (townLevel == 4)
          {
               BuildingTypeDropdown.options.Add(new Dropdown.OptionData("Farm (Wood: 5, Food: 30)"));
               BuildingTypeDropdown.options.Add(new Dropdown.OptionData("Mill and Bakery (Wood: 50, Stone: 25)"));
               goalPopulation = 30;
          }
          else if (townLevel == 5)
          {
               BuildingTypeDropdown.options.Add(new Dropdown.OptionData("Trading Post (Wood: 125, Stone: 50)"));
               goalPopulation = 0;
          }

          if (townLevel >= 1 && townLevel < 5) GoalText.text = "GOAL: Reach " + goalPopulation + " population by building more houses.";
          else GoalText.text = "";

     }

     public void CheckTownUpgradePossibility()
     {
          if (GlobVars.POPULATION >= goalPopulation && goalPopulation != 0)
          {
               TownLevelUpButton.gameObject.SetActive(true);
          }

     }

     public void LevelUpTown()
     {
          townLevel += 1;
          TownLevelManager();
          TownLevelUpButton.gameObject.SetActive(false);
     }

     private void PlaceBuildingNear(Vector3 clickPoint)
     {
          Vector3 finalPosition = buildingGrid.GetComponent<GridScript>().GetNearestPointOnGrid(clickPoint);

          GameObject newBuilding = null;
          if (BuildingTypeDropdown.value == 0 && GlobVars.WOOD >= 30 && GlobVars.STONE >= 10)
          {
               newBuilding = Instantiate(Resources.Load("House"), finalPosition, Quaternion.identity) as GameObject;
               DecreaseResource(ItemType.WOOD, 30); DecreaseResource(ItemType.STONE, 10);
               SpawnWorker(finalPosition);
          }
          else if (BuildingTypeDropdown.value == 1 && GlobVars.WOOD >= 10 && GlobVars.STONE >= 5)
          {
               newBuilding = Instantiate(Resources.Load("Campfire"), finalPosition, Quaternion.identity) as GameObject;
               DecreaseResource(ItemType.WOOD, 10); DecreaseResource(ItemType.STONE, 5);
          }
          else if (BuildingTypeDropdown.value == 2 && GlobVars.WOOD >= 15 && GlobVars.STONE >= 10)
          {
               newBuilding = Instantiate(Resources.Load("Storage"), finalPosition, Quaternion.identity) as GameObject;
               DecreaseResource(ItemType.WOOD, 15); DecreaseResource(ItemType.STONE, 10);
          }
          else if (BuildingTypeDropdown.value == 3 && GlobVars.WOOD >= 20 && GlobVars.STONE >= 5)
          {
               newBuilding = Instantiate(Resources.Load("Granary"), finalPosition, Quaternion.identity) as GameObject;
               DecreaseResource(ItemType.WOOD, 20); DecreaseResource(ItemType.STONE, 5);
          }
          else if (BuildingTypeDropdown.value == 4 && GlobVars.WOOD >= 15 && GlobVars.FOOD >= 30)
          {
               newBuilding = Instantiate(Resources.Load("Farm"), finalPosition, Quaternion.identity) as GameObject;
               DecreaseResource(ItemType.WOOD, 5); DecreaseFood(30);
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
               TradingPanelVisibilityButton.interactable = true;
          }
          else
          {
               Debug.Log("Not enough resource to build.");
          }

          if (newBuilding != null)
          {
               newBuilding.transform.SetParent(buildingGrid.GetComponent<Transform>());
               FindAllPlacedStorageBuilding();
               GlobVars.RecountStoredItems();
               GlobVars.RecountStoredItemsToTopUIBar();
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
               TradingPanel.GetComponent<Animator>().SetBool("PanelOpen", false);
          else
               TradingPanel.GetComponent<Animator>().SetBool("PanelOpen", true);
     }

     public void OpenInfoPanel()
     {
          if (InfoPanel.GetComponent<Animator>().GetBool("PanelOpen"))
               InfoPanel.GetComponent<Animator>().SetBool("PanelOpen", false);
          else
               InfoPanel.GetComponent<Animator>().SetBool("PanelOpen", true);
     }
     
     public void InitAnimalSpawnSettings()
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
          if (isAnimalSpawnOn)
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
     }
     
     private void PlaceBuilding()
     {

          if (BuildingPanel.GetComponent<Animator>().GetBool("PanelOpen")) isBuildingModeOn = true;
          else isBuildingModeOn = false;


          if (Input.GetMouseButtonDown(0) && isBuildingModeOn == true)
          {
               RaycastHit hitInfo = new RaycastHit();
               Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
               
               if (Physics.Raycast(ray, out hitInfo) && hitInfo.transform.tag != "Object" && hitInfo.transform.tag != "WaterBlock")
               {
                    Vector3 correctedPositionByGrid = buildingGrid.GetComponent<GridScript>().GetNearestPointOnGrid(hitInfo.point);
                    correctedPositionByGrid.z -= 1f;
                    if (Physics.SphereCast(correctedPositionByGrid, 1f, Vector3.forward, out hitInfo) && hitInfo.transform.tag != "Object" && hitInfo.transform.tag != "WaterBlock")
                         PlaceBuildingNear(correctedPositionByGrid);
               }
          }
     }
     
     public void FindAllPlacedStorageBuilding()
     {
          // Itarate trought the Buildings gameobject's children and add it to the building list
          foreach (Transform building in buildingGrid.transform)
          {
               if (building.GetComponent<BuildingScript>() && building.GetComponent<BuildingScript>().inventory != null)
               {
                    if(GlobVars.AddBuildingToStorageBuildings(building.gameObject)) Debug.Log(building.GetComponent<BuildingScript>().buildingName + " has been added to the building list");

               }
          }
     }
     
     private void DecreaseResource(ItemType itemType, int decreaseValue)
     {
          int value = decreaseValue;

          foreach (GameObject building in GlobVars.GetStorageBuildings())
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

     private void DecreaseFood(int decreaseValue) // Iterate trough the inventory to find any kind of food and decrase it by the give value
     {
          int value = decreaseValue;

          foreach (GameObject building in GlobVars.GetStorageBuildings())
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
                    DecreaseFood(GlobVars.POPULATION);
               }
          }

          if (GlobVars.FOOD == 0)
          {
               SceneManager.LoadScene("GameOverMenu", LoadSceneMode.Single);
          }
     }
     
     private void SpawnWorker(Vector3 position)
     {
          Vector3 pos = position;
          pos.y -= 5;
          GameObject newWorker = Instantiate(Resources.Load("Worker"), pos, Quaternion.identity) as GameObject;
          newWorker.name = "Worker";
          newWorker.transform.SetParent(GameObject.Find("Workers").GetComponent<Transform>());
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

                    // if (GlobVars.FOOD == 0) worker.GetComponent<WorkerScript>().SetCooldownModifier(1.75f);
                    // else worker.GetComponent<WorkerScript>().SetCooldownModifier(1f);
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
          dateIncreaseTimer -= Time.deltaTime;
          
          if (dateIncreaseTimer <= 0.0f)
          {
               dateIncreaseTimer = dateIncreaseTimerInitial;
               gameDate = gameDate.AddDays(1);
          }
     }

     private void UpdateSeason()
     {
          if (gameDate.Month >= 3 && gameDate.Month < 6) GlobVars.season = Season.SPRING;
          else if (gameDate.Month >= 6 && gameDate.Month < 9) GlobVars.season = Season.SUMMER;
          else if (gameDate.Month >= 9 && gameDate.Month < 12) GlobVars.season = Season.AUTUMN;
          else GlobVars.season = Season.WINTER;

          if (gameDate.Month == 3 || gameDate.Month == 6 || gameDate.Month == 9 || gameDate.Month == 12){
               if(gameDate.Day == 1)
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

          if (Input.GetKeyDown(KeyCode.Escape))
          {
               if (!SceneManager.GetSceneByName("PauseMenu").isLoaded)
               {
                    SceneManager.LoadScene("PauseMenu", LoadSceneMode.Additive);
               }
          }

          //   Cheat shortcuts

          if(Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl))
          {
               if (Input.GetKeyDown(KeyCode.G))
               {
                    if (townLevel < 5) LevelUpTown();
               }
               if (Input.GetKeyDown(KeyCode.H))
               {
                    GlobVars.ModifyStoredItemQuantity(ItemType.WOOD, 20);
               }
               if (Input.GetKeyDown(KeyCode.J))
               {
                    GlobVars.ModifyStoredItemQuantity(ItemType.STONE, 20);
               }
               if (Input.GetKeyDown(KeyCode.K))
               {
                    GlobVars.ModifyStoredItemQuantity(ItemType.BERRY, 20);
               }
               if (Input.GetKeyDown(KeyCode.L))
               {
                    GlobVars.GOLD += 100;
               }
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
                         InfoPanelTargetDataText.text = "Nothing selected";
                         break;
               }
          }
     }
     
     private void UpdateTopUIBar()
     {
          GameDateText.text = "Date: " + gameDate.Year + "." + gameDate.Month + "." + gameDate.Day + " (" + GlobVars.season.ToString().ToLower() + ") ";
          SupplyText.text = "Population: " + GlobVars.POPULATION + "   Wood: " + GlobVars.WOOD + "   Stone: " + GlobVars.STONE + "   Food: " + GlobVars.FOOD + "   Gold: " + GlobVars.GOLD;
          TownLevelText.text = "Town level " + townLevel;
     }
 
}
