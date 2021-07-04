using System.IO;
using UnityEngine;

public class Recorder : MonoBehaviour
{
	[System.Serializable]
	private struct TestData
	{
		public string code;
		public Measurement[] tests;
	}

	#region Variables
	private int recordCounter = 0;
	[SerializeField] private bool shouldSave = true;

	private Vector3 prevPos;
	private Quaternion prevRot;
	private Measurement measurement;
	private TestData testData;
	private bool prev_idle;
	private bool idle;

	private Vector3 targetPos = Vector3.zero;
	private float prev_distance;
	#endregion

	public void Init(string code)
	{
		testData.code = code;
		testData.tests = new Measurement[3];
	}

	void FixedUpdate()
	{
		if (measurement == null)
		{
			return;
		}

		measurement.time_passed += Time.fixedDeltaTime;
		UpdateDistances();

		if (idle = IsIdle())
		{
			UpdateIdleTime();
		}
		UpdateRotation();

		prev_idle = idle;
	}

	void UpdateDistances()
	{
		float step = (transform.position - prevPos).magnitude;
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
		prevPos = transform.position;
	}

	private float GetDistance()
	{
		return (targetPos - transform.position).magnitude;
	}

	bool IsIdle()
	{
		return prevPos.IsCloseTo(transform.position);
	}

	void UpdateIdleTime()
	{
		measurement.time_idle += Time.fixedDeltaTime;

		if (measurement.idle_phases.Count == 0 || idle != prev_idle)
		{
			measurement.SetupIdlePhase(Time.time, transform.position);
		}

		measurement.idle_phases[measurement.idle_phases.Count - 1].duration += Time.fixedDeltaTime;
		measurement.idle_phases[measurement.idle_phases.Count - 1].degrees_turned += Quaternion.Angle(transform.rotation, prevRot);
	}

	private void UpdateRotation()
	{
		measurement.degrees_turned += Quaternion.Angle(transform.rotation, prevRot);
		prevRot = transform.rotation;
	}

	#region Start/Finish Recording
	public void StartRecording(GameLoop.TestMode mode, Vector3 _targetPos)
	{
		if (measurement != null)
		{
			UpdateData();
		}
		measurement = new Measurement(mode);
		targetPos = _targetPos;
		prevPos = transform.position;
		prevRot = transform.rotation;
		prev_distance = GetDistance();
	}

	public void FinishRecording()
	{
		UpdateData();
		measurement = null;
	}

	private void UpdateData()
	{
		if (recordCounter < testData.tests.Length)
		{
			testData.tests[recordCounter++] = measurement;
		}
	}
	#endregion

	public void Save()
	{
		if (shouldSave)
		{
			string filepath = Directory.GetCurrentDirectory() + "\\meassure.txt";
			File.WriteAllText(filepath, JsonUtility.ToJson(testData));
			Debug.Log("Saved data in: " + filepath);
		}
	}
}