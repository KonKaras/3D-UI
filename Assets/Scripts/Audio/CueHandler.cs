using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CueHandler : MonoBehaviour
{
	#region Variables
	private bool isUpdating = false;
	private Vector3 targetPos;
	[Header("Repetitions")]
	[SerializeField]
	private List<Interval> intervals_repetitions;

	[Header("Volume")]
	[SerializeField]
	private float volume_max = 1;
	[SerializeField]
	private List<Interval> intervals_volume;

	[Header("Tools")]
	public GameLoop.TestMode mode;
	[SerializeField]
	[Tooltip("Defines distance intervals as color key locations.")]
	private Gradient intervals_percentages;
	[SerializeField]
	private float update_cooldown = 0.3f;
	[SerializeField]
	private float dropoff_distance = 20f;
	private float update_cooldown_curr;
	private float update_cooldown_start;

	private AudioSource source;

	private float starting_dist_target;
	private float repetitions_per_second_curr;

	[Header("Distance display")]
	[SerializeField]
	private GameObject distanceDisplay;
	[SerializeField]
	private Text distanceText;
	#endregion

	#region Initialization
	private void OnEnable()
	{
		source = GetComponent<AudioSource>();

		MatchPercentages();
		distanceDisplay.SetActive(false);
	}

	private void MatchPercentages()
	{
		// maybe catch this
		int id = 0;
		foreach (GradientColorKey key in intervals_percentages.colorKeys)
		{
			intervals_repetitions[id].SetPercentage(key.time);
			intervals_volume[id].SetPercentage(key.time);
			id++;
		}
	}

	public void StartTest(GameLoop.TestMode _mode, Vector3 _targetPos)
	{
		mode = _mode;
		targetPos = _targetPos;
		isUpdating = true;

		starting_dist_target = UpdateDistance() + dropoff_distance;

		if (mode == GameLoop.TestMode.REPETITION)
		{
			source.volume = volume_max;
			update_cooldown_curr = 1f / intervals_repetitions[0].lower;
		}
		else
		{
			update_cooldown_curr = update_cooldown;
		}

		distanceDisplay.SetActive(mode == GameLoop.TestMode.TEXT);

		update_cooldown_start = update_cooldown;

		StartCoroutine(UpdateTestValue());
	}
	#endregion

	private IEnumerator UpdateTestValue()
	{
		PerformModeStep();
		yield return new WaitForSeconds(update_cooldown);
		StartCoroutine(UpdateTestValue());
	}

	private void PerformModeStep()
	{
		switch (mode)
		{
			case GameLoop.TestMode.VOLUME: VolumeStep(); break;
			case GameLoop.TestMode.REPETITION: RepetitionStep(); break;
			case GameLoop.TestMode.TEXT: TextStep(); break;
		}
	}

	#region Update
	void FixedUpdate()
	{
		RunEditorUpdate();
		if (isUpdating)
		{
			PlaySound();
		}
	}

	private void RunEditorUpdate()//Just testcode for editor
	{
		if (mode == GameLoop.TestMode.REPETITION)
		{
			source.volume = volume_max;
		}
		else
		{
			update_cooldown_start = update_cooldown;
		}
		distanceDisplay.SetActive(mode == GameLoop.TestMode.TEXT);
	}

	private void PlaySound()
	{
		update_cooldown_curr -= Time.fixedDeltaTime;
		if (update_cooldown_curr <= 0f)
		{
			if (mode == GameLoop.TestMode.TEXT)
			{
				distanceDisplay.SetActive(true);
			}
			else
			{
				distanceDisplay.SetActive(false);
				source.Play();
			}
			update_cooldown_curr = update_cooldown_start;
		}
	}
	#endregion

	public void PauseTest()
	{
		isUpdating = false;
		StopAllCoroutines();
	}

	#region Volume
	private void VolumeStep()
	{
		float current_dist = UpdateDistance();
		/*
		Vector3 relativePos = transform.InverseTransformPoint(startPos);
		if(relativePos.z < 0f)
        {
			Debug.Log(relativePos.z);
			float dist_percentage = Mathf.Clamp(Mathf.Abs(relativePos.z) / dropoff_distance, 0f, 1f);
			Debug.Log(dist_percentage);
			source.volume = Mathf.Lerp(0f, intervals_volume[0].lower, dist_percentage);
		}
		else
        {
		*/
		float dist_percentage = 1f - Mathf.Clamp(current_dist / starting_dist_target, 0f, 1f);

		Interval current_interval = SelectInterval(intervals_volume, dist_percentage);

		float lerp_percentage = CalculateLerpPercentage(intervals_volume, current_interval, dist_percentage);
		source.volume = Mathf.Lerp(current_interval.lower, current_interval.upper, lerp_percentage);
		//}
	}
	#endregion

	#region Repetition
	private void RepetitionStep()
	{
		float current_dist = UpdateDistance();

		float dist_percentage = 1f - Mathf.Clamp(current_dist / starting_dist_target, 0f, 1f);
		Interval current_interval = SelectInterval(intervals_repetitions, dist_percentage);

		float lerp_percentage = CalculateLerpPercentage(intervals_repetitions, current_interval, dist_percentage);
		repetitions_per_second_curr = Mathf.Lerp(current_interval.lower, current_interval.upper, lerp_percentage);
		if (repetitions_per_second_curr == 0f) repetitions_per_second_curr = 0.00001f;
		update_cooldown_start = 1f / repetitions_per_second_curr;
		if (update_cooldown_start < update_cooldown_curr) update_cooldown_curr = update_cooldown_start;
	}
	#endregion

	#region Text
	private void TextStep()
	{
		distanceText.text = string.Format("{0:00}m", UpdateDistance());
	}
	#endregion

	#region Helper functions
	private float UpdateDistance()
	{
		return Mathf.Abs((transform.parent.position - targetPos).magnitude);
	}

	private Interval SelectInterval(List<Interval> from, float percentage)
	{
		Interval prev_interval = from[0];
		foreach (Interval interval in from)
		{
			float curr_percentage = interval.GetPercentage();
			if (percentage > curr_percentage) prev_interval = interval;
			else
			{
				if (percentage == curr_percentage) return interval;
				else return prev_interval;
			}
		}
		return from[from.Count - 1];
	}

	private float CalculateLerpPercentage(List<Interval> from, Interval current_interval, float dist_percentage)
	{
		Interval next_interval = from[from.Count - 1];
		int id = 0;
		int next_id = from.Count - 1;
		foreach (Interval interval in from)
		{
			if (interval.GetPercentage() == current_interval.GetPercentage())
			{
				next_id = (id < from.Count - 1) ? id + 1 : from.Count - 1;
				next_interval = from[next_id];
				break;
			}
			id++;
		}

		//we lerp between current interval percentage (as marked in gradient) to the next interval key or 1 for last key
		float upper_value = (next_id != from.Count - 1) ? next_interval.GetPercentage() : 1f;
		float lerp_percentage = Mathf.InverseLerp(current_interval.GetPercentage(), upper_value, dist_percentage);

		return lerp_percentage;
	}
	#endregion
}