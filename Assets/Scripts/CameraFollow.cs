using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject ball;
    public GameObject goal;
    public Ball ballScript;
    public Vector3 initialPos;
    public float minHeight;

    public float posK = 1.0f;
    public float backupDistance = 2.0f;
    void Start()
    {
        //this.transform.rotation = Quaternion.Euler(new Vector3(0.0f, 30.0f * Mathf.PI / 180.0f, 0.0f));
        if(ball == null)
        {
            ball = GameObject.FindGameObjectWithTag("Ball");
        }
        this.ballScript = ball.GetComponent<Ball>();
        this.ballScript.ResetEvent += this.reset;
    }
    

    // Update is called once per frame
    void Update()
    {
        this.updateRotation();   
    }

    void updateRotation()
    {
        /*
         * 
         */
        if (this.ballScript.kicked)
        {
            //Calculate target pos
            Vector3 targetPos = this.ball.transform.position + Vector3.Normalize(this.ball.transform.position - this.goal.transform.position)*this.backupDistance;
            Vector3 error = targetPos - this.transform.position;
            if(error.magnitude > 0.001f && this.ball.transform.position.z > this.goal.transform.position.z)  //Don't follow if the ball is behind the goal
            {
                Vector3 newPos = error * Time.deltaTime * this.posK + this.transform.position;
                newPos.y = Mathf.Max(newPos.y, this.minHeight);
                this.transform.position = newPos;
                // Saturate the vertical position.
                
                this.transform.LookAt(this.ball.transform);


            }
            
        }
    }

    void reset()
    {
        this.transform.position = this.initialPos;
        this.transform.LookAt(this.ball.transform);
    }
}
