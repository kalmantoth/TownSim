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

public enum ResourceType { WOOD, STONE, FOOD, GOLD};
public enum WorkerStatusType { IDLE = 1, MOVING = 2 , WOOD_CHOPPING = 3, STONE_MINING = 4, FISHING = 5, FARMING = 6, CONSTRUCING = 7, GATHERING = 8, HUNTING = 9}
public enum BuildingType { HOUSE, TOWNHALL, FARM,  MINE, LUMBERMILL, FISHERHUT, STOREHOUSE }
public enum AnimalType { DEER }
public enum Season { SPRING, SUMMER, AUTUMN, WINTER }


public static class GlobVars
{
     public static int POPULATION = 0,WOOD = 0, STONE = 0 , FOOD = 100, GOLD = 0;
     public static GameObject infoPanelGameObject = null;
     public static int ingameClock = 0;
     public static List<GameObject> workerList = new List<GameObject>();
     public static int selectedWorkerCount = 0;
     public static Season season = Season.SUMMER;
     public static bool firstDayOfSeason;



     public static void addToWorkerList(GameObject worker)
     {
          workerList.Add(worker);
     }

     public static void unselectedWorkers()
     {
          foreach (GameObject worker in workerList)
          {
               if(worker.GetComponent<WorkerScript>().unitIsSelected)
               {
                    worker.GetComponent<WorkerScript>().unselectWorker();
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
     public Toggle EnableBuildingModeToggle;
     public Dropdown BuildingTypeDropdown;
     public Button TownLevelUpButton;

     public int initialResources;


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
     


     // Start is called before the first frame update
     void Start()
     {
          // Timer settings
          gameTime = new DateTime(300, 12, 1);
          dateIncreaseTimerInitial = (yearPassInMinutes * 60f)/(365f);
          Debug.Log(dateIncreaseTimerInitial);
          dateIncreaseTimer = dateIncreaseTimerInitial;

          

          ingameClockInFloat = 0f;

          foodConsumptionTimer = foodConsumptionTimerInitial = 10f;
          
          // First spawn of animal
          animalSpawnTimer = 30f;

          // Building mode settings
          isBuildingModeOn = false;


          // Default UI settings
          SupplyText.text = "SUPPLIES   Wood: " + GlobVars.WOOD + "   Stone: " + GlobVars.STONE;
          GameDateText.text = "Date: " + gameTime.Year + "." + gameTime.Month + "." + gameTime.Day;
          TownLevelText.text = "Town level 0";
          InfoPanelTargetDataText.text = "";
          
          // CHEAT MODE - LOTS OF SUPPLIES
          // GlobVars.WOOD = GlobVars.STONE = GlobVars.FOOD = GlobVars.GOLD = 10000;
          
          buildingGrid = GameObject.Find("Buildings");

          rnd = new Random();

          townLevel = 0;
          TownLevelUpButton.gameObject.SetActive(false);
          TownLevelUpButton.onClick.AddListener(townLevelUp);


          if (initialResources != 0)
          {
               GlobVars.WOOD = GlobVars.STONE = GlobVars.FOOD = GlobVars.GOLD = initialResources; 
          }

     }



     // Update is called once per frame
     void Update()
     {
          
          updateIngameTime();
          updateSeason();
          changeGroundBySeason();
          manageFoodConsumption();
          spawnAnimals();
          checkTownUpgradePossibility();
          



          if (!EventSystem.current.IsPointerOverGameObject())
          {
               placeBuilding();
               manageWorkers();
               updateInfoPanelText();
          }
          updateTopUIBar();
          

     }

     public void checkTownUpgradePossibility()
     {
          if (GlobVars.WOOD >= 200 && GlobVars.STONE >= 200)
          {
               TownLevelUpButton.gameObject.SetActive(true);
          }
          
     }


     public void townLevelUp()
     {
          townLevel += 1;
          GlobVars.WOOD -= 200;
          GlobVars.STONE -= 200;
          TownLevelUpButton.gameObject.SetActive(false);
          GoalText.text = "GOAL: None.";
     }


     private void spawnAnimals()
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


     private void placeBuilding()
     {

          if (EnableBuildingModeToggle.isOn) isBuildingModeOn = true;
          else isBuildingModeOn = false;


          if (Input.GetMouseButtonDown(0) && isBuildingModeOn == true)
          {
               RaycastHit hitInfo;
               Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);



               if (Physics.Raycast(ray, out hitInfo) && hitInfo.transform.tag != "Object")
               {
                    Debug.Log("The given point's position is " + hitInfo.point.ToString());
                    placeBuildingNear(hitInfo.point);
               }
          }
     }

     private void placeBuildingNear(Vector3 clickPoint)
     {
          Vector3 finalPosition = buildingGrid.GetComponent<GridScript>().GetNearestPointOnGrid(clickPoint);
          Debug.Log("The corrigated point's position is " + finalPosition.ToString());

          GameObject newBuilding = null;
          if (BuildingTypeDropdown.value == 0 && GlobVars.WOOD >= 25 && GlobVars.STONE >= 5)
          {
               newBuilding = Instantiate(Resources.Load("House"), finalPosition, Quaternion.identity) as GameObject;
               newBuilding.name = "House";
               GlobVars.WOOD -= 25; GlobVars.STONE -= 5;
               spawnWorker(finalPosition);
          }
          else if (BuildingTypeDropdown.value == 1 && GlobVars.WOOD >= 80 && GlobVars.STONE >= 25)
          {
               newBuilding = Instantiate(Resources.Load("TownHall"), finalPosition, Quaternion.identity) as GameObject;
               newBuilding.name = "TownHall";
               GlobVars.WOOD -= 80; GlobVars.STONE -= 25;
               spawnWorker(finalPosition);
               spawnWorker(finalPosition);
          }
          else
          {
               Debug.Log("Not enough resource to build.");
          }



          if(newBuilding != null)
          {
               newBuilding.transform.SetParent(buildingGrid.GetComponent<Transform>());
          }
          
          
          
     }

     private void spawnWorker(Vector3 position)
     {
          GameObject newWorker = Instantiate(Resources.Load("Worker"), position, Quaternion.identity) as GameObject;
          newWorker.name = "Worker";
          newWorker.transform.SetParent(GameObject.Find("Workers").GetComponent<Transform>());
          //Debug.Log("New Worker is spawned.");
     }

     private void manageWorkers()
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
                              
                              worker.GetComponent<WorkerScript>().stopCurrentActivity();

                              worker.GetComponent<WorkerScript>().targetClickPosition = hit.point;
                              worker.GetComponent<WorkerScript>().previousTarget = worker.GetComponent<WorkerScript>().target;         //PrevTarget
                              worker.GetComponent<WorkerScript>().target = hit.collider.gameObject;                                    //Target
                              
                              worker.GetComponent<WorkerScript>().agent.SetDestination(hit.point);

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

     private void changeGroundBySeason()
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

     private void updateIngameTime()
     {
          GlobVars.ingameClock = (int)(ingameClockInFloat += Time.deltaTime);
          dateIncreaseTimer -= Time.deltaTime;
          
          if (dateIncreaseTimer <= 0.0f)
          {
               dateIncreaseTimer = dateIncreaseTimerInitial;
               gameTime = gameTime.AddDays(1);

          }
     }

     private void updateSeason()
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


     private void manageFoodConsumption()
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

     private void updateInfoPanelText()
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
                    default:
                         InfoPanelTargetDataText.text = "No information is given.";
                         break;
               }
          }
     }



     private void updateTopUIBar()
     {
          GameDateText.text = "Date: " + gameTime.Year + "." + gameTime.Month + "." + gameTime.Day + " (" + GlobVars.season + ") " + "      Passed Time: " + GlobVars.ingameClock + " sec";
          SupplyText.text = "Population: " + GlobVars.POPULATION + "    Wood: " + GlobVars.WOOD + "    Stone: " + GlobVars.STONE + "    Food: " + GlobVars.FOOD + "    Gold: " + GlobVars.GOLD;
          TownLevelText.text = "Town level " + townLevel;
     }

     
}
