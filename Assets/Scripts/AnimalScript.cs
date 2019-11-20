using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = System.Random;

public enum AnimalType { DEER }

public class AnimalScript : MonoBehaviour
{

     // Target def
     public GameObject target;


     public NavMeshAgent agent;
     public AnimalType animalType;
     public string animalName;
     public bool isFleeing;
     private float movingSpeed;
     private Vector3 movingVelocity;
     private Vector3 lastPosition;


     private GameObject animalLeavingPoint;
     
     private Random rnd;

     private ArrayList spritesInitialRenderingOrder;

     public GameObject workerTargetPoint;



     private void Awake()
     {

          movingVelocity = new Vector3();

          workerTargetPoint = Utils.SetWorkerTargetPoint(this.gameObject);

          movingVelocity = (transform.position - lastPosition) / Time.deltaTime;
          lastPosition = this.transform.position;

          //Rotating the Deer's sprite for the NavMeshAgent
          transform.Find("deer").GetComponent<Transform>().Rotate(90, 0, 0);
          
          isFleeing = false;

          animalLeavingPoint = GameObject.Find("/AnimalController/AnimalLeavingPoint");

          rnd = new Random();

          // Setting initial rendering order of the Worker's sprites
          spritesInitialRenderingOrder = new ArrayList();
          foreach (SpriteRenderer sprite in this.gameObject.GetComponentsInChildren(typeof(SpriteRenderer)))
          {
               spritesInitialRenderingOrder.Add(sprite.sortingOrder);
          }
          ModifyRenderingOrder();


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


          if (Utils.CalculateDistance(animalLeavingPoint.gameObject, this.gameObject) <= 2)
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
               this.transform.Find("deer").GetComponent<Transform>().localEulerAngles = new Vector3(90.0f, 0.0f, 0.0f);
          }
          else // Facing Left
          {
               //Debug.Log("Deer is facing left.");
               this.transform.Find("deer").GetComponent<Transform>().localEulerAngles = new Vector3(270.0f, -180.0f, 0.0f);

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
          return animalName + "\n\tanimal type: " + animalType.ToString() + "\n\tis fleeing: " + isFleeing.ToString();
     }

     
}
