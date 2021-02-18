using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KickHelper : MonoBehaviour
{
    public GameObject gizmoprefab;
    public int gizmoCount=5;
    public float lengthScale = 0.5f;
    public Vector3 startPos;
    Ball ballScript;
    // Start is called before the first frame update
    void Start()
    {
        GameObject ballObj = GameObject.FindGameObjectWithTag("Ball").gameObject;
        if(ballObj != null)
        {
            this.ballScript = ballObj.GetComponent<Ball>();
        }

        for(int i=0;i<this.gizmoCount;i++)
        {
            GameObject gizmo = Instantiate(this.gizmoprefab);
            gizmo.transform.parent = this.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Draw the gizmo if player pressing
        if (this.ballScript.pressed)
        {
            Vector3 endPoint = this.startPos + this.ballScript.getForceDir() * ballScript.kickForce * this.lengthScale;
            Vector3 midPoint = Vector3.Lerp(this.startPos, endPoint, 0.5f);
            Vector3 extremeCurvePoint = new Vector3(endPoint.x, this.startPos.y, this.startPos.z);
            // ballScript.curveAngle/ballScript.curveFactor is 0 for no curve and 1 for extreme curve
            Vector3 curvePoint = Vector3.Lerp(midPoint, extremeCurvePoint, ballScript.curveAngle / ballScript.curveFactor);

            int count = 0;
            foreach (Transform child in this.transform)
            {
                float t = (float)count / (float)this.transform.childCount;
                //Bezier curve
                Vector3 pa = Vector3.Lerp(this.startPos, curvePoint, t);
                Vector3 pb = Vector3.Lerp(curvePoint, endPoint, t);
                Vector3 currPoint = Vector3.Lerp(pa, pb, t);
                count++;
                child.position = currPoint;
            }
        }
    }
}
