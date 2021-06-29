using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioPlayer : MonoBehaviour
{
	#region Variables
	[SerializeField]
	private Transform target;
	[Header("Repetitions")]
	[SerializeField]
	private List<Interval> intervals_repetitions;
	/*
    [SerializeField]
    private float repetitions_per_second_start = 1;
    [SerializeField]
    private float repetitions_per_second_max = 20;    
    */
	[Header("Volume")]
	/*
    [SerializeField]
    private float volume_start = 0.05f;
    */
	[SerializeField]
	private float volume_max = 1;
	[SerializeField]
	private List<Interval> intervals_volume;

	//public bool playAudio = true;
	[Header("Tools")]
	public bool volume_mode = true;
	[SerializeField]
	[Tooltip("Defines distance intervals as color key locations.")]
	private Gradient intervals_percentages;

	private AudioSource source;

	private float starting_dist;
	private float repetitions_per_second_curr;
	private float volume_curr;

	private bool is_playing;
	private bool mode_prev;
	private bool mode_curr;

	//Reminder: TODO maybe create an own class for text (or rename class)
	[SerializeField]
	private GameObject distanceDisplay;
	[SerializeField]
	private Text distanceText;
	public bool isUsingText = false; //TODO only testcode
	#endregion

	private void OnEnable()
	{
		source = GetComponent<AudioSource>();
		volume_curr = volume_max;

		starting_dist = UpdateDistance();
		MatchPercentages();

		repetitions_per_second_curr = 3f;

		mode_prev = volume_mode;
	}

	private void Update()
	{
		distanceDisplay.SetActive(isUsingText);//TODO I should find a better solution
	}

	void FixedUpdate()
	{
		mode_curr = volume_mode || isUsingText;
		ChooseMode();
	}

	void MatchPercentages()
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

	Interval SelectInterval(List<Interval> from, float percentage)
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

	float CalculateLerpPercentage(List<Interval> from, Interval current_interval, float dist_percentage)
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
		//Debug.Log(lerp_percentage + " " + current_interval.lower + " " + current_interval.upper);
		//Debug.Log(current_interval.GetPercentage() + " " + upper_value);

		return lerp_percentage;
	}

	void ChooseMode()
	{
		if (mode_curr != mode_prev)
		{
			if (mode_curr)
			{
				StopCoroutine(OnPlayRepetition());
				repetitions_per_second_curr = 1f;
				VolumeStep();
			}
			else
			{
				StopCoroutine(OnPlayRepetition());
				source.volume = volume_max;
				RepetitionStep();
			}
			mode_prev = mode_curr;
		}
		else
		{
			if (mode_curr) VolumeStep();
			else RepetitionStep();
		}
	}

	void RepetitionStep()
	{
		AdaptRepetitions();
		StartCoroutine(OnPlayRepetition());
	}

	void VolumeStep()
	{
		AdaptVolume();
		StartCoroutine(OnPlayRepetition());
	}

	private void AdaptRepetitions()
	{
		float current_dist = UpdateDistance();
		//Debug.Log("Current distance " + current_dist);

		float dist_percentage = 1f - Mathf.Clamp(current_dist / starting_dist, 0f, 1f);
		//Debug.Log("Current distance % " + dist_percentage);
		Interval current_interval = SelectInterval(intervals_repetitions, dist_percentage);

		float lerp_percentage = CalculateLerpPercentage(intervals_repetitions, current_interval, dist_percentage);
		repetitions_per_second_curr = Mathf.Lerp(current_interval.lower, current_interval.upper, lerp_percentage);
	}

	void AdaptVolume()
	{
		float current_dist = UpdateDistance();
		float dist_percentage = 1f - Mathf.Clamp(current_dist / starting_dist, 0f, 1f);

		Interval current_interval = SelectInterval(intervals_volume, dist_percentage);

		float lerp_percentage = CalculateLerpPercentage(intervals_volume, current_interval, dist_percentage);
		source.volume = Mathf.Lerp(current_interval.lower, current_interval.upper, lerp_percentage);
		//Debug.Log("Vol " + source.volume + " lerp_percentage " + lerp_percentage);
	}

	private float UpdateDistance()
	{
		return Mathf.Abs((transform.parent.position - target.transform.position).magnitude);
	}

	private IEnumerator OnPlayRepetition()
	{
		if (!is_playing)
		{
			is_playing = true;
			if (isUsingText)
			{
				distanceText.text = string.Format("{0:00}m", UpdateDistance());
			}
			else
			{
				source.Play();
			}

			yield return new WaitForSeconds(1 / repetitions_per_second_curr);
			is_playing = false;
		}
	}
}