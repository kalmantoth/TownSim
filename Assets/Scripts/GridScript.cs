using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridScript : MonoBehaviour
{

     [SerializeField]
     private float size = 10f;

     public Vector3 GetNearestPointOnGrid(Vector3 position)
     {
          position -= transform.position;
          
          int xCount = Mathf.RoundToInt(position.x / size);
          int yCount = Mathf.RoundToInt(position.y / size);
          int zCount = Mathf.RoundToInt(position.z / size);

          Vector3 result = new Vector3(
              (float)(xCount) * size,   // + 0.5f is becouse tilemap grid layout corrigation
              (float)(yCount) * size,
              (float)zCount * size);

          result += transform.position;

          return result;
     }
     /*
     private void OnDrawGizmos()
     {
          Gizmos.color = Color.yellow;
          for (float x = -20; x < 20; x += size*2)
          {
               for (float y = -20; y < 20; y += size*2)
               {
                    var point = GetNearestPointOnGrid(new Vector3(x, y, 0f));
                    Gizmos.DrawSphere(point, 0.1f);
               }

          }
     }
     */
}
