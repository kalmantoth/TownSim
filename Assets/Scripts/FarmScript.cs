using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FarmScript : MonoBehaviour
{
     public FarmType farmType;
     public int stage;
     public int maxStage;

     public bool menuShow;
     public Vector3 menuShowPosition;

     public float plantGrowTimer, plantGrowTimerInit;
     public float plantFullGrowTime;

     public bool farmIsReadyToGrowPlants;
     public bool farmIsReadyToHarvest;
     public bool plantGrowingInProgress;

     

     void Awake()
     {
          farmIsReadyToGrowPlants = farmIsReadyToHarvest = false;
          plantGrowingInProgress = false;
          plantGrowTimer = plantGrowTimerInit = 0f;
          plantFullGrowTime = 15;

          /*farmType = FarmType.NOTHING;
          stage = 0;*/
     }
     
     void Update()
     {
          IsThereWinterSeason();
          PlantGrowingProgressHandler();
          
     }

     private void LateUpdate()
     {
          SpriteChangeByStage();
     }
     /*
     public void OnMouseOver()
     {
          if (Input.GetMouseButtonDown(1) && GlobVars.selectedWorkerCount == 0)
          {
               Debug.Log("Pressed right button on Farm.");
               menuShow = true;
               menuShowPosition = Input.mousePosition;
               menuShowPosition.y = Screen.height - menuShowPosition.y;
          }
     }
     */
     public void StartPlantGrowProgress()
     {
          if (farmIsReadyToGrowPlants)
          {
               farmIsReadyToGrowPlants = false;
               stage = 0;
               
               if (farmType == FarmType.POTATO)
               {
                    maxStage = 3;
               }
               else if (farmType == FarmType.WHEAT)
               {
                    maxStage = 5;
               }

               plantGrowTimer = plantGrowTimerInit = plantFullGrowTime / maxStage;
               plantGrowingInProgress = true;
          }
     }
     

     public void PlantGrowingProgressHandler()
     {
          if (plantGrowingInProgress)
          {
               plantGrowTimer -= Time.deltaTime;

               if (plantGrowTimer <= -0.1f && stage < maxStage)
               {
                    stage++;
                    plantGrowTimer = plantGrowTimerInit;

               }

               if (stage == maxStage)
               {
                    plantGrowTimer = plantGrowTimerInit;
                    plantGrowingInProgress = false;
                    farmIsReadyToHarvest = true;

                    if (farmType == FarmType.POTATO)
                    {
                         this.transform.GetComponent<ResourceScript>().itemType = ItemType.POTATO;
                         this.transform.GetComponent<ResourceScript>().fullAmount = this.transform.GetComponent<ResourceScript>().currentAmount = 50;
                    }
                    else if (farmType == FarmType.WHEAT)
                    {
                         this.transform.GetComponent<ResourceScript>().itemType = ItemType.WHEAT;
                         this.transform.GetComponent<ResourceScript>().fullAmount = this.transform.GetComponent<ResourceScript>().currentAmount = 100;
                    }

               }
          }

          if (farmIsReadyToHarvest)
          {
               if (this.gameObject.GetComponent<ResourceScript>().IsResourceEmpty())
               {
                    farmIsReadyToHarvest = false;
                    stage = 0;
                    farmIsReadyToGrowPlants = true;
               }
          }
     }

     public void SpriteChangeByStage()
     {
          if(farmType == FarmType.NOTHING)
          {
               // Doin sweet nothing :D
          }
          else if(farmType != FarmType.NOTHING && stage == 0)
          {
               this.gameObject.GetComponentInChildren<SpriteRenderer>().sprite = Resources.Load<Sprite>("Buildings/farm");
          }
          else
          {
               this.gameObject.GetComponentInChildren<SpriteRenderer>().sprite = Resources.Load<Sprite>("Buildings/farm_" + farmType.ToString().ToLower() + "_stage_" + stage);
          }
     }
     
     /*

     private void OnGUI()
     {
          if (menuShow && Input.GetMouseButton(1))
          {
               if (farmType == FarmType.NOTHING && GlobVars.season != Season.WINTER)
               {
                    GUIStyle customStyle = new GUIStyle(GUI.skin.GetStyle("label")); // Menu style
                    customStyle.fontSize = 18;
                    customStyle.normal.textColor = Color.white;

                    Utils.DrawScreenRect(new Rect(menuShowPosition.x, menuShowPosition.y - 30, 100f, 120f), Color.gray); // Background to the menu
                    GUI.Label(new Rect(menuShowPosition.x + 10, menuShowPosition.y - 30, 100f, 30f), "Plant type", customStyle); // Menu label 
                    if (GUI.Button(new Rect(menuShowPosition.x, menuShowPosition.y, 100f, 30f), "Potato"))    // Options for the menu
                    {
                         Debug.Log("Clicked on POTATO button.");
                         farmType = FarmType.POTATO;
                         farmIsReadyToGrowPlants = true;
                         menuShow = false;
                    }
                    else if (GUI.Button(new Rect(menuShowPosition.x, menuShowPosition.y + 30, 100f, 30f), "Wheat"))
                    {
                         Debug.Log("Clicked on WHEAT button.");
                         farmType = FarmType.WHEAT;
                         farmIsReadyToGrowPlants = true;
                         menuShow = false;
                    }
                    else if (GUI.Button(new Rect(menuShowPosition.x, menuShowPosition.y + 60, 100f, 30f), "Nothing"))
                    {
                         Debug.Log("Clicked on NOTHING button.");
                         farmType = FarmType.NOTHING;
                         farmIsReadyToGrowPlants = false;
                         menuShow = false;
                    }
               }
          }
          else
          {
               menuShow = false;
          }
     }
     */
     public void IsThereWinterSeason()
     {
          if(GlobVars.season == Season.WINTER)
          {
               if(farmType != FarmType.NOTHING || stage != 0)
               {
                    farmType = FarmType.NOTHING;
                    stage = 0;
                    farmIsReadyToGrowPlants = farmIsReadyToHarvest = plantGrowingInProgress = false;
               }
          }
     }

     public bool IsTheFarmNotAvailable()
     {
          if(farmType == FarmType.NOTHING || plantGrowingInProgress)  return true;
          return false;
     }


     public string ToString()
     {
          string returnString = "";
          returnString = "farm type: " + farmType.ToString() + "\n\t stage: " + stage + "\n\t ready to grow plants: " + farmIsReadyToGrowPlants.ToString() + "\n\t ready to harvest: " + farmIsReadyToHarvest.ToString() + "\n\t growing in progress: " + plantGrowingInProgress.ToString();

          if (farmIsReadyToHarvest == true)
          {
               returnString += "\n\t resource quantity: " + this.gameObject.GetComponent<ResourceScript>().currentAmount + "/" + GetComponent<ResourceScript>().fullAmount;
          }

          return returnString;
          /*
           *public bool farmIsReadyToGrowPlants;
     public bool farmIsReadyToHarvest;
     public bool plantGrowingInProgress;

           */
     }
     //public void change



}


