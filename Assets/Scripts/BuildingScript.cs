using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingScript : MonoBehaviour
{
     public BuildingType buildingType;
     public string buildingName;

     public bool isBuildingInUse;

     public Inventory inventory;
     public InventoryType inventoryType;



     private float campfireBurningTimerInitial;
     private float campfireBurningTimer;
     //private GameObject user;



     // Basic functions

     private void Awake()
     {
          isBuildingInUse = false;

          campfireBurningTimer = campfireBurningTimerInitial = 5f;
          
          // Reset action cooldown even on idle mode
          //if (campfireBurningTimer <= -0.25f) campfireBurningTimer = campfireBurningTimerInitial;
          if (campfireBurningTimer <= 0.0f) //doinsomething


          // Setting render sorting order by finding gameobject's global position;
               this.gameObject.GetComponentInChildren<SpriteRenderer>().sortingOrder = -(int)(this.gameObject.transform.position.y * 100);
          

          if (buildingType == BuildingType.STORAGE)
          {
               inventoryType = InventoryType.RESOURCE;
               inventory = new Inventory(500,InventoryType.RESOURCE);
               inventory.ModifyInventory(ResourceType.WOOD, 150);
               inventory.ModifyInventory(ResourceType.STONE, 150);
          }

          if (buildingType == BuildingType.GRANARY)
          {
               inventoryType = InventoryType.FOOD;
               inventory = new Inventory(200,InventoryType.FOOD);
               inventory.ModifyInventory(FoodType.RAW_MEAT, 100);
               inventory.ModifyInventory(FoodType.BERRY, 100);
               inventory.ModifyInventory(FoodType.COOKED_MEAT, 50);
          }

          if (buildingType == BuildingType.TOWNHALL)
          {
               inventoryType = InventoryType.ALL;
               inventory = new Inventory(75);
               inventory.ModifyInventory(ResourceType.WOOD, 75);
               inventory.ModifyInventory(ResourceType.STONE, 75);
               inventory.ModifyInventory(FoodType.COOKED_MEAT, 75);
          }




     }

     void Update()
     {
          
          //if (buildingType == BuildingType.CAMPFIRE && isBuildingInUse) campfireBurningTimer -= Time.deltaTime;
          if (buildingType == BuildingType.CAMPFIRE)
          {
               if (isBuildingInUse)
               {
                    //campfireBurningTimer = campfireBurningTimerInitial;
                    this.gameObject.GetComponentInChildren<SpriteRenderer>().sprite = Resources.Load<Sprite>("Buildings/campfire_burning");
               }
               else
               {
                    this.gameObject.GetComponentInChildren<SpriteRenderer>().sprite = Resources.Load<Sprite>("Buildings/campfire");
               }
               /*
               if (campfireBurningTimer <= 0.0f)
               {
                    isBuildingInUse = false;
                    this.gameObject.GetComponentInChildren<SpriteRenderer>().sprite = Resources.Load<Sprite>("Buildings/campfire");
               }*/
          }
          
          if (GlobVars.ingameClockInFloat % 0.1f == 0)
          {   
          }



          ChangeSpriteBySeason();
          
     }

     // ---------------- //
     // ---------------- //
     // ---------------- //

     // Game Mechanic functions
     /*
     public bool isUserSet()
     {
          if(this.user != null) return true;
          else return false;
     }

     public void setUser(GameObject user)
     {
          this.user = user;
     }

     public void removeUser(GameObject user)
     {
          this.user = null;
     }*/

     public void OnMouseDown()
     {
          GlobVars.infoPanelGameObject = this.gameObject;
     }

     // ---------------- //
     // ---------------- //
     // ---------------- //

     // Graphic modifying functions

     public void ChangeSpriteBySeason()
     {
          string building = this.buildingType.ToString().ToLower();

          if(!building.Equals("campfire") || !building.Equals("campfire"))
          {
               if (GlobVars.season == Season.WINTER)
               {
                    building += "_winter";
               }
               this.gameObject.GetComponentInChildren<SpriteRenderer>().sprite = Resources.Load<Sprite>("Buildings/" + building);
          }
          


     }



     // ---------------- //
     // ---------------- //
     // ---------------- //

     // String manipulation functions


     public string ToString()
     {
          if (buildingType == BuildingType.STORAGE)    return buildingName + "\n\tbuilding type: " + buildingType.ToString() + "\n\t inventory: " + inventory.ToStringNoZeroItems();
          else if (buildingType == BuildingType.GRANARY) return buildingName + "\n\tbuilding type: " + buildingType.ToString() + "\n\t inventory: " + inventory.ToStringNoZeroItems();
          else return buildingName + "\n\tbuilding type: " + buildingType.ToString();



     }
}
