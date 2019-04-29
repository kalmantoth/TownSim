using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceScript : MonoBehaviour
{
     public string resourceContainerName;
     public ResourceType resourceType;
     public int fullAmount =  200 , currentAmount =  200;
     public List<GameObject> userList = new List<GameObject>();
     private Quaternion startingPosition;

     void Start()
     {
          // Setting render sorting order by finding gameobject's global position;
          this.gameObject.GetComponentInChildren<SpriteRenderer>().sortingOrder = -(int)(this.gameObject.transform.position.y * 100);


          if (!this.gameObject.GetComponent<AnimalScript>())
          {
               startingPosition = this.gameObject.transform.GetChild(0).gameObject.transform.rotation;   // Starting position of the Sprite (animation)
          }
          
     }

     void Update()
     {
          if (userList.Count != 0)
          {
               foreach (GameObject worker in userList)
               {
                    if( currentAmount > 0)
                    {
                         if (worker.GetComponent<WorkerScript>().gatherResource()) produceResource();
                    }
                    else
                    {
                         destroyResourceGameObject();
                         break;
                         
                    }
               }
               //startProducingAnimation();
          }
          else
          {
               //stopProducingAnimation();
          }
     }

     public void addToResourceUserList(GameObject worker)
     {
          if(!userList.Contains(worker))
          userList.Add(worker);
     }

     public void removeFromResourceUserList(GameObject worker)
     {
          userList.Remove(worker);
     }

     public void destroyResourceGameObject()
     {
          foreach (GameObject worker in userList)
          {
               worker.GetComponent<WorkerScript>().setTargetToGround();
               worker.GetComponent<WorkerScript>().workerStatus = WorkerStatusType.IDLE;

               Destroy(this.gameObject);
          }
     }

     public void produceResource()
     {
          currentAmount--;

          switch (resourceType)
          {
               case ResourceType.WOOD:
                    GlobVars.WOOD++;
                    break;
               case ResourceType.STONE:
                    GlobVars.STONE++;
                    break;
               case ResourceType.FOOD:
                    if (resourceContainerName.Equals("Deer carcass")) GlobVars.FOOD+= 5;
                    else GlobVars.FOOD++;
                    break;
               default:
                    break;
          }
     }

     public void startProducingAnimation()
     {
          this.gameObject.transform.GetChild(0).gameObject.transform.RotateAround(this.gameObject.transform.GetChild(0).gameObject.transform.position, this.gameObject.transform.GetChild(0).gameObject.transform.forward, 5);
     }

     public void stopProducingAnimation()
     {
          // Set the Sprite to the starting position
          this.gameObject.transform.GetChild(0).gameObject.transform.rotation = startingPosition;  
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
