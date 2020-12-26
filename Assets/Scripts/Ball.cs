using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    // Start is called before the first frame update
    public float maxVerticalAngle = 60.0f;
  
    public float curveAngle = 25.0f; //Between -90 and 90. This angle will curve the ball
    public float curveFactor = 10.0f;
    public Rigidbody rb;
    public float maxForce = 500.0f;
    public float timeToForce;
    private Vector3 pressPos, releasePos;
    public float pressTime, releaseTime;
    public bool kicked = false; //Player can't kick the ball while its already been kicked
    private bool pressed = false; //Will be set to true when player starts kicking 
    private float verticalAngle;
    private float horizontalAngle;  //0deg is straight to kale. Between -90 and 90
    private List<Vector3> mousePoints;
    private Vector3 lastPressPos;
    private float lastPressTime;
    private float pointSaveFreq = 0.1f;

    private SphereCollider insideCollider;
    private SphereCollider outsideCollider; //This one aims to provide a bigger collider than the object, since it can be difficult to hit the ball when actual collider is too small

    //Event
    public delegate void ResetEventHandler();
    public event ResetEventHandler ResetEvent;

    void Start()
    {
        this.rb = (Rigidbody)this.GetComponent<Rigidbody>();
        this.kicked = false;
        this.timeToForce = 0.1f;
        this.mousePoints = new List<Vector3>();
        this.insideCollider = this.GetComponents<SphereCollider>()[0];
        this.outsideCollider = this.GetComponents<SphereCollider>()[1];
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
            if(!this.kicked && this.pressed && this.mousePoints.Count > 1)  //Have at least two points
            {

                /*
                 * Angle diff between the last and the first forceVec can be the curvature angle
                 */
                float minX = this.mousePoints[0].x;
                float maxX = this.mousePoints[0].x;
                float totalMag = 0;
                for(int i =1;i<this.mousePoints.Count;i++)
                {
                    Vector3 _diff = this.mousePoints[i] - this.mousePoints[i - 1];
                    totalMag += _diff.magnitude;
                }

                // Calculate force vector params
                this.releasePos = Input.mousePosition;
                this.verticalAngle = (Screen.height * 1.0f/3.0f - Mathf.Clamp(this.pressPos.y, 0, Screen.height * 1.0f / 3.0f)) / (Screen.height * 1.0f / 3.0f) *this.maxVerticalAngle;
                Vector3 diffVec = this.releasePos - this.pressPos;
                float forceAmount = Mathf.Clamp(totalMag, 0.0f, maxForce)*timeToForce;
                this.horizontalAngle = Mathf.Atan(diffVec.x / diffVec.y);

                // Curve
                this.curveAngle = -1*(this.pressPos.x - Screen.width/2.0f)/(Screen.width*0.5f)*90.0f;
                this.kicked = true;
                this.pressed = false;

                this.kickBall(forceAmount, verticalAngle, horizontalAngle, curveAngle);

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

        //Save force vectors
        if(this.pressed && this.mousePoints.Count <= 10)
        {
            float currTime = Time.time;
            if((currTime - this.lastPressTime)%this.pointSaveFreq == 0)
            {
                Vector3 currPos = Input.mousePosition;
                this.mousePoints.Add(currPos);
                this.lastPressPos = currPos;
            }
        }
    }
    void FixedUpdate()
    {
        /*
         * Params:
         * i) velMag
         * ii) curveAngle
         */
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
            Debug.DrawRay(ray.origin, ray.direction*1000.0f, Color.red);
            Debug.Log(ray.direction);
            result = new Vector3(999, 999, 999);
            return false;
        }
    }
    void kickBall(float forceMag, float verticalAngle, float horizontalAngle, float curveAngle)
    {

        //Vector3 forceVec = new Vector3(diffVec.x, );
        Vector3 forceVec = new Vector3(-1 * Mathf.Sin(this.horizontalAngle), Mathf.Sin(this.verticalAngle * Mathf.Deg2Rad), -1 * Mathf.Cos(this.horizontalAngle));
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
