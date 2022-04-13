using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundScript : MonoBehaviour
{
     public void OnMouseDown()
     {
          GlobVars.UnselectWorkers();
          GlobVars.infoPanelGameObject = this.gameObject;
          
     }

     public override string ToString()
     {
          return "Fertile ground";
     }
     
}
