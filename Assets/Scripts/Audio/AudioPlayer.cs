﻿using System.Collections;
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

	[Header("Volume")]
	[SerializeField]
	private float volume_max = 1;
	[SerializeField]
	private List<Interval> intervals_volume;

	//public bool playAudio = true;
	[Header("Tools")]
	public GameLoop.TEST_MODE mode;
	[SerializeField]
	[Tooltip("Defines distance intervals as color key locations.")]
	private Gradient intervals_percentages;
	[SerializeField]
	private float update_cooldown = 0.3f;
	private float update_cooldown_curr;
	private float update_cooldown_start;

	private AudioSource source;

	private float starting_dist;
	private float repetitions_per_second_curr;

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

		starting_dist = UpdateDistance();
		MatchPercentages();

		//repetitions_per_second_curr = 3f;

		if(mode == GameLoop.TEST_MODE.REPETITION)
        {
			update_cooldown_curr = 1f / intervals_repetitions[0].lower;
        }
        else
        {
			update_cooldown_curr = update_cooldown;
		}

		if (mode == GameLoop.TEST_MODE.TEXT)
		{
			distanceDisplay.SetActive(true);
		}

		update_cooldown_start = update_cooldown;

		StartCoroutine(UpdateTestValue());
	}

	void FixedUpdate()
	{
		PlaySound();
	}

	void PlaySound()
    {
		update_cooldown_curr -= Time.fixedDeltaTime;
		if(update_cooldown_curr <= 0f)
        {
			if (mode == GameLoop.TEST_MODE.TEXT)
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

	IEnumerator UpdateTestValue()
    {
		PerformModeStep();
		yield return new WaitForSeconds(update_cooldown);
		StartCoroutine(UpdateTestValue());
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

		return lerp_percentage;
	}

	void PerformModeStep()
	{
        switch (mode)
        {
			case GameLoop.TEST_MODE.VOLUME: VolumeStep(); break;
			case GameLoop.TEST_MODE.REPETITION: RepetitionStep(); break;
			case GameLoop.TEST_MODE.TEXT: TextStep(); break;
        }
		
		
	}
	

	void RepetitionStep()
	{
		source.volume = volume_max;
		AdaptRepetitions();
	}

	void VolumeStep()
	{
		update_cooldown_start = update_cooldown;
		AdaptVolume();
	}

	void TextStep()
    {
		distanceDisplay.SetActive(true);
		distanceText.text = string.Format("{0:00}m", UpdateDistance());
	}


	private void AdaptRepetitions()
	{
		float current_dist = UpdateDistance();

		float dist_percentage = 1f - Mathf.Clamp(current_dist / starting_dist, 0f, 1f);
		Interval current_interval = SelectInterval(intervals_repetitions, dist_percentage);

		float lerp_percentage = CalculateLerpPercentage(intervals_repetitions, current_interval, dist_percentage);
		repetitions_per_second_curr = Mathf.Lerp(current_interval.lower, current_interval.upper, lerp_percentage);

		update_cooldown_start = 1f / repetitions_per_second_curr;
	}

	void AdaptVolume()
	{
		float current_dist = UpdateDistance();
		float dist_percentage = 1f - Mathf.Clamp(current_dist / starting_dist, 0f, 1f);

		Interval current_interval = SelectInterval(intervals_volume, dist_percentage);

		float lerp_percentage = CalculateLerpPercentage(intervals_volume, current_interval, dist_percentage);
		source.volume = Mathf.Lerp(current_interval.lower, current_interval.upper, lerp_percentage);
	}

	private float UpdateDistance()
	{
		return Mathf.Abs((transform.parent.position - target.transform.position).magnitude);
	}

	//TODO move this into a game logic class

}