using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainManager : MonoBehaviour
{
    //Private Scripts
    Goalee goaleeScript;
    ObstacleManager om;
    //Parameters
    int maxObstacleCount;

    //Prefabs
    public GameObject goaleePrefab;
    public GameObject obstaclePrefab;

    GameObject goalee;
    public GameObject ball;
    public GameObject obstacleParent;

    public Vector3 obstSpawnOffset;

    void Start()
    {
        //if(this.goalee != null)
        //{
        //    this.goaleeScript = this.goalee.GetComponent<Goalee>();
        //    this.goaleeScript.GoalEvent += this.OnGoalEvent;
        //}
        this.goalee = null;
        this.om = this.obstacleParent.GetComponent<ObstacleManager>();
        this.maxObstacleCount = 3;
        this.obstSpawnOffset = new Vector3(0.0f, 0.0f, -12.0f);
        this.ChangeScene();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnGoalEvent(object source)
    {
        this.ChangeScene();
    }
    void ChangeScene()
    {
        
        if(this.goalee == null)
        {
            this.goalee = Instantiate(this.goaleePrefab);
            this.goaleeScript = this.goalee.GetComponent<Goalee>();
            this.goaleeScript.GoalEvent += this.OnGoalEvent;
            goalee.name = "Goalee";
        }
        //Adjust position of goalee
        float newGoaleeX = Random.Range(-5.5f, 5.5f);
        Vector3 oldGoaleePos = this.goalee.transform.position;
        this.goalee.transform.position = new Vector3(newGoaleeX , oldGoaleePos.y, -15.0f);
        Debug.Log(this.goalee.transform.position);

        //Clear previous obstacles
        foreach (Transform child in this.obstacleParent.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        int obstacleCount = Random.Range(1, this.maxObstacleCount);
        float minX = this.obstSpawnOffset.x - 16.0f*0.5f;
        float maxX = this.obstSpawnOffset.x + 16.0f * 0.5f;
        float intervalX = 16.0f / this.maxObstacleCount;
        for(int i=0;i<this.maxObstacleCount;i++)
        {
            GameObject obstacle = Instantiate(this.obstaclePrefab);
            obstacle.transform.parent = this.obstacleParent.transform;
            //Set transform
            float width = 16.0f;
            float randX = Random.Range(minX + i*intervalX, minX + (i+1) * intervalX);
            Vector3 oldPos = obstacle.transform.position;
            obstacle.transform.position = new Vector3(randX, oldPos.y, oldPos.z);

        }
        Debug.Log("Change Scene");
    }
}
