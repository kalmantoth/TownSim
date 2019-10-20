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

     private Inventory inventory;
     
     private GameObject savedTarget;

     
     // Basic functions

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


          GlobVars.AddWorkerToWorkerList(this.gameObject);
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
          ModifyRenderingOrder();

          weaponFired = false;
          successHunt = huntingIsInProccess = false;

          workerSpriteType = rnd.Next(1, 3);
          Debug.Log("worker type number: " + workerSpriteType);

          ChangeWorkerSprite(workerSpriteType);

          inventory = new Inventory(10 , InventoryType.ALL);
          inventory.ModifyInventory(ResourceType.WOOD, 9);
          inventory.ModifyInventory(FoodType.BERRY, 10);
          inventory.ModifyInventory(FoodType.RAW_MEAT, 5);


     }

     void Update()
     {

          actionCooldown -= Time.deltaTime;

          movingSpeed = Mathf.Lerp(movingSpeed, (transform.position - lastPosition).magnitude / Time.deltaTime, 0.75f);
          lastPosition = transform.position;


          // Animator update with the worker's current status and moving speed
          this.GetComponent<Animator>().SetInteger("WorkerStatus", (int)workerStatus);
          this.GetComponent<Animator>().SetFloat("Speed", movingSpeed);

          // Reset action cooldown even on idle mode
          if (actionCooldown <= -0.74f) actionCooldown = actionCooldownInitial;

          HandleActivity();
          LongIdleCheck();

          if (GlobVars.ingameClockInFloat % 0.25f == 0)
          { 
               
          }

     }

     private void LateUpdate()
     {
          CheckFacingSide();
          ModifyRenderingOrder();

     }

     // ---------------- //
     // ---------------- //
     // ---------------- //

     // Game Mechanic functions


     

     

     public void HandleActivity()
     {
          if (target != null)
          {
               distanceFromTarget = CalculateDistance(this.gameObject, target);
               targetLayer = LayerMask.LayerToName(target.layer);
               
               
               // Worker while target nothing and idle
               if (targetLayer.Equals("Ground"))
               {
                    if (workerStatus != WorkerStatusType.IDLE && movingSpeed < 0.1f) workerStatus = WorkerStatusType.IDLE;
               }
               else if (target.GetComponent<BuildingScript>() != null && CalculateDistance(this.transform.Find("SpriteContainer/selector").gameObject, target) <= 3.5)
               {
                    if(workerStatus == WorkerStatusType.UNPACKING)
                    {
                         agent.SetDestination(this.gameObject.transform.position); // Stopping the worker agent movement.
                         this.inventory.TransferFullItemStackToInventory(target.GetComponent<BuildingScript>().inventory, target.GetComponent<BuildingScript>().inventory.inventoryType);
                         SetTarget(savedTarget);
                    }
                    else if(target.GetComponent<BuildingScript>().buildingType == BuildingType.STORAGE || target.GetComponent<BuildingScript>().buildingType == BuildingType.GRANARY)
                    {
                         this.inventory.TransferFullItemStackToInventory(target.GetComponent<BuildingScript>().inventory, target.GetComponent<BuildingScript>().inventory.inventoryType);
                         SetTargetToGround();
                    }
                    
                    
               }
               // Worker while collecting resources
               else if (targetLayer.Equals("Resources") && CalculateDistance(this.transform.Find("SpriteContainer/selector").gameObject, target) <= 3.5f)
               {
                    if(workerStatus != WorkerStatusType.WOOD_CHOPPING || workerStatus != WorkerStatusType.STONE_MINING || workerStatus != WorkerStatusType.GATHERING) { 
                         agent.SetDestination(this.gameObject.transform.position); // Stopping the worker agent movement.
                         target.GetComponent<ResourceScript>().AddToResourceUserList(this.gameObject); // Add the worker to the resource user list

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
                    }

                    CollectResourceActivity();
               }
               // Worker in hunting
               else if (targetLayer.Equals("Animals") && CalculateDistance(this.transform.Find("SpriteContainer/selector").gameObject, target) <= 15)
               {
                    if(workerStatus != WorkerStatusType.HUNTING)
                    {
                         agent.SetDestination(this.gameObject.transform.position); // Stopping the worker agent movement.
                         target.GetComponent<AnimalScript>().StopMovement();    // Stopping the animal agent movement.
                         workerStatus = WorkerStatusType.HUNTING;
                    }
                    HuntActivity();
               }

          }
     }
     
     public void UnloadInventory(BuildingType buildingType)
     {
          GameObject closestBuildingToUnload  = null;
          if(workerStatus != WorkerStatusType.UNPACKING) { 

               float minDistance = float.MaxValue;
               foreach (GameObject building in GlobVars.buildingList)
               {
                    if (building.GetComponent<BuildingScript>().buildingType == buildingType)
                    {
                         if (CalculateDistance(this.gameObject, building) < minDistance)
                         {
                              minDistance = CalculateDistance(this.gameObject, building);
                              closestBuildingToUnload = building;
                         }
                    }
               }

               if (closestBuildingToUnload == null)
               {
                    Debug.Log("No storage on the map.");
                    SetTargetToGround();
               }
               else
               {
                    savedTarget = target;
                    SetTarget(closestBuildingToUnload, WorkerStatusType.UNPACKING);
               }
          }
          
     }

     public void SetTarget(GameObject target, WorkerStatusType workerStatusType = WorkerStatusType.MOVING)
     {
          agent.SetDestination(this.gameObject.transform.position); // Stopping the worker agent movement.
          HandlePreviousTarget();
          this.previousTarget = this.target;
          this.target = target;


          if (target.GetComponent<GroundScript>() != null)  // If the target is ground then move to click position
          {
               agent.SetDestination(targetClickPosition);
          }
          else // Agent moves to the target
          {
               agent.SetDestination(target.gameObject.transform.position);
          }

          workerStatus = workerStatusType;


     }

     public void HandlePreviousTarget()
     {
          if (previousTarget != null)
          {
               if (previousTarget.GetComponent<ResourceScript>() != null)
               {
                    previousTarget.GetComponent<ResourceScript>().RemoveFromResourceUserList(this.gameObject);
               }
               else if (previousTarget.GetComponent<AnimalScript>() != null)
               {
                    previousTarget.GetComponent<AnimalScript>().SetFleeing();
               }
          }
     }

     public void CollectResourceActivity()
     {
          int collectValue = 1;

          if (actionCooldown <= 0.0f && target.GetComponent<ResourceScript>().ResourceAmountCanBeDecreased(collectValue))
          {
               actionCooldown = 2.0f;

               if(target.GetComponent<ResourceScript>().resourceType != ResourceType.NOTHING && target.GetComponent<ResourceScript>().foodType == FoodType.NOTHING)
               {
                    if (this.inventory.ModifyInventory(target.GetComponent<ResourceScript>().resourceType, collectValue) == false)
                    {
                         target.GetComponent<ResourceScript>().RemoveFromResourceUserList(this.gameObject);
                         SetTargetToGround();
                    }
                    else
                    {
                         target.GetComponent<ResourceScript>().DecreaseCurrentResourceAmount(collectValue);
                    }
               }
               else if (target.GetComponent<ResourceScript>().resourceType == ResourceType.NOTHING && target.GetComponent<ResourceScript>().foodType != FoodType.NOTHING)
               {
                    if (this.inventory.ModifyInventory(target.GetComponent<ResourceScript>().foodType, collectValue) == false)
                    {
                         target.GetComponent<ResourceScript>().RemoveFromResourceUserList(this.gameObject);
                         SetTargetToGround();
                    }
                    else
                    {
                         target.GetComponent<ResourceScript>().DecreaseCurrentResourceAmount(collectValue);
                    }
               }

               
          }

          if (inventory.IsThereFullItemStack())
          {
               if(inventory.FullItemStackItemType() == ItemType.RESOURCE)
               {
                    UnloadInventory(BuildingType.STORAGE);
               }
               else if (inventory.FullItemStackItemType() == ItemType.FOOD)
               {
                    UnloadInventory(BuildingType.GRANARY);
               }
          }
     }

     public void HuntActivity()
     {
          huntingTimer -= Time.deltaTime;

          if (!huntingIsInProccess) // Defining hunting success chance
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
          else if (huntingTimer <= 0) // End of hunting action
          {
               huntingTimer = huntingTimerInitial;

               if (successHunt)
               {
                    Debug.Log("Animal hunt has SUCCEDED.");
                    target = target.GetComponent<AnimalScript>().SuccessHunt(true);
                    agent.SetDestination(target.transform.position);
               }
               else
               {
                    Debug.Log("Animal hunt has FAILED.");
                    target.GetComponent<AnimalScript>().SuccessHunt(false);
                    target = ground;
               }

               successHunt = huntingIsInProccess = weaponFired = false;
               workerStatus = WorkerStatusType.IDLE;
          }
     }

     public void SelectWorker()
     {
          unitIsSelected = true;
          GlobVars.selectedWorkerCount++;

          transform.Find("SpriteContainer/selector").gameObject.SetActive(true);

          Debug.Log("Unit Is Selected");
     }

     public void UnselectWorker()
     {
          unitIsSelected = false;
          GlobVars.selectedWorkerCount--;

          transform.Find("SpriteContainer/selector").gameObject.SetActive(false);
     }

     public void SetTargetToGround()
     {
          agent.SetDestination(this.gameObject.transform.position); // Stopping the worker agent movement.
          target = ground;
     }

     public void OnMouseDown()
     {
          if (!unitIsSelected)
          {
               SelectWorker();
               GlobVars.infoPanelGameObject = this.gameObject;
          }
          else
          {
               UnselectWorker();
          }
     }

     // ---------------- //
     // ---------------- //
     // ---------------- //

     // Graphic modifying functions


     public void ChangeWorkerSprite(int typeNumber)
     {
          // Changing the workers sprite from the gotten type number WIP 
          Debug.Log(this.transform.Find(workerSpritePath).transform.Find("Head").ToString());
          if (typeNumber == 2)
          {
               this.transform.Find(workerSpritePath).transform.Find("Head").GetComponent<SpriteResolver>().SetCategoryAndLabel("Head", "Head2");
               this.transform.Find(workerSpritePath).transform.Find("Chest").GetComponent<SpriteResolver>().SetCategoryAndLabel("Chest", "Chest2");
          }
     }

     public void ModifyRenderingOrder()
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



     public void CheckFacingSide()
     {
          if (target != null)
          {
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

     public void LongIdleCheck()
     {
          if (idleTimer == 0.0f && workerStatus == WorkerStatusType.IDLE)
          {
               idleTimerIsCounting = true;
          }
          if (idleTimerIsCounting)
          {
               if (idleTimer >= 30)
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

     

     // ---------------- //
     // ---------------- //
     // ---------------- //

     // String manipulation functions

     public string ToString()
     {

          string workerString = "Worker" + "\n\t is selected: " + unitIsSelected.ToString() + "\n\t status: " + workerStatus.ToString() + "\n\t inventory: " + inventory.ToStringNoZeroItems();

          if(targetLayer.Equals("Ground")) {
               workerString += "\n\t target: None";
          }
          else
          {
               workerString += "\n\t target: " + this.target.name;
          }

          return workerString;
          
     }

     // ---------------- //
     // ---------------- //
     // ---------------- //

     // Assist functions

     private float CalculateDistance(GameObject from, GameObject to)
     {
          return Vector3.Distance(from.transform.position, to.transform.position);
     }
     

}
