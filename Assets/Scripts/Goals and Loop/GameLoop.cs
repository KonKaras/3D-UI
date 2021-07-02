using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameLoop : MonoBehaviour
{
	#region Variables
	[SerializeField] private InputField _codeField;
	[SerializeField] private GameObject nextText;
	[SerializeField] private GameObject endText;
	[SerializeField] private ClueHandler _clueHandler;

	[SerializeField] private Recorder _recorder;

	[SerializeField] private Transform start1;
	[SerializeField] private Transform start2;
	[SerializeField] private Transform start3;
	[SerializeField] private GameObject goal1;
	[SerializeField] private GameObject goal2;
	[SerializeField] private GameObject goal3;

	private int progress = 0;
	private int currentTest = -1;

	private bool waiting;
	private int testCode;
	#endregion

	void Start()
	{
		waiting = true;
	}

	/// <summary> Called by the start button. Reades testcode once and starts the next test. </summary>
	public void StartTest()
	{
		if (currentTest == -1)
		{
			if (_codeField.text.Length != 3)
			{
				return;
			}
			testCode = int.Parse(_codeField.text);
			_codeField.gameObject.SetActive(false);
			nextText.SetActive(false);
		}

		PrepareNextStep();

		currentTest = GetTest();
		if (currentTest == -1)
		{
			return;// something went wrong
		}
		_clueHandler.StartTest(GetMode());
		_recorder.StartRecording(GetMode(), GetTargetPos());
		++progress;
	}

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
		else
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

	public void FinishTest()
	{
		nextText.SetActive(true);
		if (progress < 3)
		{
			nextText.SetActive(true);
		}
		else
		{
			endText.SetActive(true);
		}
		_clueHandler.PauseTest();
		_recorder.FinishRecording();
	}

	public enum TestMode
	{
		VOLUME,
		REPETITION,
		TEXT
	}
}