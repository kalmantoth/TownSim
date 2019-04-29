using UnityEngine;
using UnityEngine.AI;
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
     private float movingSpeed;
     private Animator animator;
     private Vector3 lastPosition;
     private GameObject ground;
     public float distanceFromTarget;

     private Random rnd;
     private int randomEventTime;

     private ArrayList spritesInitialRenderingOrder;


     private void Awake()
     {
          movingSpeed = -1000f;

          unitIsSelected = false;

          ground = GameObject.Find("GroundPlane");
          target = ground;
          previousTarget = target;
          distanceFromTarget = 0f;
          targetLayer = LayerMask.LayerToName(target.layer);

          actionCooldown = actionCooldownInitial = 2f;
          huntingTimer = huntingTimerInitial = 5f;

          animator = this.transform.Find("PeasantMaleSprite").GetComponent<Animator>();

          lastPosition = this.transform.position;
          

          GlobVars.addToWorkerList(this.gameObject);
          workerStatus = WorkerStatusType.IDLE;

          
          //Rotating the Worker's sprite for the NavMeshAgent
          //this.transform.Find("PeasantMaleSprite").GetComponent<Transform>().Rotate(90,0,0);
          Debug.Log(this.transform.Find("PeasantMaleSprite").GetComponent<Transform>().ToString());

          Debug.Log("New Worker spawned.");

          rnd = new Random();
          randomEventTime = rnd.Next(10, 30);
          Debug.Log("Time of the NOHEAD anim play is: " + randomEventTime);


          // Setting initial rendering order of the Worker's sprites
          spritesInitialRenderingOrder = new ArrayList();
          foreach (SpriteRenderer sprite in this.gameObject.GetComponentsInChildren(typeof(SpriteRenderer)))
          {
               spritesInitialRenderingOrder.Add(sprite.sortingOrder);
          }
          //Debug.Log("Sprites count: " + spritesInitialRenderingOrder.Count);

          modifyRenderingOrder();
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

          

          animator.SetInteger("WorkerStatus", (int)workerStatus);
          animator.SetFloat("Speed", movingSpeed);
          
          setActivity();
          
          // Reset cooldown even on idle mode
          if (actionCooldown <= -1f)    actionCooldown = actionCooldownInitial;

          
          
     }

     private void LateUpdate()
     {
          checkFacingSide();
          modifyRenderingOrder();
     }

     public void checkFacingSide()
     {
          Vector3 pointToFace = new Vector3();
          if (target == ground) pointToFace = targetClickPosition;
          else pointToFace = target.transform.position;

          if (pointToFace.x - transform.position.x > 0)
          {
               this.transform.Find("PeasantMaleSprite").GetComponent<Transform>().localEulerAngles = new Vector3(270.0f, -180.0f, 0.0f);
          }
          else
          {
               this.transform.Find("PeasantMaleSprite").GetComponent<Transform>().localEulerAngles = new Vector3(90.0f, 0.0f, 0.0f);
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


               if(targetLayer.Equals("Ground"))
               {
                    if (movingSpeed > 0.1f) workerStatus = WorkerStatusType.MOVING;
                    else workerStatus = WorkerStatusType.IDLE;

                    //whatTheHellIHaveNoHead();
               }

               if (targetLayer.Equals("Resources") && calculateDistance(this.gameObject, target) <= 0.75)
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
               if (targetLayer.Equals("Animals") && calculateDistance(this.gameObject, target) <= 3)
               {
                    
                    workerStatus = WorkerStatusType.HUNTING;
                    agent.SetDestination(this.gameObject.transform.position); // Stopping the worker agent movement.
                    target.GetComponent<AnimalScript>().stopMovement();    // Stopping the animal agent movement.

                    huntingTimer -= Time.deltaTime;


                    if(huntingTimer <= 0)
                    {
                         huntingTimer = huntingTimerInitial;
                         target = target.GetComponent<AnimalScript>().engageAnimal();
                         if (target == null)      // On unsuccessful hunting
                         {
                              target = ground;
                         }
                         else
                         {
                              agent.SetDestination(target.transform.position);
                         }
                         
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


     private void whatTheHellIHaveNoHead()
     {
          if (GlobVars.ingameClock == randomEventTime && this.workerStatus == WorkerStatusType.IDLE)
          {
               Debug.Log("I HAVE NO HEAD!!!");
               animator.SetTrigger("NoHead");
               randomEventTime += rnd.Next(20, 60);
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
          
          transform.Find("PeasantMaleSprite/selector/WorkerSelector").GetComponent<SpriteRenderer>().enabled = true;

          Debug.Log("Unit Is Selected");
     }

     public void unselectWorker()
     {
          unitIsSelected = false;
          GlobVars.selectedWorkerCount--;

          transform.Find("PeasantMaleSprite/selector/WorkerSelector").GetComponent<SpriteRenderer>().enabled = false;
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
