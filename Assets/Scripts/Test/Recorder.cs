using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class Recorder : MonoBehaviour
{
	#region Variables
	[SerializeField]
	private string path = "";
	private int recordCounter = 0;

	private Transform prev_pose;
	private Measurement measurement;
	private bool prev_idle;
	private bool idle;

	private Vector3 targetPos = Vector3.zero;
	private float prev_distance;
	#endregion

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
		if(measurement == null)
		{
			return;
		}

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

	public void StartRecording(GameLoop.TestMode mode, Vector3 _targetPos)
	{
		if(measurement != null)
		{
			Save();
		}
		measurement = new Measurement(mode);
		targetPos = _targetPos;
	}

	public void FinishRecording()
	{
		Save();
		measurement = null;
	}

	private void Save()
	{
		//"meassure.txt" should be created/overriden in a subfolder of /Users/Username/Appdata (this folder might be hidden)
		//Inside Editor: Resources/meassure.txt
		//Directory.GetCurrentDirectory();
		File.WriteAllText(Application.dataPath + "//Resources//" + path + "meassure" + ++recordCounter + ".txt", JsonUtility.ToJson(measurement));
	}
}