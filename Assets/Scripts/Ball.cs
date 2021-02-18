using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{

    //Public Scipts
    Goalee goaleeScript;
    public GameObject goaleeObject;

    // HyperParams
    Vector2 relCenterPos;  //A normalized vector representing the balls position relative to the screen. For x=0.5 it means that the ball is roughly at the center and y=1/3 means
    //that ball's height is 1/3 of the screen's height. When kicking the ball, curve and vertical angles will be decided by comparing the pressed positions to this vector

    public float maxVerticalAngle = 60.0f;
  
    //Between -90 and 90. This angle will curve the ball
    public float curveFactor = 60.0f;
    public float horizontalAngleFactor = 60.0f;
    public Rigidbody rb;
    public float maxForce = 5.0f;
    public float timeToForce;
    private Vector3 pressPos, releasePos;
    public float pressTime, releaseTime;
    public bool kicked = false; //Player can't kick the ball while its already been kicked
    public bool pressed = false; //Will be set to true when player starts kicking 

    private List<Vector3> mousePoints;
    private Vector3 lastPressPos;
    private float lastPressTime;
    private float pointSaveFreq = 0.1f;

    //Movement variables (Public because other scripts might need these)
    [SerializeField]
    public float verticalAngle = 0.0f;
    [SerializeField]
    public float horizontalAngle = 0.0f;  //0deg is straight to kale. Between -90 and 90
    [SerializeField]
    public float kickForce = 0.0f;
    [SerializeField]
    public float curveAngle = 0.0f;

    private SphereCollider insideCollider;
    private SphereCollider outsideCollider; //This one aims to provide a bigger collider than the object, since it can be difficult to hit the ball when actual collider is too small

    //Event
    public delegate void ResetEventHandler();
    public event ResetEventHandler ResetEvent;

    void Start()
    {
        this.relCenterPos = new Vector2(0.5f, 1.0f / 3.0f);
        this.rb = (Rigidbody)this.GetComponent<Rigidbody>();
        this.kicked = false;
        this.timeToForce = 0.1f;
        this.mousePoints = new List<Vector3>();
        this.insideCollider = this.GetComponents<SphereCollider>()[0];
        this.outsideCollider = this.GetComponents<SphereCollider>()[1];

        if(this.goaleeObject != null)
        {
            this.goaleeScript = this.goaleeObject.GetComponent<Goalee>();
            this.goaleeScript.GoalEvent += this.OnGoalEvent;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            if(!this.pressed)
            {
                this.pressPos = Input.mousePosition;
                this.pressed = true;
                this.lastPressPos = this.pressPos;
                this.lastPressTime = Time.time;
                //Vector3 result;
                //this.raycastTest(this.pressPos, out result);
            }
        }
        else if(Input.GetMouseButtonUp(0))
        {
            if(!this.kicked)  //Have at least two points
            {

                ///*
                // * Angle diff between the last and the first forceVec can be the curvature angle
                // */
                //float minX = this.mousePoints[0].x;
                //float maxX = this.mousePoints[0].x;
                //float totalMag = 0;
                //for(int i =1;i<this.mousePoints.Count;i++)
                //{
                //    Vector3 _diff = this.mousePoints[i] - this.mousePoints[i - 1];
                //    totalMag += _diff.magnitude;
                //}

                //Vector3 diffTotal = this.mousePoints[this.mousePoints.Count - 1] - this.mousePoints[0];

                //// Calculate force vector params
                //this.releasePos = Input.mousePosition;
                //this.verticalAngle = (Screen.height * 1.0f/3.0f - Mathf.Clamp(this.pressPos.y, 0, Screen.height * 1.0f / 3.0f)) / (Screen.height * 1.0f / 3.0f) *this.maxVerticalAngle;
                //Vector3 diffVec = this.releasePos - this.pressPos;
                //float forceAmount = Mathf.Clamp(totalMag, 0.0f, maxForce)*timeToForce;
                //this.horizontalAngle = Mathf.Atan(diffVec.x / diffVec.y);

                //// Curve
                //Debug.Log(this.mousePoints.Count);
                //this.curveAngle = -1*(this.pressPos.x - Screen.width/2.0f)/(Screen.width*0.5f)*90.0f;
                //this.kicked = true;
                //this.pressed = false;
                this.pressed = false;
                this.kicked = true;
                this.kickBall(this.kickForce, this.verticalAngle, this.horizontalAngle, this.curveAngle);

            }
            else
            {
                this.pressed = false;
            }

        }
        if(Input.GetKeyDown(KeyCode.R))
        {
            this.resetBall();  //Reset the ball before so others can act on it
            ResetEvent();
        }

        ////Save force vectors
        //if(this.pressed && this.mousePoints.Count <= 10)
        //{
        //    float currTime = Time.time;
        //    if((currTime - this.lastPressTime)%this.pointSaveFreq == 0)
        //    {
        //        Vector3 currPos = Input.mousePosition;
        //        this.mousePoints.Add(currPos);
        //        this.lastPressPos = currPos;
        //    }
        //}

        if(this.pressed)
        {
            //Get current position
            float centerX = Screen.width * this.relCenterPos.x;
            float centerY = Screen.height * this.relCenterPos.y;
            Vector3 currPos = Input.mousePosition;
            this.curveAngle = (this.pressPos.x - centerX) / centerX * curveFactor;
            this.verticalAngle = (this.pressPos.y - centerY) / centerY * horizontalAngleFactor;
            Vector3 diffVec = this.pressPos - currPos;
            this.horizontalAngle = Mathf.Atan2(diffVec.y, diffVec.x) * Mathf.Rad2Deg;
            this.kickForce = Mathf.Abs(diffVec.y) / (Screen.height * 0.5f); //Different devices have different heights. A different vector that traverses half of the screen should mean max force
            this.kickForce = Mathf.Max(this.kickForce, 1.0f); // Clamp with 1 since this must be a normalized value
            this.kickForce = this.kickForce * this.maxForce;
        }
    }
    void FixedUpdate()
    {
        /*
         * Params:
         * i) velMag
         * ii) curveAngle
         */

        //This is for giving curve effect to the ball
        float velMag = this.rb.velocity.magnitude;
        if(this.kicked && this.rb.velocity.y >= 0.01f)
        {
            this.rb.velocity = new Vector3(this.rb.velocity.x + velMag*this.curveFactor*Time.deltaTime * Mathf.Sin(this.curveAngle * Mathf.Deg2Rad), this.rb.velocity.y, this.rb.velocity.z);
        }
    }

    bool raycastTest(Vector2 touchPos, out Vector3 result)
    {
        /*
         * Casts a ray to the ball. Ball's layer must be the 8th layer (see layer mask).
         * touchPos: Screen coords to make the test from.
         * result: A normalized vector that represents the local impact point on the ball.
         * return: True if ray hits, false if not.
         */
        Ray ray = Camera.main.ScreenPointToRay(touchPos);
        RaycastHit hitInfo;
        int rayMask = 1 << 8; // Mask for the Ball's layer
        if(Physics.Raycast(ray, out hitInfo, 1000.0f, rayMask))
        {
            Vector3 hitPoint = hitInfo.point; //In terms o

            // Get the local position
            Vector3 localPos = hitPoint - this.transform.position;
            result = localPos / (this.outsideCollider.radius*0.5f);
            return true;
        }
        else
        {
            result = new Vector3(999, 999, 999);
            return false;
        }
    }
    void OnGoalEvent(object source)
    {
        this.resetBall();
    }
    public Vector3 getForceDir()
    {
        return new Vector3(-1 * Mathf.Cos(horizontalAngle * Mathf.Deg2Rad), Mathf.Sin(verticalAngle * Mathf.Deg2Rad), -1 * Mathf.Sin(horizontalAngle * Mathf.Deg2Rad)).normalized;
    }
    void kickBall(float forceMag, float verticalAngle, float horizontalAngle, float curveAngle)
    {

        //Vector3 forceVec = new Vector3(diffVec.x, );
        Debug.Log(horizontalAngle);
        Vector3 forceVec = this.getForceDir();
        this.rb.AddForce(forceVec.normalized * forceMag, ForceMode.Impulse);
        this.kicked = true;
    }

    void resetBall()
    {
        this.transform.position = new Vector3(0.0f, 1.0f, -5.0f);
        this.rb.velocity = Vector3.zero;
        this.rb.angularVelocity = Vector3.zero;
        this.kicked = false;
    }
}
