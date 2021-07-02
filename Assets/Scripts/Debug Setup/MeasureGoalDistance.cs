using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeasureGoalDistance : MonoBehaviour
{
    public Transform goal1;
    public Transform goal2;
    public Transform goal3;

    public Transform spawn1;
    public Transform spawn2;
    public Transform spawn3;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Test 1: " + (goal1.position - spawn1.position).magnitude);
        Debug.Log("Test 2: " + (goal2.position - spawn2.position).magnitude);
        Debug.Log("Test 3: " + (goal3.position - spawn3.position).magnitude);
    }
}
