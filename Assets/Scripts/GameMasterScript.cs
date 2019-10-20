﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using Random = System.Random;

public enum ResourceType { NOTHING, WOOD, WOOD_PROCESSED, STONE, STONE_PROCESSED, GOLD, FOOD};
public enum FoodType { NOTHING, BERRY, RAW_MEAT, COOKED_MEAT, RAW_FISH, COOKED_FISH};
public enum ItemType { NOTHING, RESOURCE, FOOD };
public enum InventoryType { RESOURCE, FOOD, ALL };
public enum WorkerStatusType { IDLE = 1, MOVING = 2 , WOOD_CHOPPING = 3, STONE_MINING = 4, FISHING = 5, FARMING = 6, CONSTRUCING = 7, GATHERING = 8, HUNTING = 9, UNPACKING = 10 }
public enum BuildingType { HOUSE, TOWNHALL, STORAGE, CAMPFIRE, GRANARY}
public enum AnimalType { DEER }
public enum Season { SPRING, SUMMER, AUTUMN, WINTER }

public class Item
{
     public int value;

     public Item()
     {
          this.value = 0;
     }

     public Item(int value)
     {
          this.value = value;
     }

     public string ToString()
     {
          return "YO";
     }
}

public class ResourceItem : Item
{
     public ResourceType resourceType;

     public ResourceItem(ResourceType resourceType) : base()
     {
          this.resourceType = resourceType;
     }

     public ResourceItem(ResourceType resourceType, int value) : base(value)
     {
          this.resourceType = resourceType;
     }


     public string ToString()
     {
          if (this.value > 0) return "Resource / " + resourceType.ToString() + " (" + value + " gold)";
          else return "Resource / " + resourceType.ToString();
     }
}

public class FoodItem : Item
{
     public FoodType foodType;

     public FoodItem(FoodType foodType) : base()
     {
          this.foodType = foodType;
     }

     public FoodItem(FoodType foodType, int value) : base(value)
     {
          this.foodType = foodType;
     }


     public string ToString()
     {
          if (this.value > 0) return "Food / " + foodType.ToString() + " (" + value + " gold)";
          else return "Food / " + foodType.ToString();
     }
}

public class ItemStack
{

     public Item item;
     public int minQuantity;
     public int maxQuantity;
     public int currentQuantity;

     public ItemStack(Item item)
     {
          this.item = item;
          this.minQuantity = 0;
          this.maxQuantity = 100;
          this.currentQuantity = 0;
     }

     public ItemStack(Item item, int minQuantity, int maxQuantity, int currentQuantity)
     {
          this.item = item;
          this.minQuantity = minQuantity;
          this.maxQuantity = maxQuantity;

          if (currentQuantity > maxQuantity)
          {
               this.currentQuantity = this.maxQuantity;
          }
          else this.currentQuantity = currentQuantity;
     }

     public bool ModifyItemStack(int modifyingValue)
     {
          if (currentQuantity + modifyingValue >= minQuantity && currentQuantity + modifyingValue <= maxQuantity)
          {
               currentQuantity += modifyingValue;
               return true;
          }
          return false;
     }

     public string ToString()
     {
          if (item is ResourceItem)
          {
               return ((ResourceItem)item).ToString() + " " + this.currentQuantity + "/" + this.maxQuantity;
          }
          else if (item is FoodItem)
          {
               return ((FoodItem)item).ToString() + " " + this.currentQuantity + "/" + this.maxQuantity;
          }
          else
          {
               return item.ToString() + " " + this.currentQuantity + "/" + this.maxQuantity;
          }
          

     }

     

     
}

public class Inventory
{
     public InventoryType inventoryType;
     ItemStack[] itemStacks;

     int resourceTypeEnumLength = Enum.GetValues(typeof(ResourceType)).Length;
     int foodTypeEnumLength = Enum.GetValues(typeof(FoodType)).Length;

     public Inventory(int maxItemQuantity = 100, InventoryType inventoryType = InventoryType.ALL)  
     {
          this.inventoryType = inventoryType;

          if (inventoryType == InventoryType.RESOURCE)           // Inventory that can hold only resources
          {
               itemStacks = new ItemStack[resourceTypeEnumLength];
               int index = 0;
               foreach (ResourceType resType in Enum.GetValues(typeof(ResourceType)))
               {
                    itemStacks[index] = new ItemStack(new ResourceItem(resType), 0, maxItemQuantity, 0);
                    index++;
               }
          }
          else if (inventoryType == InventoryType.FOOD)          // Inventory that can hold only foods
          {
               itemStacks = new ItemStack[foodTypeEnumLength];
               int index = 0;
               foreach (FoodType foodType in Enum.GetValues(typeof(FoodType)))
               {
                    itemStacks[index] = new ItemStack(new FoodItem(foodType), 0, maxItemQuantity, 0);
                    index++;
               }
          }
          else if (inventoryType == InventoryType.ALL)           // Inventory that can hold resources and foods too
          {
               // Add all the ItemStack to the inventory at the initalization
               itemStacks = new ItemStack[resourceTypeEnumLength + foodTypeEnumLength];
               int index = 0;
               foreach (ResourceType resType in Enum.GetValues(typeof(ResourceType)))
               {
                    itemStacks[index] = new ItemStack(new ResourceItem(resType), 0, maxItemQuantity, 0);
                    index++;
               }

               foreach (FoodType foodType in Enum.GetValues(typeof(FoodType)))
               {
                    itemStacks[index] = new ItemStack(new FoodItem(foodType), 0, maxItemQuantity, 0);
                    index++;
               }
          }
          
     }

     public bool ModifyInventory(ResourceType resourceType, int modifyingValue)
     {
          foreach (ItemStack iStack in itemStacks)
          {
               if (iStack.item is ResourceItem)
               {
                    ResourceItem resourceItem = (ResourceItem)iStack.item;

                    if (resourceItem.resourceType == resourceType)
                    {
                         return iStack.ModifyItemStack(modifyingValue);
                    }
                    
               }

               
               
               
          }
          return false;
     }

     public bool ModifyInventory(FoodType foodType, int modifyingValue)
     {
          foreach (ItemStack iStack in itemStacks)
          {
               if (iStack.item is FoodItem)
               {
                    FoodItem foodItem = (FoodItem)iStack.item;

                    if (foodItem.foodType == foodType)
                    {
                         return iStack.ModifyItemStack(modifyingValue);
                    }
                    
               }

               
                    
          }
          return false;
     }

     public bool IsThereFullItemStack()
     {
          foreach (ItemStack iStack in itemStacks)
          {
               if (iStack.currentQuantity == iStack.maxQuantity)
               {
                    return true;
               }
          }

          return false;
     }

     public ItemType FullItemStackItemType()
     {
          foreach (ItemStack iStack in itemStacks)
          {
               if (iStack.currentQuantity == iStack.maxQuantity)
               {
                    if(iStack.item is ResourceItem)
                    {
                         return ItemType.RESOURCE;
                    }
                    else if(iStack.item is FoodItem)
                    {
                         return ItemType.FOOD;
                    }
                    
               }
          }
          return ItemType.NOTHING;
     }

     public void TransferFullItemStackToInventory(Inventory otherInventory, InventoryType otherInventoryType)
     {
          if (otherInventoryType == InventoryType.RESOURCE)
          {
               for (int i = 0; i < resourceTypeEnumLength; i++) //resourceTypeEnumLength
               {
                    if (otherInventory.itemStacks[i].ModifyItemStack(itemStacks[i].currentQuantity))   // Transfer full itemStack from the main inventory to the other
                    {
                         itemStacks[i].currentQuantity = 0;
                    }
               }
          }
          else if (otherInventoryType == InventoryType.FOOD)
          {
                    for (int i = resourceTypeEnumLength; i < resourceTypeEnumLength + foodTypeEnumLength; i++) //resourceTypeEnumLength
                    {
                         if (otherInventory.itemStacks[i - resourceTypeEnumLength].ModifyItemStack(itemStacks[i].currentQuantity))   // Transfer full itemStack from the main inventory to the other
                         {
                              itemStacks[i].currentQuantity = 0;
                         }
                    }
          }

     }

     public int GetItemstackCurrentQuantity(ResourceType resourceType)
     {
          return itemStacks[(int)resourceType].currentQuantity;
     }

     public int GetItemstackCurrentQuantity(FoodType foodType)
     {
          return itemStacks[(int)foodType].currentQuantity;
     }

     public string ToString()
     {
          string returnString = "";
          foreach(ItemStack iStack in itemStacks)
          {
               returnString += iStack.ToString() + "   ";
          }
          return returnString;
     }

     public string ToStringNoZeroItems()
     {
          string returnString = "";
          foreach (ItemStack iStack in itemStacks)
          {
               if (iStack.currentQuantity != 0)
               {
                    returnString += iStack.ToString() + "   ";
               }
          }
          return returnString;
     }

}


public static class GlobVars
{
     public static int POPULATION = 0, WOOD = 0, STONE = 0 , FOOD = 0, GOLD = 0;
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
}


public class GameMasterScript : MonoBehaviour
{
     
     public Camera cam;

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

     
     void Start()
     {
          // Timer settings
          gameTime = new DateTime(300, 6, 1);
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




     }

     


     
     void Update()
     {
          
          UpdateIngameTime();
          UpdateSeason();
          ChangeGroundBySeason();
          ManageFoodConsumption();
          SpawnAnimals();
          CheckTownUpgradePossibility();
          



          if (!EventSystem.current.IsPointerOverGameObject())
          {
               PlaceBuilding();
               ManageWorkers();
               UpdateInfoPanelText();
          }
          UpdateTopUIBar();

          if (GlobVars.ingameClockInFloat % 0.25f == 0)
          {
               FindAllPlacedBuilding();
               RecountStoredResources();

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
               DecreaseResource(ResourceType.WOOD, 30); DecreaseResource(ResourceType.STONE, 10);
               //GlobVars.WOOD -= 25; GlobVars.STONE -= 5;
               SpawnWorker(finalPosition);
          }
          else if (BuildingTypeDropdown.value == 1 && GlobVars.WOOD >= 80 && GlobVars.STONE >= 25)
          {
               newBuilding = Instantiate(Resources.Load("TownHall"), finalPosition, Quaternion.identity) as GameObject;
               DecreaseResource(ResourceType.WOOD, 80); DecreaseResource(ResourceType.STONE, 25);
               SpawnWorker(finalPosition);
               SpawnWorker(finalPosition);
          }
          else if (BuildingTypeDropdown.value == 2 && GlobVars.WOOD >= 15 && GlobVars.STONE >= 10)
          {
               newBuilding = Instantiate(Resources.Load("Storage"), finalPosition, Quaternion.identity) as GameObject;
               DecreaseResource(ResourceType.WOOD, 15); DecreaseResource(ResourceType.STONE, 10);
          }
          else if (BuildingTypeDropdown.value == 3 && GlobVars.WOOD >= 20 && GlobVars.STONE >= 5)
          {
               newBuilding = Instantiate(Resources.Load("Granary"), finalPosition, Quaternion.identity) as GameObject;
               DecreaseResource(ResourceType.WOOD, 20); DecreaseResource(ResourceType.STONE, 5);
          }
          else if (BuildingTypeDropdown.value == 4 && GlobVars.WOOD >= 10 && GlobVars.STONE >= 5)
          {
               newBuilding = Instantiate(Resources.Load("Campfire"), finalPosition, Quaternion.identity) as GameObject;
               DecreaseResource(ResourceType.WOOD, 10); DecreaseResource(ResourceType.STONE, 5);
          }
          else
          {
               Debug.Log("Not enough resource to build.");
          }
          
          if(newBuilding != null)
          {
               RecountStoredResources();
               newBuilding.transform.SetParent(buildingGrid.GetComponent<Transform>());
          }
     }

     private void SetBuildingTypeScrollDownMenuOptions()
     {
          BuildingTypeDropdown.options.Add(new Dropdown.OptionData("House (Wood: 30, Stone: 10)"));
          BuildingTypeDropdown.options.Add(new Dropdown.OptionData("Town Hall (Wood: 80, Stone: 25)"));
          BuildingTypeDropdown.options.Add(new Dropdown.OptionData("Storage (Wood: 15, Stone: 10)"));
          BuildingTypeDropdown.options.Add(new Dropdown.OptionData("Granary (Wood: 20, Stone: 5)"));
          BuildingTypeDropdown.options.Add(new Dropdown.OptionData("Campfire (Wood: 10, Stone: 5)"));
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
                         GlobVars.buildingList.Add(building.gameObject);
                    }

               }
          }
     }
     
     public void RecountStoredResources()
     {
          int wood, stone, food;
          wood = stone = food = 0;
          foreach (GameObject building in GlobVars.buildingList)
          {
               if (building.GetComponent<BuildingScript>().buildingType == BuildingType.STORAGE)
               {
                    wood += building.GetComponent<BuildingScript>().inventory.GetItemstackCurrentQuantity(ResourceType.WOOD);
                    stone += building.GetComponent<BuildingScript>().inventory.GetItemstackCurrentQuantity(ResourceType.STONE);
                    food += building.GetComponent<BuildingScript>().inventory.GetItemstackCurrentQuantity(ResourceType.FOOD);
               }
               else if (building.GetComponent<BuildingScript>().buildingType == BuildingType.GRANARY)
               {
                    food += building.GetComponent<BuildingScript>().inventory.GetItemstackCurrentQuantity(FoodType.BERRY);
                    food += building.GetComponent<BuildingScript>().inventory.GetItemstackCurrentQuantity(FoodType.COOKED_MEAT);
                    food += building.GetComponent<BuildingScript>().inventory.GetItemstackCurrentQuantity(FoodType.COOKED_FISH);
               }
          }
          GlobVars.WOOD = wood;
          GlobVars.STONE = stone;
          GlobVars.FOOD = food;
     }
     

     private void DecreaseResource(ResourceType resourceType, int decreaseValue)
     {
          int value = decreaseValue;

          foreach (GameObject building in GlobVars.buildingList)
          {
               if (building.GetComponent<BuildingScript>().buildingType == BuildingType.STORAGE)
               {
                    if (value == 0) break;

                    if(building.GetComponent<BuildingScript>().inventory.GetItemstackCurrentQuantity(resourceType) - value >= 0)
                    {
                         building.GetComponent<BuildingScript>().inventory.ModifyInventory(resourceType, -value);
                         Debug.Log(building.GetComponent<BuildingScript>().gameObject.name + " decrased " + value + " " + resourceType);
                         value -= value;
                         Debug.Log("Left resource " + value + " " + resourceType);
                    }
                    else
                    {
                         value = value - building.GetComponent<BuildingScript>().inventory.GetItemstackCurrentQuantity(resourceType);
                         building.GetComponent<BuildingScript>().inventory.ModifyInventory(resourceType, -building.GetComponent<BuildingScript>().inventory.GetItemstackCurrentQuantity(resourceType));
                         Debug.Log(building.GetComponent<BuildingScript>().gameObject.name + " decrased " + building.GetComponent<BuildingScript>().inventory.GetItemstackCurrentQuantity(resourceType) + " " + resourceType);
                         Debug.Log("Left resource " + value + " " + resourceType);
                    }
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
                         worker.GetComponent<WorkerScript>().actionCooldownInitial = 5f;
                    }
                    else
                    {
                         worker.GetComponent<WorkerScript>().actionCooldownInitial = 2f;
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


     private void ManageFoodConsumption()
     {
          foodConsumptionTimer  -= Time.deltaTime;

          if (foodConsumptionTimer <= 0 && GlobVars.POPULATION > 0)
          {
               foodConsumptionTimer = foodConsumptionTimerInitial;

               int calculatedFoodDecrease = GlobVars.POPULATION;
               GlobVars.FOOD -= calculatedFoodDecrease;
               if (GlobVars.FOOD < 0) GlobVars.FOOD = 0;
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
          SupplyText.text = "Population: " + GlobVars.POPULATION + "    Wood: " + GlobVars.WOOD + "    Stone: " + GlobVars.STONE + "    Food: " + GlobVars.FOOD;
          TownLevelText.text = "Town level " + townLevel;
     }

     
}
