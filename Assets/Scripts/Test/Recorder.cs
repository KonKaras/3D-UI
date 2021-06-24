using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using UnityEngine;

public class Recorder : MonoBehaviour
{
    [SerializeField]
    private string path = "";

    private float time_passed;
    private float time_idle;

    private float distance_traveled;
    private float degrees_turned;

    private Transform prev_pose;
    private Measurement measurement;
    string saved = "";

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        prev_pose = transform;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        time_passed += Time.deltaTime;

        if (IsIdle()) UpdateIdleTime();
    }

    bool IsIdle()
    {
        // maybe lower than very small threshold instead if errors occur
        return prev_pose.position == transform.position;
    }

    void UpdateIdleTime()
    {
        time_idle += Time.deltaTime;
        // TODO create new Measurement.IdlePhase when needed
    }

    void UpdateTimePassed()
    {
        
    }

    // TODO save at the end of test e.g as JSON 

}
