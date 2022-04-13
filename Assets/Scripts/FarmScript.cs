using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum FarmType { NOTHING, WHEAT, POTATO }

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
               // Doin' sweet nothing :D
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


     public override string ToString()
     {
          string returnString = "";
          returnString = "Farm type: " + Utils.UppercaseFirst(farmType.ToString()) + "\n\t Stage: " + stage + "\n\t Ready to grow plants: " + farmIsReadyToGrowPlants.ToString() + "\n\t Ready to harvest: " + farmIsReadyToHarvest.ToString() + "\n\t Growing in progress: " + plantGrowingInProgress.ToString();

          if (farmIsReadyToHarvest == true)
          {
               returnString += "\n\t Resource quantity: " + this.gameObject.GetComponent<ResourceScript>().currentAmount + "/" + GetComponent<ResourceScript>().fullAmount;
          }

          return returnString;
     }
     
}


