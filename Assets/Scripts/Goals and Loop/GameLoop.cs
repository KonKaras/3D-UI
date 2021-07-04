using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameLoop : MonoBehaviour
{
	#region Variables
	[Header("UI")]
	[SerializeField] private InputField _codeField;
	[SerializeField] private GameObject nextText;
	[SerializeField] private GameObject endText;
	[Header("Utility")]
	[SerializeField] private CueHandler _cueHandler;
	[SerializeField] private Recorder _recorder;
	[SerializeField] private FirstPersonController _player;
	[Header("Spawns & Targets")]
	[SerializeField] private Transform start1;
	[SerializeField] private Transform start2;
	[SerializeField] private Transform start3;
	[SerializeField] private GameObject goal1;
	[SerializeField] private GameObject goal2;
	[SerializeField] private GameObject goal3;

	private int progress = 0;
	private int currentTest = -1;

	private int testCode;
	#endregion

	/// <summary> Called by the start button. Reades testcode once and starts the next test. </summary>
	public void StartTest()
	{
		if (currentTest == -1)
		{
			if (_codeField.text.Length != 3)
			{
				return;
			}

			_recorder.Init(_codeField.text);
			testCode = int.Parse(_codeField.text.Substring(_codeField.text.Length - 3));
			_codeField.gameObject.SetActive(false);
			nextText.SetActive(false);
		}

		currentTest = GetTest();
		if (currentTest == -1)
		{
			return;// something went wrong
		}

		_player.SetInGame(true, GetSpawnPos());
		PrepareNextStep();

		TestMode mode = GetMode();
		Vector3 target = GetTargetPos();
		_cueHandler.StartTest(mode, target);
		_recorder.StartRecording(mode, target);
		++progress;
	}

	#region Init helpers
	public void PrepareNextStep()
	{
		if (progress == 0)
		{
			goal1.SetActive(true);
			nextText.SetActive(false);
		}
		else if (progress == 1)
		{
			goal1.SetActive(false);
			goal2.SetActive(true);
			nextText.SetActive(false);
		}
		else if (progress == 2)
		{
			goal2.SetActive(false);
			goal3.SetActive(true);
			nextText.SetActive(false);
		}
		else if(progress == 3)
		{
			goal3.SetActive(false);
			nextText.SetActive(false);
			endText.SetActive(true);
		}
	}

	private int GetTest()
	{
		switch (progress)
		{
			case 0: return testCode / 100;
			case 1: return (testCode / 10) % 10;
			case 2: return testCode % 10;
			default: return -1;
		}
	}

	private Vector3 GetSpawnPos()
	{
		switch (progress)
		{
			case 0: return start1.transform.position;
			case 1: return start2.transform.position;
			default: return start3.transform.position;
		}
	}

	private TestMode GetMode()
	{
		switch (currentTest)
		{
			case 1: return TestMode.VOLUME;
			case 2: return TestMode.REPETITION;
			default: return TestMode.TEXT;
		}
	}

	private Vector3 GetTargetPos()
	{
		switch (progress)
		{
			case 0: return goal1.transform.position;
			case 1: return goal2.transform.position;
			default: return goal3.transform.position;
		}
	}
	#endregion

	public void FinishTest()
	{
		_recorder.FinishRecording();
		if (progress < 3)
		{
			nextText.SetActive(true);
		}
		else
		{
			endText.SetActive(true);
			_recorder.Save();
		}
		_player.SetInGame(false, Vector3.zero);
		_cueHandler.PauseTest();
	}

	public enum TestMode
	{
		VOLUME,
		REPETITION,
		TEXT
	}
}