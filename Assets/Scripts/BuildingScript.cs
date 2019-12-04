using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuildingType { HOUSE, TOWNHALL, STORAGE, CAMPFIRE, GRANARY, FARM, MILLBAKERY, TRADINGPOST }

public class BuildingScript : MonoBehaviour
{
     public BuildingType buildingType;
     public string buildingName;

     public bool isBuildingInUse;

     public Inventory inventory;

     public ItemType selectedActiveItemType;

     public GameObject workerTargetPoint;

     public List<GameObject> indoorWorkers;
     
     public bool menuShow;
     public Vector3 menuShowPosition;

     private Animator buildingAnimator;

     // Basic functions

     private void Awake()
     {
          

          indoorWorkers = new List<GameObject>();

          isBuildingInUse = false;

          ModifyRenderingOrder();

          workerTargetPoint = Utils.SetWorkerTargetPoint(this.gameObject);
          
          
          
          // Setting render sorting order by finding gameobject's global position;
          //     this.gameObject.GetComponentInChildren<SpriteRenderer>().sortingOrder = -(int)(this.gameObject.transform.position.y * 100);
          

          if (buildingType == BuildingType.STORAGE)
          {
               inventory = new Inventory(500 , InventoryType.RESOURCE);
          }

          if (buildingType == BuildingType.GRANARY)
          {
               inventory = new Inventory(200 , InventoryType.FOOD);
          }

          if (buildingType == BuildingType.TOWNHALL)
          {
               inventory = new Inventory(125, InventoryType.ALL);
               inventory.ModifyInventory(ItemType.WOOD, 25);
               inventory.ModifyInventory(ItemType.STONE, 25);
               inventory.ModifyInventory(ItemType.BERRY, 35);
          }
          
          if(this.GetComponent<Animator>() != null) buildingAnimator = this.GetComponent<Animator>();
          
     }

     void Update()
     {
          BuildingUsageManager();

          // If there is a building animator then update with the building's current status
          if (buildingAnimator != null)
          {
               buildingAnimator.SetBool("IsWinter", GlobVars.season == Season.WINTER ? true : false);
               buildingAnimator.SetBool("BuildingIsActive", isBuildingInUse);
          }

          
          
     }

     private void LateUpdate()
     {
          ChangeSpiritBuildingTypeSpecific();

          if (GlobVars.firstDayOfSeason)
          {
               ChangeSpriteBySeason();
          }
               
     }


     // ---------------- //
     // ---------------- //
     // ---------------- //

     // Game Mechanic functions

     public void BuildingUsageManager()
     {
          if(buildingType == BuildingType.MILLBAKERY)
          {
               if (indoorWorkers.Count > 0 && !isBuildingInUse)
               {
                    isBuildingInUse = true;
               }
               else if (indoorWorkers.Count == 0 && isBuildingInUse)
               {
                    isBuildingInUse = false;
               }
          }
          
     }

     public void AddIndoorWorker(GameObject worker)
     {
          if (!indoorWorkers.Contains(worker)) indoorWorkers.Add(worker);
     }

     public void RemoveIndoorWorker(GameObject worker)
     {
          Debug.Log("Worker removed from indoorWorkers list.");
          indoorWorkers.Remove(worker);
     }

     public void RemoveAllIndoorWorkers()
     {
          foreach (GameObject worker in indoorWorkers)
          {
               WorkerScript ws = worker.GetComponent<WorkerScript>();
               if (ws.workerIsHidden) ws.workerIsHidden = false;
               ws.SetTargetToGround();
          }
     }

     public void OnMouseDown()
     {
          GlobVars.infoPanelGameObject = this.gameObject;
     }

     public void OnMouseOver()
     {
          if (Input.GetMouseButtonDown(1) && GlobVars.GetSelectedWorkers().Length == 0)
          {
               Debug.Log("Pressed right button on " + buildingName);
               menuShow = true;
               menuShowPosition = Input.mousePosition;
               menuShowPosition.y = Screen.height - menuShowPosition.y;
          }
     }

     private void OnGUI()
     {
          if (menuShow && Input.GetMouseButton(1))
          {
               GUIStyle customStyle = new GUIStyle(GUI.skin.GetStyle("label")); // Menu style
               customStyle.fontSize = 18;
               customStyle.normal.textColor = Color.white;

               if (buildingType == BuildingType.FARM)
               {
                    if (this.GetComponent<FarmScript>().farmType == FarmType.NOTHING && GlobVars.season != Season.WINTER)
                    {
                         Utils.DrawScreenRect(new Rect(menuShowPosition.x, menuShowPosition.y - 30, 100f, 120f), Color.gray); // Background to the menu
                         GUI.Label(new Rect(menuShowPosition.x + 10, menuShowPosition.y - 30, 100f, 30f), "Plant type", customStyle); // Menu label 
                         if (GUI.Button(new Rect(menuShowPosition.x, menuShowPosition.y, 100f, 30f), "Potato"))    // Options for the menu
                         {
                              Debug.Log("Clicked on POTATO button.");
                              this.GetComponent<FarmScript>().farmType = FarmType.POTATO;
                              this.GetComponent<FarmScript>().farmIsReadyToGrowPlants = true;
                              menuShow = false;
                         }
                         else if (GUI.Button(new Rect(menuShowPosition.x, menuShowPosition.y + 30, 100f, 30f), "Wheat"))
                         {
                              Debug.Log("Clicked on WHEAT button.");
                              this.GetComponent<FarmScript>().farmType = FarmType.WHEAT;
                              this.GetComponent<FarmScript>().farmIsReadyToGrowPlants = true;
                              menuShow = false;
                         }
                         else if (GUI.Button(new Rect(menuShowPosition.x, menuShowPosition.y + 60, 100f, 30f), "Nothing"))
                         {
                              Debug.Log("Clicked on NOTHING button.");
                              this.GetComponent<FarmScript>().farmType = FarmType.NOTHING;
                              this.GetComponent<FarmScript>().farmIsReadyToGrowPlants = false;
                              menuShow = false;
                         }
                    }
               }
               else if(buildingType == BuildingType.CAMPFIRE)
               {
                    Utils.DrawScreenRect(new Rect(menuShowPosition.x, menuShowPosition.y - 30, 100f, 150f), Color.gray); // Background to the menu
                    GUI.Label(new Rect(menuShowPosition.x + 30, menuShowPosition.y - 30, 100f, 30f), "Cook", customStyle); // Menu label 
                    if (GUI.Button(new Rect(menuShowPosition.x, menuShowPosition.y, 100f, 30f), "Potato"))    // Options for the menu
                    {
                         selectedActiveItemType = ItemType.POTATO;
                         menuShow = false;
                    }
                    else if (GUI.Button(new Rect(menuShowPosition.x, menuShowPosition.y + 30, 100f, 30f), "Meat"))
                    {
                         selectedActiveItemType = ItemType.RAW_MEAT;
                         menuShow = false;
                    }
                    else if (GUI.Button(new Rect(menuShowPosition.x, menuShowPosition.y + 60, 100f, 30f), "Fish"))
                    {
                         selectedActiveItemType = ItemType.RAW_FISH;
                         menuShow = false;
                    }
                    else if (GUI.Button(new Rect(menuShowPosition.x, menuShowPosition.y + 90, 100f, 30f), "Anything"))
                    {
                         selectedActiveItemType = ItemType.ANYTHING;
                         menuShow = false;
                    }
               }
               else if (buildingType == BuildingType.MILLBAKERY)
               {
                    if(isBuildingInUse && indoorWorkers.Count > 0)
                    {
                         Utils.DrawScreenRect(new Rect(menuShowPosition.x, menuShowPosition.y, 100f, 30f), Color.gray); // Background to the menu
                         if (GUI.Button(new Rect(menuShowPosition.x, menuShowPosition.y, 100f, 30f), "Stop workers"))    // Options for the menu
                         {
                              RemoveAllIndoorWorkers();
                              isBuildingInUse = false;
                              menuShow = false;
                         }
                    }
                    
               }

          }
          else
          {
               menuShow = false;
          }
     }

     // ---------------- //
     // ---------------- //
     // ---------------- //

     // Graphic modifying functions

     public void ChangeSpiritBuildingTypeSpecific()
     {
          if (buildingType == BuildingType.CAMPFIRE)
          {
               if (isBuildingInUse)
               {
                    this.gameObject.GetComponentInChildren<SpriteRenderer>().sprite = Resources.Load<Sprite>("Buildings/campfire_burning");
               }
               else
               {
                    this.gameObject.GetComponentInChildren<SpriteRenderer>().sprite = Resources.Load<Sprite>("Buildings/campfire");
               }
          }
     }

     public void ChangeSpriteBySeason()
     {

          string building = this.buildingType.ToString().ToLower();
          if (!building.Equals("millbakery"))
          {
               if (GlobVars.season == Season.WINTER)
               {
                    if (building.Equals("campfire"))
                    {

                    }
                    else if (building.Equals("farm"))
                    {
                         if (this.gameObject.GetComponent<FarmScript>().stage == 0 && this.gameObject.GetComponent<FarmScript>().farmType != FarmType.NOTHING)
                         {
                              building += "_winter";
                         }
                         else if (this.gameObject.GetComponent<FarmScript>().farmType == FarmType.NOTHING)
                         {
                              building += "_winter";
                         }

                    }
                    else
                    {
                         building += "_winter";
                    }
               }

               this.gameObject.GetComponentInChildren<SpriteRenderer>().sprite = Resources.Load<Sprite>("Buildings/" + building);
          }
          
          
     }

     public void ModifyRenderingOrder()
     {
          if(this.GetComponent<ResourceScript>() == null)
          {
               this.GetComponentInChildren<SpriteRenderer>().sortingOrder = -(int)(((this.gameObject.transform.position.y) * 100) - 25); // The number -50 is because the worker render's correct display
          }
          
     }


     

     // ---------------- //
     // ---------------- //
     // ---------------- //

     // String manipulation functions


     public override string ToString()
     {
          string returnString = buildingName + "\n\tbuilding type: " + buildingType.ToString();

          if (buildingType == BuildingType.STORAGE || buildingType == BuildingType.GRANARY || buildingType == BuildingType.TOWNHALL) returnString += "\n\tinventory: " + inventory.ToString();
          else if(buildingType == BuildingType.FARM)
          {
               returnString += "\n\t " + gameObject.GetComponent<FarmScript>().ToString();
          }
          else if (buildingType == BuildingType.CAMPFIRE)
          {
               returnString += "\n\tselected food type: " + selectedActiveItemType.ToString();
          }

          if (isBuildingInUse)
          {
               if (buildingType == BuildingType.MILLBAKERY)
               {
                    ItemType doneFood = Utils.SpecifyDonePairOfUndoneFood(selectedActiveItemType);
                    returnString += "\n\t(worker: (" + selectedActiveItemType + "/" + doneFood + "))\t";
                    foreach (GameObject worker in indoorWorkers)
                    {
                         WorkerScript ws = worker.GetComponent<WorkerScript>();
                         returnString += "\t" + worker.name + ": (" + ws.inventory.GetItemCurrentQuantity(selectedActiveItemType) + "/" + ws.inventory.GetItemCurrentQuantity(doneFood) + "))\t";
                    }
               }


          }

          return returnString;

     }
}
