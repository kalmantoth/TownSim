using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = System.Random;

public class AnimalScript : MonoBehaviour
{

     public NavMeshAgent agent;
     public AnimalType animalType;
     public string animalName;
     public bool isFleeing;
     private float movingSpeed;
     private Vector3 lastPosition;

     private GameObject animalLeavingPoint;
     
     private Random rnd;

     private ArrayList spritesInitialRenderingOrder;



     private void Awake()
     {
          lastPosition = this.transform.position;

          //Rotating the Worker's sprite for the NavMeshAgent
          transform.Find("deer").GetComponent<Transform>().Rotate(90, 0, 0);
          
          isFleeing = false;

          animalLeavingPoint = GameObject.Find("AnimalLeavingPoint");

          rnd = new Random();

          // Setting initial rendering order of the Worker's sprites
          spritesInitialRenderingOrder = new ArrayList();
          foreach (SpriteRenderer sprite in this.gameObject.GetComponentsInChildren(typeof(SpriteRenderer)))
          {
               spritesInitialRenderingOrder.Add(sprite.sortingOrder);
          }
          modifyRenderingOrder();


     }


     // Start is called before the first frame update
     void Start()
     {
        
     }

     // Update is called once per frame
     void Update()
     {
          movingSpeed = Mathf.Lerp(movingSpeed, (transform.position - lastPosition).magnitude / Time.deltaTime, 0.75f);
          lastPosition = transform.position;

          this.GetComponent<Animator>().SetFloat("Speed", movingSpeed);


          if (calculateDistance(animalLeavingPoint.gameObject, this.gameObject) <= 2)
          {
               Debug.Log("Animal has escaped.");
               Destroy(this.gameObject);
          }

          
     }

     private void LateUpdate()
     {
          checkFacingSide();
          modifyRenderingOrder();
     }

     public void checkFacingSide()
     {
          if (isFleeing)
          {
               Vector3 pointToFace = new Vector3();
               pointToFace = animalLeavingPoint.transform.position;

               if (pointToFace.x - transform.position.x > 0) // Facing Right
               {
                    Debug.Log("Deer is facing right.");
                    this.transform.Find("deer").GetComponent<Transform>().localEulerAngles = new Vector3(90.0f, 0.0f, 0.0f);
               }
               else // Facing Left
               {
                    Debug.Log("Deer is facing left.");
                    this.transform.Find("deer").GetComponent<Transform>().localEulerAngles = new Vector3(270.0f, -180.0f, 0.0f);
                    
               }

          }
          
     }

     public GameObject createAnimalCarcass()
     {

          GameObject deerCarcass = Instantiate(Resources.Load("DeerCarcass"), this.transform.position, Quaternion.identity) as GameObject;
          deerCarcass.name = "DeerCarcass";
          deerCarcass.transform.SetParent(GameObject.Find("Resources").GetComponent<Transform>());
          Debug.Log("Deer carcass is created.");

          Destroy(this.gameObject);

          return deerCarcass;
     }
     
     public void stopMovement()
     {
          agent.SetDestination(this.gameObject.transform.position);
     }

     public GameObject successHunt(bool successOfHunting)
     {
          
          if (successOfHunting)
          {
               return createAnimalCarcass();
          }
          else
          {
               setFleeing();
               return null;
          }
          

     }

     public void modifyRenderingOrder()
     {
          /* WIP
          int i = 0;
          // Setting render sorting order by finding gameobject's global position;
          foreach (SpriteRenderer sprite in this.gameObject.GetComponentsInChildren(typeof(SpriteRenderer)))
          {
               int localRenderingOrderInSprite = -(int)spritesInitialRenderingOrder[i];
               sprite.sortingOrder = -(int)(((this.gameObject.transform.position.y) * 100) + localRenderingOrderInSprite);
               i++;
          }

           */

     }


     public void setFleeing()
     {
          isFleeing = true;
          agent.SetDestination(animalLeavingPoint.transform.position);
     }
     
     public void OnMouseDown()
     {
          GlobVars.infoPanelGameObject = this.gameObject;
     }

     public string ToString()
     {
          return animalName + "\n\tanimal type: " + animalType.ToString() + "\n\tis fleeing: " + isFleeing.ToString();
     }


     private float calculateDistance(GameObject from, GameObject to)
     {
          return Vector3.Distance(from.transform.position, to.transform.position);
     }
}
