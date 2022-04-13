using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceScript : MonoBehaviour
{
     public string resourceContainerName;
     public ItemType itemType;
     public int fullAmount, currentAmount;
     public List<GameObject> userList = new List<GameObject>();
     public GameObject workerTargetPoint;

     private SpriteRenderer resourceSpriteRenderer;

     void Awake()
     {

          workerTargetPoint = Utils.SetWorkerTargetPoint(this.gameObject);

          resourceSpriteRenderer = this.GetComponentInChildren<SpriteRenderer>();
          
          ModifyRenderingOrder();
     }
     

     void Update()
     {
          if (userList.Count != 0)
          {
               if( currentAmount <= 0)
               {
                    foreach (GameObject worker in userList)
                    {
                         worker.GetComponent<WorkerScript>().SetTargetToGround();
                         RemoveFromResourceUserList(worker);
                    }
                    DestroyResourceGameObject();
               }
          }
          
     }

     void LateUpdate()
     {
          if (GlobVars.firstDayOfSeason)
          {
               ChangeSpriteBySeason();
               ChangeResourceAmountBySeason();
          }
          
     }

     public void ModifyRenderingOrder()
     {
          resourceSpriteRenderer.sortingOrder = -(int)(((this.gameObject.transform.position.y)*100)-25);
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
               resourceSpriteRenderer.sprite = Resources.Load<Sprite>(seasonResourcePath + resource);
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
          if(!userList.Contains(worker)) userList.Add(worker);
     }

     public void RemoveFromResourceUserList(GameObject worker)
     {
          Debug.Log("Worker removed from user list.");
          userList.Remove(worker);
     }
     
     public bool IsResourceEmpty()
     {
          if(currentAmount <= 0)
          {
               return true;
          }
          return false;
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

     public override string ToString()
     {
          return resourceContainerName + "\n\t Resource type: " + Utils.UppercaseFirst(itemType.ToString()) + "\n\t Worker count: " + userList.Count + "\n\t Resource amount: " + currentAmount + "/" + fullAmount;
     }

}
