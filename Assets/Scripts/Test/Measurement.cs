using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Measurement : MonoBehaviour
{
    public float time_passed;
    public float time_idle;

    public float distance_traveled;
    public float degrees_turned;
    public List<IdlePhase> idle_phases;

    public struct IdlePhase
    {
        public float time_entered;
        public float duration;
        public float degrees_turned;
    }
}
