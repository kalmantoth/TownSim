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
     
     private GameObject animalLeavingPoint;
     
     private Random rnd;

     private void Awake()
     {
          //Rotating the Worker's sprite for the NavMeshAgent
          transform.Find("Sprite").GetComponent<Transform>().Rotate(90, 0, 0);
          
          isFleeing = false;

          animalLeavingPoint = GameObject.Find("AnimalLeavingPoint");

          rnd = new Random();
     }


     // Start is called before the first frame update
     void Start()
     {
        
     }

     // Update is called once per frame
     void Update()
     {
          if (calculateDistance(animalLeavingPoint.gameObject, this.gameObject) <= 2)
          {
               Debug.Log("Animal has escaped.");
               Destroy(this.gameObject);
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

     public GameObject engageAnimal()
     {
          

          int roll = rnd.Next(0, 101);

          Debug.Log("Animal has been engaged.");
          Debug.Log("Hunter rolled a " + roll + ".");

          if (roll >= 70)
          {
               Debug.Log("Animal hunting is SUCCEDED.");
               return createAnimalCarcass();
          }
          else
          {
               Debug.Log("Animal hunting is FAILED.");
               setFleeing();
               return null;
          }
          

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
