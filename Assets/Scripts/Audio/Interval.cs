using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Interval
{
    public float lower;
    public float upper;
    private float percentage;
    
    public void SetPercentage(float percentage)
    {
        this.percentage = percentage;
    }

    public float GetPercentage()
    {
        return percentage;
    }
}
