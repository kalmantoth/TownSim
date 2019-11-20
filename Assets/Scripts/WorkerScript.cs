using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.U2D.Animation;
using Random = System.Random;
using System.Collections;

public enum WorkerStatusType { IDLE = 1, MOVING = 2, WOOD_CHOPPING = 3, STONE_MINING = 4, FISHING = 5, FARMING = 6, CONSTRUCING = 7, GATHERING = 8, HUNTING = 9, ITEMDEPOSIT = 10, ITEMDRAW = 11, COOKING = 12, BAKING = 13 }

public class WorkerScript : MonoBehaviour
{
     // Target defs
     public Vector3 targetClickPosition;
     public GameObject target, previousTarget;
     private string targetLayer;
     private GameObject savedTarget;
     

     // Timer defs
     public float actionCooldownInitial;
     public float actionCooldown;
     public float cooldownModifier;
     private float huntingTimer, huntingTimerInitial;
     private float idleTimer;
     private bool idleTimerIsCounting;
     
     // Hunting defs
     private bool weaponFired;
     private bool huntingIsInProccess;
     private bool successHunt;

     public NavMeshAgent agent;
     public bool unitIsSelected;

     public WorkerStatusType workerStatus;

     private float movingSpeed;
     private Vector3 movingVelocity;
     private Vector3 lastPosition;
     private GameObject ground;
     public float distanceFromTarget;

     private Random rnd;
     private int randomEventTime;

     private ArrayList spritesInitialRenderingOrder;
     private int workerSpriteType;
     private string workerSpritePath;

     public Inventory inventory;
     private GameObject selector;

     public ItemType drawItemType;
     public int drawItemQuantity;
     public ItemType depositItemType;
     public int depositItemQuantity;

     private Animator workerAnimator;


     public bool workerIsHidden;
     
     // Basic functions
     private void Awake()
     {
          workerSpritePath = ("SpriteContainer/WorkerBaseSprite");
          workerIsHidden = false;

          movingSpeed = -1000f;
          movingVelocity = new Vector3();

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


          GlobVars.AddWorkerToWorkers(this.gameObject);
          workerStatus = WorkerStatusType.IDLE;


          //Rotating the SpriteContainer GO for the NavMeshAgent
          this.transform.Find("SpriteContainer").GetComponent<Transform>().localEulerAngles = new Vector3(90.0f, 0.0f, 0.0f);

          Debug.Log("New Worker spawned.");

          // Random example code for future features for worker
          rnd = new Random();
          randomEventTime = rnd.Next(10, 30);



          // Setting initial rendering order of the Worker's sprites
          ModifyRenderingOrder();

          weaponFired = false;
          successHunt = huntingIsInProccess = false;

          workerSpriteType = rnd.Next(1, 3);
          Debug.Log("worker type number: " + workerSpriteType);

          ChangeWorkerSprite(workerSpriteType);

          inventory = new Inventory(10 , InventoryType.ALL);
          inventory.ModifyInventory(ItemType.BERRY, 0);

          selector = this.transform.Find("SpriteContainer/selector").gameObject;

          cooldownModifier = 1f;

          drawItemType = depositItemType = ItemType.NOTHING;
          drawItemQuantity = depositItemQuantity = 0;

          workerAnimator = this.GetComponent<Animator>();
     }
     

     void Update()
     {
          actionCooldown -= Time.deltaTime;

          movingSpeed = Mathf.Lerp(movingSpeed, (transform.position - lastPosition).magnitude / Time.deltaTime, 0.75f);
          movingVelocity = (transform.position - lastPosition) / Time.deltaTime;
          lastPosition = transform.position;


          // Animator update with the worker's current status and moving speed
          workerAnimator.SetInteger("WorkerStatus", (int)workerStatus);
          workerAnimator.SetFloat("Speed", movingSpeed);

          // Reset action cooldown even on idle mode
          if (actionCooldown <= -0.25f) actionCooldown = actionCooldownInitial;

          HandleActivities();
          LongIdleCheck();
          // if (GlobVars.ingameClockInFloat % 0.25f == 0)

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
     

     public void HandleActivities()
     {
          if (target != null)
          {
               distanceFromTarget = Utils.CalculateDistance(this.gameObject, target);
               targetLayer = LayerMask.LayerToName(target.layer);
               

               
               // Worker while target nothing and idle
               if (targetLayer.Equals("Ground"))
               {
                    if(workerStatus != WorkerStatusType.MOVING && movingSpeed > 0.1f ) workerStatus = WorkerStatusType.MOVING;
                    else if (workerStatus != WorkerStatusType.IDLE && movingSpeed < 0.1f) workerStatus = WorkerStatusType.IDLE;
               }
               // Worker activity with buildings
               else if (target.GetComponent<BuildingScript>() != null && Utils.CalculateDistance(selector, target.GetComponent<BuildingScript>().workerTargetPoint) <= 3.5)
               {
                    // Worker unloading it's full item stacks to the correct building then returning to it's former activity
                    if (workerStatus == WorkerStatusType.ITEMDEPOSIT)
                    {
                         if (depositItemType != ItemType.NOTHING)
                         {
                              target.GetComponent<BuildingScript>().inventory.ModifyInventory(depositItemType, depositItemQuantity);
                              inventory.ModifyInventory(depositItemType, -depositItemQuantity);
                              depositItemType = ItemType.NOTHING;
                              depositItemQuantity = 0;
                         }
                         else
                         {
                              this.inventory.TransferFullItemStackToInventory(target.GetComponent<BuildingScript>().inventory, target.GetComponent<BuildingScript>().inventory.inventoryType);
                         }

                         agent.SetDestination(this.gameObject.transform.position); // Stopping the worker agent movement.
                         Debug.Log("Going back to " + savedTarget.gameObject.name);
                         SetTarget(savedTarget);
                         savedTarget = null;
                         //nem megy vissza dógozni

                    }
                    // Worker load the wanted food type and goes back to the active building
                    else if (workerStatus == WorkerStatusType.ITEMDRAW )
                    {
                         agent.SetDestination(this.gameObject.transform.position); // Stopping the worker agent movement.
                         target.GetComponent<BuildingScript>().inventory.ModifyInventory(drawItemType, -drawItemQuantity);
                         inventory.ModifyInventory(drawItemType, drawItemQuantity);
                         SetTarget(savedTarget);
                         savedTarget = null;
                         drawItemType = ItemType.NOTHING;
                         drawItemQuantity = 0;
                    }
                    // Worker manually unload it's full item stacks
                    else if (target.GetComponent<BuildingScript>().buildingType == BuildingType.STORAGE || target.GetComponent<BuildingScript>().buildingType == BuildingType.GRANARY || target.GetComponent<BuildingScript>().buildingType == BuildingType.TOWNHALL)
                    {
                         Debug.Log("Unloaded all the items to following building's inventory: " + target.gameObject.name);
                         this.inventory.TransferFullItemStackToInventory(target.GetComponent<BuildingScript>().inventory, target.GetComponent<BuildingScript>().inventory.inventoryType);
                         SetTargetToGround();
                    }
                    // Worker while cooking at campfire
                    else if (target.GetComponent<BuildingScript>().buildingType == BuildingType.CAMPFIRE)
                    {
                         if (workerStatus != WorkerStatusType.COOKING)
                         {
                              agent.SetDestination(this.gameObject.transform.position); // Stopping the worker agent movement.
                              workerStatus = WorkerStatusType.COOKING;
                         }
                         
                         CookActivity();
                    }
                    else if (target.GetComponent<BuildingScript>().buildingType == BuildingType.FARM)
                    {
                         if (workerStatus != WorkerStatusType.FARMING)
                         {
                              agent.SetDestination(this.gameObject.transform.position); // Stopping the worker agent movement.
                              workerStatus = WorkerStatusType.FARMING;
                         }
                         
                         FarmActivity();
                         //CookActivity();
                    }
                    // Worker while at the millbakery
                    else if (target.GetComponent<BuildingScript>().buildingType == BuildingType.MILLBAKERY)
                    {
                         if (workerStatus != WorkerStatusType.BAKING)
                         {
                              agent.SetDestination(this.gameObject.transform.position); // Stopping the worker agent movement.
                              workerStatus = WorkerStatusType.BAKING;
                         }

                         BakingActivity();
                    }

               }
               // Worker while collecting resources
               else if (targetLayer.Equals("Resources") && Utils.CalculateDistance(selector, target.GetComponent<ResourceScript>().workerTargetPoint) <= 3.5f)
               {
                    if(workerStatus != WorkerStatusType.WOOD_CHOPPING || workerStatus != WorkerStatusType.STONE_MINING || workerStatus != WorkerStatusType.GATHERING || workerStatus != WorkerStatusType.FARMING) { 
                         agent.SetDestination(this.gameObject.transform.position); // Stopping the worker agent movement.
                         target.GetComponent<ResourceScript>().AddToResourceUserList(this.gameObject); // Add the worker to the resource user list

                         switch (target.GetComponent<ResourceScript>().itemType)
                         {
                              case ItemType.WOOD:
                                   workerStatus = WorkerStatusType.WOOD_CHOPPING;
                                   break;
                              case ItemType.STONE:
                                   workerStatus = WorkerStatusType.STONE_MINING;
                                   break;
                              case ItemType.BERRY:
                              case ItemType.RAW_MEAT:
                              case ItemType.RAW_FISH:
                              case ItemType.NOTHING:
                                   workerStatus = WorkerStatusType.GATHERING;
                                   break;
                              case ItemType.WHEAT:
                              case ItemType.POTATO:
                                   workerStatus = WorkerStatusType.FARMING;
                                   break;
                              default:
                                   break;
                         }
                    }

                    CollectResourceActivity();
               }
               // Worker while hunting
               else if (targetLayer.Equals("Animals") && Utils.CalculateDistance(selector, target.GetComponent<AnimalScript>().workerTargetPoint) <= 15)
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
     
     public void FarmActivity()
     {
          if (workerStatus == WorkerStatusType.FARMING)
          {

               if (target.GetComponent<FarmScript>().farmIsReadyToHarvest)
               {
                    target.GetComponent<ResourceScript>().AddToResourceUserList(this.gameObject);
                    CollectResourceActivity();

               }


               if (target.GetComponent<FarmScript>().farmIsReadyToGrowPlants)
               {
                    Debug.Log("The farm is growing now.");
                    target.GetComponent<FarmScript>().StartPlantGrowProgress();
               }

               if (target.GetComponent<FarmScript>().IsTheFarmNotAvailable()) SetTargetToGround();
          }
     }


     public void CookActivity()
     {
          if(workerStatus == WorkerStatusType.COOKING)
          {
               ItemType campfireSelectedFoodType = target.GetComponent<BuildingScript>().selectedActiveItemType;

               // If the worker's inventory is full of cooked food then automatically unload it
               if (inventory.IsItemTypeFull(inventory.GetFoodItemTypeWhich(true)))
               {
                    UnloadInventory(inventory.GetFoodItemTypeWhich(true));
               }

               // Checking if no raw food found at the inventories on the map
               if (GlobVars.RAW_FOOD == 0 && !inventory.IsThereFoodWhich(true) && !inventory.IsThereFoodWhich(false))
               {
                    Debug.Log("Could not found any raw food to cook.");
                    SetTargetToGround();
               }
               else if (campfireSelectedFoodType == ItemType.ANYTHING)
               {
                    if (!inventory.IsThereFoodWhich(false))
                    {
                         target.GetComponent<BuildingScript>().isBuildingInUse = false;
                         if(GlobVars.FirstEdibleFood() != ItemType.NOTHING)
                         {
                              LoadInventory(GlobVars.FirstEdibleFood());
                         }
                         
                    }
                    else
                    {
                         target.GetComponent<BuildingScript>().isBuildingInUse = true;
                         if (actionCooldown <= 0.0f)
                         {
                              actionCooldown = actionCooldownInitial * cooldownModifier;
                              if (inventory.GetFirstFoodWhich(false) != ItemType.NOTHING)
                              {
                                   ItemType foodToCook = inventory.GetFirstFoodWhich(false);
                                   inventory.ModifyInventory(foodToCook, -1);
                                   inventory.ModifyInventory(Utils.SpecifyDonePairOfUndoneFood(foodToCook), +1);
                              }
                         }
                         
                    }
               }
               else if (campfireSelectedFoodType == ItemType.POTATO || campfireSelectedFoodType == ItemType.RAW_MEAT || campfireSelectedFoodType == ItemType.RAW_FISH)
               {
                    ItemType foodToCook = target.GetComponent<BuildingScript>().selectedActiveItemType;
                    if (inventory.GetItemQuantity(target.GetComponent<BuildingScript>().selectedActiveItemType) <= 0)
                    {
                         target.GetComponent<BuildingScript>().isBuildingInUse = false;
                         LoadInventory(foodToCook);
                    }
                    else
                    {
                         target.GetComponent<BuildingScript>().isBuildingInUse = true;

                         if (actionCooldown <= 0.0f)
                         {
                              actionCooldown = actionCooldownInitial * cooldownModifier;
                              if (inventory.GetFirstFoodWhich(false) != ItemType.NOTHING)
                              {
                                   inventory.ModifyInventory(foodToCook, -1);
                                   inventory.ModifyInventory(Utils.SpecifyDonePairOfUndoneFood(foodToCook), +1);
                              }
                         }

                    }
               }
               
          }
          
     }

     public void BakingActivity()
     {
          if (workerStatus == WorkerStatusType.BAKING)
          {
               ItemType baseMaterial = target.GetComponent<BuildingScript>().selectedActiveItemType;
               ItemType doneFood = Utils.SpecifyDonePairOfUndoneFood(baseMaterial);

               if (inventory.IsItemTypeFull(doneFood))
               {
                    UnloadInventory(doneFood);
               }

               if (GlobVars.GetGlobalStoredItemQuantity(baseMaterial) == 0 && inventory.GetItemCurrentQuantity(baseMaterial) <= 0 && inventory.GetItemCurrentQuantity(doneFood) <= 0)
               {
                    Debug.Log("Could not found any " + baseMaterial + " to bake.");
                    SetTargetToGround();
               }

               
               if (inventory.GetItemQuantity(baseMaterial) <= 0 && inventory.GetItemQuantity(doneFood) == 0)
               {
                    target.GetComponent<BuildingScript>().RemoveIndoorWorker(this.gameObject);
                    workerIsHidden = false;
                    LoadInventory(baseMaterial);
               }
               else
               {
                    target.GetComponent<BuildingScript>().AddIndoorWorker(this.gameObject);
                    workerIsHidden = true;
                    UnselectWorker();

                    if (actionCooldown <= 0.0f)
                    {
                         actionCooldown = actionCooldownInitial * cooldownModifier;
                         if (inventory.GetItemQuantity(baseMaterial) > 0)
                         {
                              inventory.ModifyInventory(doneFood, +1);
                              inventory.ModifyInventory(baseMaterial, -1);
                         }
                    }

               }

          }

     }


     public void SetTarget(GameObject target, WorkerStatusType workerStatusType = WorkerStatusType.MOVING)
     {
          agent.SetDestination(this.gameObject.transform.position); // Stopping the worker agent movement.
          this.previousTarget = this.target;
          HandlePreviousTarget();
          this.target = target;
          
          if (target.GetComponent<GroundScript>() != null)  // If the target is ground then move to click position
          {
               agent.SetDestination(targetClickPosition);
          }
          else // Agent moves to the target
          {
               if(target.transform.Find("WorkerTargetPoint").gameObject != null)
               {
                    agent.SetDestination(target.transform.Find("WorkerTargetPoint").gameObject.transform.position);
               }
               else
               {
                    agent.SetDestination(target.gameObject.transform.position);
               }

               
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
               else if (previousTarget.GetComponent<BuildingScript>() != null)
               {
                    previousTarget.GetComponent<BuildingScript>().isBuildingInUse = false;
               }
          }
     }

     public void CollectResourceActivity()
     {
          int collectValue = 1;

          ResourceScript resourceTarget = target.GetComponent<ResourceScript>();



          if (actionCooldown <= 0.0f && resourceTarget.ResourceAmountCanBeDecreased(collectValue))
          {
               actionCooldown = actionCooldownInitial * cooldownModifier;

               if(resourceTarget.itemType != ItemType.NOTHING)
               {
                    if (this.inventory.ModifyInventory(resourceTarget.itemType, collectValue) == false)
                    {
                         resourceTarget.RemoveFromResourceUserList(this.gameObject);
                         SetTargetToGround();
                    }
                    else
                    {
                         resourceTarget.DecreaseCurrentResourceAmount(collectValue);
                    }
               }
          }

          if (this.inventory.IsItemTypeFull(resourceTarget.itemType))   // If the resource that I'm collecting is full
          {
               UnloadInventory(resourceTarget.itemType);
          }
     }

     public void LoadInventory(ItemType itemType = ItemType.NOTHING)
     {
          GameObject closestBuildingToLoad = null;
          if (workerStatus != WorkerStatusType.ITEMDRAW)
          {
               float minDistance = float.MaxValue;
               int loadQuantity = 0;
               foreach (GameObject building in GlobVars.storageBuildings)
               {
                    bool isBuildingLoadAble = false;
                    if (Utils.SpecifyInventoryType(itemType) == building.GetComponent<BuildingScript>().inventory.inventoryType || building.GetComponent<BuildingScript>().inventory.inventoryType == InventoryType.ALL)
                    {
                         if (building.GetComponent<BuildingScript>().inventory.CanModifyItemQuantity(itemType, -this.inventory.GetMaxItemQuantity(itemType)))
                         {
                              loadQuantity = this.inventory.GetMaxItemQuantity(itemType);
                              isBuildingLoadAble = true;
                         }
                         else if (building.GetComponent<BuildingScript>().inventory.GetItemCurrentQuantity(itemType) != 0 && building.GetComponent<BuildingScript>().inventory.CanModifyItemQuantity(itemType, building.GetComponent<BuildingScript>().inventory.GetItemCurrentQuantity(itemType)))
                         {
                              loadQuantity = building.GetComponent<BuildingScript>().inventory.GetItemCurrentQuantity(itemType);
                              isBuildingLoadAble = true;
                         }

                         if (isBuildingLoadAble)
                         {
                              if (Utils.CalculateDistance(this.gameObject, building) < minDistance)
                              {
                                   minDistance = Utils.CalculateDistance(this.gameObject, building);
                                   closestBuildingToLoad = building;
                              }
                         }

                    }
               }

               if (closestBuildingToLoad == null)
               {
                    Debug.Log("No building on the map that can contain ENOUGH " + itemType.ToString());
                    SetTargetToGround();
               }
               else
               {
                    savedTarget = target;
                    if (itemType != ItemType.NOTHING)
                    {
                         drawItemType = itemType;
                         drawItemQuantity = loadQuantity;
                    }

                    SetTarget(closestBuildingToLoad, WorkerStatusType.ITEMDRAW);
               }
          }

     }
     
     
     public void UnloadInventory(ItemType itemType = ItemType.NOTHING)
     {
          GameObject closestBuildingToUnload = null;
          if (workerStatus != WorkerStatusType.ITEMDEPOSIT)
          {
               float minDistance = float.MaxValue;
               int unloadQuantity = 0;
               foreach (GameObject building in GlobVars.storageBuildings)
               {
                    bool isBuildingUnloadAble = false;
                    if (Utils.SpecifyInventoryType(itemType) == building.GetComponent<BuildingScript>().inventory.inventoryType || building.GetComponent<BuildingScript>().inventory.inventoryType == InventoryType.ALL)
                    {
                         if (building.GetComponent<BuildingScript>().inventory.CanModifyItemQuantity(itemType, this.inventory.GetItemQuantity(itemType)))
                         {
                              unloadQuantity = this.inventory.GetItemQuantity(itemType);
                              isBuildingUnloadAble = true;
                         }
                         else if (building.GetComponent<BuildingScript>().inventory.GetItemQuantityToReachMax(itemType) != 0 && building.GetComponent<BuildingScript>().inventory.CanModifyItemQuantity(itemType, building.GetComponent<BuildingScript>().inventory.GetItemQuantityToReachMax(itemType)))
                         {
                              unloadQuantity = building.GetComponent<BuildingScript>().inventory.GetItemQuantityToReachMax(itemType);
                              isBuildingUnloadAble = true;
                         }

                         if (isBuildingUnloadAble)
                         {
                              if (Utils.CalculateDistance(this.gameObject, building) < minDistance)
                              {
                                   minDistance = Utils.CalculateDistance(this.gameObject, building);
                                   closestBuildingToUnload = building;
                              }
                         }
                         

                    }
               }

               if (closestBuildingToUnload == null)
               {
                    Debug.Log("No building on the map that can contain MORE " + itemType.ToString());
                    SetTargetToGround();
               }
               else
               {
                    savedTarget = target;
                    if (itemType != ItemType.NOTHING)
                    {
                         depositItemQuantity = unloadQuantity;
                         depositItemType = itemType;
                    }

                    SetTarget(closestBuildingToUnload, WorkerStatusType.ITEMDEPOSIT);
               }
          }
     }

     

     public void SetCooldownModifier (float modifierValue)
     {
          cooldownModifier = modifierValue;
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

               if (roll >= 50) successHunt = true;
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
          GlobVars.AddWorkerToSelectedWorkers(this.gameObject);

          transform.Find("SpriteContainer/selector").gameObject.SetActive(true);

          Debug.Log("Unit Is Selected");
     }

     public void UnselectWorker()
     {
          unitIsSelected = false;
          GlobVars.RemoveWorkerFromSelectedWorkers(this.gameObject);

          transform.Find("SpriteContainer/selector").gameObject.SetActive(false);
     }

     public void SetTargetToGround()
     {
          agent.SetDestination(this.gameObject.transform.position); // Stopping the worker agent movement.
          target = ground;
          workerStatus = WorkerStatusType.IDLE;
     }

     

     public void OnMouseDown()
     {
          if (Input.GetMouseButtonDown(0) && !workerIsHidden)
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

          if (spritesInitialRenderingOrder == null)
          {
               spritesInitialRenderingOrder = new ArrayList();
               foreach (SpriteRenderer sprite in this.gameObject.GetComponentsInChildren(typeof(SpriteRenderer), true))
               {
                    spritesInitialRenderingOrder.Add(sprite.sortingOrder);
                    //Debug.Log("Init sprite name in list:" + sprite.gameObject.ToString());
               }
          }

          int i = 0;
          // Setting render sorting order by finding gameobject's global position;
          foreach (SpriteRenderer sprite in this.gameObject.GetComponentsInChildren(typeof(SpriteRenderer)))
          {
               int localRenderingOrderInSprite = -(int)spritesInitialRenderingOrder[i];
               sprite.sortingOrder = -(int)(((this.gameObject.transform.position.y) * 100) + localRenderingOrderInSprite);
               // Modified rendering order to the selector
               if(sprite.name == "selector") sprite.sortingOrder = -(int)(((this.gameObject.transform.position.y+10) * 100) + localRenderingOrderInSprite);
               i++;
          }

     }
     

     public void CheckFacingSide()
     {
          if (target != null)
          {
               if (movingSpeed > 0.1f) {
                    if (movingVelocity.x >= 0) // Facing Right
                    {
                         this.transform.Find("SpriteContainer").GetComponent<Transform>().localEulerAngles = new Vector3(270.0f, -180.0f, 0.0f);
                    }
                    else // Facing Left
                    {
                         this.transform.Find("SpriteContainer").GetComponent<Transform>().localEulerAngles = new Vector3(90.0f, 0.0f, 0.0f);
                    }
               }
               else
               {
                    if (target.gameObject.transform.position.x >= this.transform.position.x) // Facing Right
                    {
                         this.transform.Find("SpriteContainer").GetComponent<Transform>().localEulerAngles = new Vector3(270.0f, -180.0f, 0.0f);
                    }
                    else // Facing Left
                    {
                         this.transform.Find("SpriteContainer").GetComponent<Transform>().localEulerAngles = new Vector3(90.0f, 0.0f, 0.0f);
                    }
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

     public override string ToString()
     {

          string workerString = "Worker" + "\n\t is selected: " + unitIsSelected.ToString() + "\n\t status: " + workerStatus.ToString() + "\n\t inventory: " + inventory.ToString();

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

     
}
