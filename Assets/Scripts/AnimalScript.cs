using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = System.Random;


public class AnimalScript : MonoBehaviour
{

     // Target def
     public GameObject target;


     public NavMeshAgent agent;
     public string animalName;
     
     private float movingSpeed;
     private Vector3 movingVelocity;
     private Vector3 lastPosition;

     public GameObject animalSpriteContainer;

     public bool isFleeing;
     private GameObject animalLeavingPoint;
     
     private ArrayList spritesInitialRenderingOrder;

     public GameObject workerTargetPoint;



     private void Awake()
     {

          movingVelocity = new Vector3();

          workerTargetPoint = Utils.SetWorkerTargetPoint(this.gameObject);
          
          movingVelocity = (transform.position - lastPosition) / Time.deltaTime;
          lastPosition = this.transform.position;

          //Rotating the Deer's sprite for the NavMeshAgent
          animalSpriteContainer.GetComponent<Transform>().Rotate(90, 0, 0);
          
          isFleeing = false;

          animalLeavingPoint = GameObject.Find("/AnimalController/AnimalLeavingPoint");
          
          ModifyRenderingOrder();
          
     }
     
     // Update is called once per frame
     void Update()
     {
          movingSpeed = 0;
          movingSpeed = Mathf.Lerp(movingSpeed, (transform.position - lastPosition).magnitude / Time.deltaTime, 0.75f);
          lastPosition = transform.position;

          this.GetComponent<Animator>().SetFloat("Speed", movingSpeed);


          if (isFleeing && Utils.CalculateDistance(animalLeavingPoint.gameObject, this.gameObject) <= 2)
          {
               Debug.Log("Animal has escaped.");
               Destroy(this.gameObject);
          }

          
     }

     private void LateUpdate()
     {
          CheckFacingSide();
          ModifyRenderingOrder();
     }

     public void CheckFacingSide()
     {

          Vector3 pointToFace = new Vector3();
          pointToFace = target.transform.position;

          if (movingVelocity.x <= 0) // Facing Right
          {
               //Debug.Log("Deer is facing right.");
               animalSpriteContainer.GetComponent<Transform>().localEulerAngles = new Vector3(90.0f, 0.0f, 0.0f);
          }
          else // Facing Left
          {
               //Debug.Log("Deer is facing left.");
               animalSpriteContainer.GetComponent<Transform>().localEulerAngles = new Vector3(270.0f, -180.0f, 0.0f);

          }

     }

     public GameObject CreateAnimalCarcass()
     {

          GameObject deerCarcass = Instantiate(Resources.Load("DeerCarcass"), this.transform.position, Quaternion.identity) as GameObject;
          deerCarcass.name = "DeerCarcass";
          deerCarcass.transform.SetParent(GameObject.Find("Resources").GetComponent<Transform>());
          Debug.Log("Deer carcass is created.");

          Destroy(this.gameObject);

          return deerCarcass;
     }
     
     public void StopMovement()
     {
          agent.SetDestination(this.gameObject.transform.position);
     }

     public GameObject SuccessHunt(bool successOfHunting)
     {
          
          if (successOfHunting)
          {
               return CreateAnimalCarcass();
          }
          else
          {
               SetFleeing();
               return null;
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
               i++;
          }

     }


     public void SetFleeing()
     {
          isFleeing = true;
          target = animalLeavingPoint.transform.gameObject;
          agent.SetDestination(animalLeavingPoint.transform.position);
     }
     
     public void OnMouseDown()
     {
          GlobVars.infoPanelGameObject = this.gameObject;
     }

     public override string ToString()
     {
          return animalName + "\n\t Is fleeing: " + isFleeing.ToString();
     }

     
}
