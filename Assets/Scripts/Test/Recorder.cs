using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using UnityEngine;

public class Recorder : MonoBehaviour
{
    [SerializeField]
    private string path = "";

    private Transform prev_pose;
    private Measurement measurement;
    string saved = "";
    bool prev_idle;
    bool idle;

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
        measurement.time_passed += Time.fixedDeltaTime;
        measurement.distance_traveled += (transform.position - prev_pose.position).magnitude;

        idle = IsIdle();
        if (idle) UpdateIdleTime();
        prev_idle = idle;
    }

    bool IsIdle()
    {
        // maybe lower than very small threshold instead if errors occur
        return prev_pose.position == transform.position;
    }

    void UpdateIdleTime()
    {
        measurement.time_idle += Time.fixedDeltaTime;

        if(measurement.idle_phases.Count == 0 || idle != prev_idle)
        {
            measurement.SetupIdlePhase(Time.time, transform.position);
        }

        measurement.idle_phases[measurement.idle_phases.Count - 1].duration += Time.fixedDeltaTime;

    }

    void UpdateTimePassed()
    {
        
    }

    // TODO save at the end of test e.g as JSON 

}
