using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceScript : MonoBehaviour
{
     public string resourceContainerName;
     public ResourceType resourceType;
     public FoodType foodType;
     public int fullAmount, currentAmount;
     public List<GameObject> userList = new List<GameObject>();
     private Quaternion startingPosition;

     void Start()
     {
          // Setting render sorting order by finding gameobject's global position;
          //this.gameObject.GetComponentInChildren<SpriteRenderer>().sortingOrder = -(int)(this.gameObject.transform.position.y * 100);

          /*
          if (!this.gameObject.GetComponent<AnimalScript>())
          {
               startingPosition = this.gameObject.transform.GetChild(0).gameObject.transform.rotation;   // Starting position of the Sprite (animation)
          }
          */
     }
     

     void Update()
     {
          if (userList.Count != 0)
          {
                    if( currentAmount > 0)
                    {
                    }
                    else
                    {
                         foreach (GameObject worker in userList)
                         {
                              worker.GetComponent<WorkerScript>().SetTargetToGround();
                              RemoveFromResourceUserList(worker);
                         }
                         DestroyResourceGameObject();
                    }
          }
          else
          {
          }

          ChangeSpriteBySeason();
          ChangeResourceAmountBySeason();
          ModifyRenderingOrder();
     }

     public void ModifyRenderingOrder()
     {
          this.GetComponentInChildren<SpriteRenderer>().sortingOrder = -(int)(((this.gameObject.transform.position.y)*100)-25); // The number -50 is because the worker render's correct display
          //25 = -(0.5*100-50)
          //   25    50  -0 
     }


     public void ChangeSpriteBySeason()
     {
          string seasonResourcePath = "seasons/" + GlobVars.season.ToString().ToLower() + "/";
          string resource = "";

          if (this.resourceContainerName.Equals("Oak tree"))
          {
               if(currentAmount != 0)   resource = "tree";
               else resource = "tree_empty";
          }
          else if (this.resourceContainerName.Equals("Raspberry bush"))
          {
               if (currentAmount != 0) resource = "bush";
               else resource = "bush_empty";
          }
          else if (this.resourceContainerName.Equals("Rock"))
          {
               if (currentAmount != 0) resource = "rock";
               else resource = "rock_empty";
          }

          if (this.resourceContainerName.Equals("Oak tree") || this.resourceContainerName.Equals("Raspberry bush") || this.resourceContainerName.Equals("Rock"))
          {
               this.gameObject.GetComponentInChildren<SpriteRenderer>().sprite = Resources.Load<Sprite>(seasonResourcePath + resource);
          }
               

     }
     

     public void ChangeResourceAmountBySeason()
     {
          if (this.resourceContainerName.Equals("Raspberry bush"))
          {
               if (GlobVars.firstDayOfSeason)
               {
                    if (GlobVars.season == Season.SPRING)
                    {
                         fullAmount = 25; 
                    }
                    if (GlobVars.season == Season.SUMMER)
                    {
                         fullAmount = 100;
                    }
                    if (GlobVars.season == Season.AUTUMN)
                    {
                         fullAmount = 50;
                    }
                    if (GlobVars.season == Season.WINTER)
                    {
                         fullAmount = 0;
                    }
                    currentAmount = fullAmount;
               }
               
          }
          
     }


     public bool ResourceAmountCanBeDecreased(int decreaseValue)
     {
          if(currentAmount - decreaseValue >= 0)
          {
               return true;
          }
          else
          {
               return false;
          }
     }

     public void DecreaseCurrentResourceAmount(int decreaseValue)
     {
          currentAmount -= decreaseValue;
     }


     public void AddToResourceUserList(GameObject worker)
     {
          if(!userList.Contains(worker))
          userList.Add(worker);
     }

     public void RemoveFromResourceUserList(GameObject worker)
     {
          userList.Remove(worker);
     }
     

     public void DestroyResourceGameObject()
     {
          foreach (GameObject worker in userList)
          {
               worker.GetComponent<WorkerScript>().SetTargetToGround();
               worker.GetComponent<WorkerScript>().workerStatus = WorkerStatusType.IDLE;
          }

          userList.Clear();
          if (resourceContainerName.Equals("Deer carcass")) Destroy(this.gameObject);
     }
     
     public void OnMouseDown()
     {
          GlobVars.infoPanelGameObject = this.gameObject;
     }

     public string ToString()
     {
          return resourceContainerName + "\n\tresource type: " + resourceType.ToString() + "\n\tworker's count: " + userList.Count + "\n\tresource amount: " + currentAmount + "/" + fullAmount;
     }

}
