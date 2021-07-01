using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoop : MonoBehaviour
{
    public GameObject goal1;
    public GameObject goal2;
    public GameObject goal3;
    public GameObject nextText;
    public GameObject endText;

    public int progress;

    bool waiting;

    // Start is called before the first frame update
    void Start()
    {
        waiting = true;
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public void nextStep(int step)
    {
        Debug.Log(step);
        progress = step;

        nextText.SetActive(true);
        waiting = true;
    }


    public void setNextStep()
    {
        if(progress == 0)
        {
            goal1.SetActive(true);
            nextText.SetActive(false);
        }
        if(progress == 1)
        {
            goal1.SetActive(false);
            goal2.SetActive(true);
            nextText.SetActive(false);
        }
        if (progress == 2)
        {
            goal2.SetActive(false);
            goal3.SetActive(true);
            nextText.SetActive(false);
        }
        if(progress == 3)
        {
            goal3.SetActive(false);
            nextText.SetActive(false);
            endText.SetActive(true);
        }
    }

    public void goNext()
    {
        if(waiting == true)
        {
            waiting = false;
            setNextStep();
        }
    }
}
