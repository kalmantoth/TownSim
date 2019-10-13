using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.U2D.Animation;
using Random = System.Random;
using System.Collections;

public class WorkerScript : MonoBehaviour
{
     
     public NavMeshAgent agent;
     public bool unitIsSelected;
     public Vector3 targetClickPosition;
     public GameObject target, previousTarget;
     private string targetLayer;
     public float actionCooldownInitial;
     public float actionCooldown;
     public WorkerStatusType workerStatus;
     public float huntingTimer, huntingTimerInitial;
     public float idleTimer;
     public bool idleTimerIsCounting;
     private float movingSpeed;
     private Vector3 lastPosition;
     private GameObject ground;
     public float distanceFromTarget;
     private bool facingLeft;

     private Random rnd;
     private int randomEventTime;

     private ArrayList spritesInitialRenderingOrder;

     private bool weaponFired;
     private bool huntingIsInProccess;
     private bool successHunt;

     private int workerSpriteType;
     private string workerSpritePath;





     private void Awake()
     {
          workerSpritePath = ("SpriteContainer/WorkerBaseSprite");

          movingSpeed = -1000f;

          facingLeft = true;

          unitIsSelected = false;

          ground = GameObject.Find("GroundPlane");
          target = ground;
          previousTarget = target;
          distanceFromTarget = 0f;
          targetLayer = LayerMask.LayerToName(target.layer);

          actionCooldown = actionCooldownInitial = 2f;
          huntingTimer = huntingTimerInitial = 4f;
          idleTimer = 0f;


          lastPosition = this.transform.position;
          

          GlobVars.addToWorkerList(this.gameObject);
          workerStatus = WorkerStatusType.IDLE;


          //Rotating the SpriteContainer GO for the NavMeshAgent
          //transform.Find("SpriteContainer").GetComponent<Transform>().Rotate(90, 0, 0);
          /*
          this.transform.Find("SpriteContainer\tools").GetComponent<Transform>().Rotate(90, 0, 0);

          Vector3 toolsPos = this.transform.Find("SpriteContainer\tools").GetComponent<Transform>().position;
          toolsPos.z = 2.53f;
          this.transform.Find("SpriteContainer\tools").GetComponent<Transform>().position = toolsPos;
          */
          Debug.Log("New Worker spawned.");

          // Random example code for future features with worker
          rnd = new Random();
          randomEventTime = rnd.Next(10, 30);
          


          // Setting initial rendering order of the Worker's sprites
          spritesInitialRenderingOrder = new ArrayList();
          foreach (SpriteRenderer sprite in this.gameObject.GetComponentsInChildren(typeof(SpriteRenderer), true))
          {
               spritesInitialRenderingOrder.Add(sprite.sortingOrder);
               //Debug.Log("Init sprite name in list:" + sprite.gameObject.ToString());
          }
          modifyRenderingOrder();

          weaponFired = false;
          successHunt = huntingIsInProccess = false;

          workerSpriteType = rnd.Next(1, 3);
          Debug.Log("worker type number: " + workerSpriteType);

          changeWorkerSprite(workerSpriteType);
          

     }

     public void changeWorkerSprite (int typeNumber)
     {
          // Changing the workers sprite from the gotten type number WIP 
          Debug.Log(this.transform.Find(workerSpritePath).transform.Find("Head").ToString());
          if (typeNumber == 2)
          {
               this.transform.Find(workerSpritePath).transform.Find("Head").GetComponent<SpriteResolver>().SetCategoryAndLabel("Head", "Head2");
               this.transform.Find(workerSpritePath).transform.Find("Chest").GetComponent<SpriteResolver>().SetCategoryAndLabel("Chest", "Chest2");
          }
     }
     
     public void modifyRenderingOrder()
     {

          int i = 0;
          // Setting render sorting order by finding gameobject's global position;
          foreach (SpriteRenderer sprite in this.gameObject.GetComponentsInChildren(typeof(SpriteRenderer)))
          {
              
               int localRenderingOrderInSprite = -(int)spritesInitialRenderingOrder[i];
               sprite.sortingOrder = -(int)(((this.gameObject.transform.position.y) * 100) + localRenderingOrderInSprite);
               i++;
          }
          
     }


     void Update()
    {
          actionCooldown -= Time.deltaTime;
          



          movingSpeed = Mathf.Lerp(movingSpeed, (transform.position - lastPosition).magnitude / Time.deltaTime, 0.75f);
          lastPosition = transform.position;


          // Animator update with the worker's current status and moving speed
          this.GetComponent<Animator>().SetInteger("WorkerStatus", (int)workerStatus);
          this.GetComponent<Animator>().SetFloat("Speed", movingSpeed);
          
          setActivity();
          
          // Reset cooldown even on idle mode
          if (actionCooldown <= -1f)    actionCooldown = actionCooldownInitial;


          // Idle Sitting animation occurs
          if (idleTimer == 0.0f && workerStatus == WorkerStatusType.IDLE)
          {
               idleTimerIsCounting = true;
          }
          if(idleTimerIsCounting)
          {
               if(idleTimer >= 30)
               {
                    this.GetComponent<Animator>().SetBool("LongIdleTime", true);
               }

               if (workerStatus == WorkerStatusType.IDLE)
               {
                    idleTimer += Time.deltaTime;
               }
               else
               {
                    idleTimer = 0.0f;
                    idleTimerIsCounting = false;
                    this.GetComponent<Animator>().SetBool("LongIdleTime", false);
               }
          }


          

     }

     private void LateUpdate()
     {
          checkFacingSide();
          modifyRenderingOrder();


     }
     

     public void checkFacingSide()
     {
          if (target != null) { 
               Vector3 pointToFace = new Vector3();
               if (target == ground) pointToFace = targetClickPosition;
               else pointToFace = target.transform.position;

          
               if (pointToFace.x - transform.position.x > 0) // Facing Right
               {
                    this.transform.Find("SpriteContainer").GetComponent<Transform>().localEulerAngles = new Vector3(270.0f, -180.0f, 0.0f);
                    facingLeft = false;
               }
               else // Facing Left
               {
                    this.transform.Find("SpriteContainer").GetComponent<Transform>().localEulerAngles = new Vector3(90.0f, 0.0f, 0.0f);
                    facingLeft = true;
               }
          }
     }

     public void OnMouseDown()
     {
          if (!unitIsSelected)
          {
               selectWorker();
               GlobVars.infoPanelGameObject = this.gameObject;
          }
          else
          {
               unselectWorker();
          }
     }

     public void setActivity()
     {
          if (target != null) {

               distanceFromTarget = calculateDistance(this.gameObject, target);
               targetLayer = LayerMask.LayerToName(target.layer);

               // Worker while target nothing
               if (targetLayer.Equals("Ground"))
               {
                    if (movingSpeed > 0.1f) workerStatus = WorkerStatusType.MOVING;
                    else workerStatus = WorkerStatusType.IDLE;
                    
               }

               // Worker while gathering resources
               if (targetLayer.Equals("Resources") && calculateDistance(this.transform.Find("SpriteContainer/selector").gameObject, target) <= 3.5)
               {
                    agent.SetDestination(this.gameObject.transform.position);
                    switch (target.GetComponent<ResourceScript>().resourceType)
                    {
                         case ResourceType.WOOD:
                              workerStatus = WorkerStatusType.WOOD_CHOPPING;
                              break;
                         case ResourceType.STONE:
                              workerStatus = WorkerStatusType.STONE_MINING;
                              break;
                         case ResourceType.FOOD:
                              workerStatus = WorkerStatusType.GATHERING;
                              break;
                         default:
                              break;
                    }
                    target.GetComponent<ResourceScript>().addToResourceUserList(this.gameObject);
               }

               // Worker in hunting

               if (targetLayer.Equals("Animals") && calculateDistance(this.transform.Find("SpriteContainer/selector").gameObject, target) <= 15) 
               {
                    workerStatus = WorkerStatusType.HUNTING;
                    agent.SetDestination(this.gameObject.transform.position); // Stopping the worker agent movement.
                    target.GetComponent<AnimalScript>().stopMovement();    // Stopping the animal agent movement.

                    huntingTimer -= Time.deltaTime;
                    
                    if(!huntingIsInProccess) // Defining hunting success chance
                    {
                         huntingIsInProccess = true;
                         int roll = rnd.Next(0, 101);

                         Debug.Log("Animal has been engaged.");
                         Debug.Log("Hunter rolled a " + roll + ".");

                         if (roll >= 110) successHunt = true;
                         else successHunt = false;
                    }
                    
                    if (huntingTimer <= 0 && !weaponFired) // Shooting projectile
                    {
                         weaponFired = true;
                         Vector3 targetPosition = target.transform.position;
                         

                         GameObject newProjectile = Resources.Load("Arrow") as GameObject;
                         newProjectile.GetComponent<ProjectileScript>().targetPos = targetPosition;

                         if (successHunt) newProjectile.GetComponent<ProjectileScript>().missTarget = false;
                         else newProjectile.GetComponent<ProjectileScript>().missTarget = true;

                         Transform bowTransform = transform.Find("SpriteContainer/tools/bow/bow_drawn");

                         GameObject projectileInstance = Instantiate(newProjectile, bowTransform.position, Quaternion.identity);
                         projectileInstance.name = "Arrow";
                         projectileInstance.transform.SetParent(GameObject.Find("TemporaryObjects").GetComponent<Transform>());

                    }
                    else if(huntingTimer <= 0) // End of hunting action
                    {
                         huntingTimer = huntingTimerInitial;
                         
                         if (successHunt)
                         {
                              Debug.Log("Animal hunt has SUCCEDED.");
                              target = target.GetComponent<AnimalScript>().successHunt(true);
                              agent.SetDestination(target.transform.position);
                         }
                         else
                         {
                              Debug.Log("Animal hunt has FAILED.");
                              target.GetComponent<AnimalScript>().successHunt(false);
                              target = ground;
                         }

                         successHunt = huntingIsInProccess = weaponFired = false;
                         workerStatus = WorkerStatusType.IDLE;

                    }
                    
               }
               
          }
     }


     public void stopCurrentActivity()
     {
          workerStatus = WorkerStatusType.IDLE;

          if (target.GetComponent<ResourceScript>() != null)
          {
               target.GetComponent<ResourceScript>().removeFromResourceUserList(this.gameObject);
          }
          if (target.GetComponent<AnimalScript>() != null)
          {
               target.GetComponent<AnimalScript>().setFleeing();
          }
     }


     

     public bool gatherResource()
     {
          if (actionCooldown <= 0.0f)
          {
               actionCooldown = 2.0f;
               return true;
          }
          else return false;
     }


     public void selectWorker()
     {
          unitIsSelected = true;
          GlobVars.selectedWorkerCount++;

          transform.Find("SpriteContainer/selector").gameObject.SetActive(true);

          Debug.Log("Unit Is Selected");
     }

     public void unselectWorker()
     {
          unitIsSelected = false;
          GlobVars.selectedWorkerCount--;

          transform.Find("SpriteContainer/selector").gameObject.SetActive(false);
     }

     public string ToString()
     {
          if(targetLayer.Equals("Ground")) {
               return "Worker\n\tis selected: " + unitIsSelected.ToString() + "\n\tstatus: " + workerStatus.ToString() + "\n\ttarget: None";
          }
          else
          {
               return "Worker\n\tis selected: " + unitIsSelected.ToString() + "\n\tstatus: " + workerStatus.ToString() + "\n\ttarget: " + this.target.name;
          }

          //Debug
          /*
          if (targetLayer.Equals("Ground"))
          {
               return "Worker\n\tis selected: " + unitIsSelected.ToString() + "\n\tstatus: " + workerStatus.ToString() + "\n\ttarget: None" + "\n\tmoving speed: " + movingSpeed;
          }
          else
          {
               return "Worker\n\tis selected: " + unitIsSelected.ToString() + "\n\tstatus: " + workerStatus.ToString() + "\n\ttarget: " + this.target.name + " distance: " + distanceFromTarget + "\n\tmoving speed: " + movingSpeed;
          }
          */
     }

     public void setTargetToGround()
     {
          target = ground;
     }

     


     private float calculateDistance(GameObject from, GameObject to)
     {
          return Vector3.Distance(from.transform.position, to.transform.position);
     }
     

}
