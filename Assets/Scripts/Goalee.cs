using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goalee : MonoBehaviour
{
    // Start is called before the first frame update

    //Goal
    public delegate void GoalEventHandler(object source);
    public event GoalEventHandler GoalEvent;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider collider)
    {
        if(collider.gameObject.tag == "Ball")
        {
            //Trigger goal event
            this.GoalEvent(this);
        }
    }
}
