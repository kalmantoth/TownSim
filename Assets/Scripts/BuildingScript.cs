using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingScript : MonoBehaviour
{
     public BuildingType buildingType;
     public string buildingName;


     private void Awake()
     {
          // Setting render sorting order by finding gameobject's global position;
          this.gameObject.GetComponentInChildren<SpriteRenderer>().sortingOrder = -(int)(this.gameObject.transform.position.y * 100);

          // Setting the TownHall Sprite's color 
          if (this.buildingType == BuildingType.TOWNHALL) { transform.Find("Sprite").GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.0f, 1.0f); }
     }

     // Start is called before the first frame update
     void Start()
    {
          
          
     }

     public void changeSpriteBySeason()
     {
          string seasonResourcePath = "seasons/" + GlobVars.season.ToString().ToLower() + "/";
          string building = "";

          if (this.buildingName.Equals("House"))
          {
               building = "house";
          }

          this.gameObject.GetComponentInChildren<SpriteRenderer>().sprite = Resources.Load<Sprite>(seasonResourcePath + building);
     }

     // Update is called once per frame
     void Update()
     {
          changeSpriteBySeason();
     }
}
