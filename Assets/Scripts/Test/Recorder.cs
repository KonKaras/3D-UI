using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
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

	private Vector3 targetPos = Vector3.zero;//TODO set somewhere
	private float prev_distance;

	void Start()
	{

	}

	private void OnEnable()
	{
		prev_pose = transform;
		prev_distance = GetDistance();
	}

	private float GetDistance()
	{
		return (targetPos - transform.position).magnitude;
	}

	void FixedUpdate()
	{
		measurement.time_passed += Time.fixedDeltaTime;
		UpdateDistances();

		if (idle = IsIdle())
		{
			UpdateIdleTime();
		}
		prev_idle = idle;
	}

	bool IsIdle()
	{
		return prev_pose.position.IsCloseTo(transform.position);
	}

	void UpdateIdleTime()
	{
		measurement.time_idle += Time.fixedDeltaTime;

		if (measurement.idle_phases.Count == 0 || idle != prev_idle)
		{
			measurement.SetupIdlePhase(Time.time, transform.position);
		}

		measurement.idle_phases[measurement.idle_phases.Count - 1].duration += Time.fixedDeltaTime;
	}

	void UpdateDistances()
	{
		float step = (transform.position - prev_pose.position).magnitude;
		measurement.distance_traveled += step;

		float cur_distance = GetDistance();
		if (cur_distance > prev_distance)
		{
			measurement.distance_wrong_dir += step;
		}
		else
		{
			measurement.distance_right_dir += step;
		}
	}

	void Save()//TODO call this after test is finished
	{
		//"messure.txt" should be created/overriden in a subfolder of /Users/Username/Appdata (this folder might be hidden)
		File.WriteAllText(Application.dataPath + "//resources//messure.txt", JsonUtility.ToJson(measurement));//TODO check if this works for list of Idles
	}
}
