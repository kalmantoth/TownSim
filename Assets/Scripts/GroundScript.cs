﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundScript : MonoBehaviour
{
     // Start is called before the first frame update
     void Start()
     {
        
     }

     // Update is called once per frame
     void Update()
     {
        
     }

     public void OnMouseDown()
     {
          GlobVars.UnselectWorkers();
          GlobVars.infoPanelGameObject = this.gameObject;
          
     }

     public override string ToString()
     {
          return "Just grass Pal! Nothing else to see.";
     }
     
}
