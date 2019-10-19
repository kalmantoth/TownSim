using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingScript : MonoBehaviour
{
     public BuildingType buildingType;
     public string buildingName;


     public Inventory inventory;
     public int initWood;
     public int initStone;

     // Basic functions

     private void Awake()
     {
          // Setting render sorting order by finding gameobject's global position;
          this.gameObject.GetComponentInChildren<SpriteRenderer>().sortingOrder = -(int)(this.gameObject.transform.position.y * 100);
          

          if (buildingType == BuildingType.STORAGE)
          {
               inventory = new Inventory(250);
               inventory.ModifyInventory(ResourceType.WOOD, initWood);
               inventory.ModifyInventory(ResourceType.STONE, initStone);
          }


     }

     void Update()
     {

          if (GlobVars.ingameClockInFloat % 0.25f == 0)
          {
               
          }

          ChangeSpriteBySeason();



     }

     // ---------------- //
     // ---------------- //
     // ---------------- //

     // Game Mechanic functions

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


          if (GlobVars.season == Season.WINTER)
          {
               building += "_winter";
          }

          this.gameObject.GetComponentInChildren<SpriteRenderer>().sprite = Resources.Load<Sprite>("Buildings/" + building);


     }



     // ---------------- //
     // ---------------- //
     // ---------------- //

     // String manipulation functions


     public string ToString()
     {
          if (buildingType == BuildingType.STORAGE)    return buildingName + "\n\tbuilding type: " + buildingType.ToString() + "\n\t inventory: " + inventory.ToStringNoZeroItems();
          else return buildingName + "\n\tbuilding type: " + buildingType.ToString();



     }
}
