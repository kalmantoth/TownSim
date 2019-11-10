using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingScript : MonoBehaviour
{
     public BuildingType buildingType;
     public string buildingName;

     public bool isBuildingInUse;

     public Inventory inventory;
     public ItemGroup itemGroup;

     public ItemType selectedActiveItemType;

     public GameObject workerTargetPoint;

     private float campfireBurningTimerInitial;
     private float campfireBurningTimer;
     //private GameObject user;

     public bool menuShow;
     public Vector3 menuShowPosition;

     // Basic functions

     private void Awake()
     {
          workerTargetPoint = Utils.SetWorkerTargetPoint(this.gameObject);
          
          isBuildingInUse = false;

          campfireBurningTimer = campfireBurningTimerInitial = 5f;
          
          // Reset action cooldown even on idle mode
          //if (campfireBurningTimer <= -0.25f) campfireBurningTimer = campfireBurningTimerInitial;
          if (campfireBurningTimer <= 0.0f) //doinsomething


          // Setting render sorting order by finding gameobject's global position;
               this.gameObject.GetComponentInChildren<SpriteRenderer>().sortingOrder = -(int)(this.gameObject.transform.position.y * 100);
          

          if (buildingType == BuildingType.STORAGE)
          {
               itemGroup = ItemGroup.RESOURCE;
               inventory = new Inventory(500 , ItemGroup.RESOURCE);
              /* inventory.ModifyInventory(ItemType.WOOD, 150);
               inventory.ModifyInventory(ItemType.STONE, 150);*/
          }

          if (buildingType == BuildingType.GRANARY)
          {
               itemGroup = ItemGroup.FOOD;
               inventory = new Inventory(200 , ItemGroup.FOOD);
               /*inventory.ModifyInventory(ItemType.RAW_MEAT, 100);
               inventory.ModifyInventory(ItemType.BERRY, 100);
               inventory.ModifyInventory(ItemType.COOKED_MEAT, 50);*/
          }

          if (buildingType == BuildingType.TOWNHALL)
          {
               itemGroup = ItemGroup.ALL;
               inventory = new Inventory(75);
               inventory.ModifyInventory(ItemType.WOOD, 75);
               inventory.ModifyInventory(ItemType.STONE, 75);
               inventory.ModifyInventory(ItemType.BERRY, 35);
          }




     }

     void Update()
     {
          if (GlobVars.ingameClockInFloat % 0.1f == 0)
          {   
          }
     }

     private void LateUpdate()
     {
          ChangeSpriteBySeason();
          ChangeSpiritBuildingTypeSpecific();
          ModifyRenderingOrder();
     }


     // ---------------- //
     // ---------------- //
     // ---------------- //

     // Game Mechanic functions
     

     public void OnMouseDown()
     {
          GlobVars.infoPanelGameObject = this.gameObject;
     }

     public void OnMouseOver()
     {
          if (Input.GetMouseButtonDown(1) && GlobVars.selectedWorkerCount == 0)
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

          if (GlobVars.season == Season.WINTER)
          {
               if(building.Equals("campfire"))
               {

               }
               else if(building.Equals("farm"))
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


     public string ToString()
     {
          if (buildingType == BuildingType.STORAGE || buildingType == BuildingType.GRANARY || buildingType == BuildingType.TOWNHALL)    return buildingName + "\n\tbuilding type: " + buildingType.ToString() + "\n\t inventory: " + inventory.ToString();
          else if(buildingType == BuildingType.FARM)
          {
               return buildingName + "\n\tbuilding type: " + buildingType.ToString() + "\n\t " + gameObject.GetComponent<FarmScript>().ToString();
               
          }
          else if (buildingType == BuildingType.CAMPFIRE)
          {
               return buildingName + "\n\tbuilding type: " + buildingType.ToString() + "\n\t selected food type: " + selectedActiveItemType.ToString();

          }
          else return buildingName + "\n\tbuilding type: " + buildingType.ToString();



     }
}
