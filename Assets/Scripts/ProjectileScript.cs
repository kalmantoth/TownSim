using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;


public class ProjectileScript : MonoBehaviour
{

     public Vector3 targetPos = new Vector3(0,0,0);
     public float speed;
     private Rigidbody2D rb;
     private Random rnd;
     public bool missTarget;


     private Vector3 moveDirection;

     private void Awake()
     {
          speed = 3;

          rnd = new Random();
          rb = this.gameObject.GetComponent<Rigidbody2D>();


          if (missTarget) OffsetTarget();
          

          moveDirection = targetPos - this.gameObject.transform.position;
          if (moveDirection != Vector3.zero)
          {
               float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
               transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
          }
          
     }
     
     public void OffsetTarget() // Offseting the projectile target position
     {

          float minX = GetRandomFloatNumber(-1.5, -1);
          float minY = GetRandomFloatNumber(-1.5, -1);
          float maxX = GetRandomFloatNumber(1, 1.5);
          float maxY = GetRandomFloatNumber(1, 1.5);

          float xOffset = GetRandomFloatNumber(minX, maxX);
          float yOffset = GetRandomFloatNumber(minY, maxY);
          targetPos = new Vector3(targetPos.x + xOffset, targetPos.y + yOffset, targetPos.z);
     }

     public float GetRandomFloatNumber(double minimum, double maximum)
     {
          return (float)(rnd.NextDouble() * (maximum - minimum) + minimum);
     }

     void Update()
     {
          Vector2 v2 = new Vector2(moveDirection.x,moveDirection.y);
          rb.AddForce(v2 * speed);


          CheckProjectileStatus();
     }

     private void CheckProjectileStatus() // Determinate the projectile life-span
     {
          float arrowDestinationLength = 1;

          if (missTarget) arrowDestinationLength = 1.3f;

          if (targetPos.x >= 0 && targetPos.x * arrowDestinationLength < transform.position.x)
          {
               Destroy(this.gameObject);
          }
          else if (targetPos.x < 0 && targetPos.x * arrowDestinationLength > transform.position.x)
          {
               Destroy(this.gameObject);
          }
     }
     

}
