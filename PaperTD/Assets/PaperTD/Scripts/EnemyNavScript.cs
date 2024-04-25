using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class with Hard Coded 3 Node Path
public class EnemyNavScript : MonoBehaviour
{
    public float movementSpeed = 5;

    string currentTargetTag = "MiddleNode";
    Vector3 target;

    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.FindGameObjectWithTag(currentTargetTag).transform.position;
    }

    // Update is called once per frame
    void Update()
    {

        float distanceFromNode = (gameObject.transform.position - target).magnitude;
        if (distanceFromNode > 0.1)
        {
            Vector3 travelDirection = (target - gameObject.transform.position).normalized;
            gameObject.transform.position += movementSpeed * Time.deltaTime * travelDirection;
            Vector3 forwardOnPlane = gameObject.transform.forward;
            forwardOnPlane.y = 0;
            float angle = Vector3.Angle(forwardOnPlane.normalized, travelDirection);
            gameObject.transform.Rotate(new Vector3(0, 1, 0), angle);
        }
        else if (currentTargetTag == "MiddleNode")
        {
            currentTargetTag = "EndNode";
            target = GameObject.FindGameObjectWithTag(currentTargetTag).transform.position;
        }
    }
}
