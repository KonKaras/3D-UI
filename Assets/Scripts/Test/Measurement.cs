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

    public class IdlePhase
    {
        public float time_entered;
        public Vector3 position;
        public float duration;
        public float degrees_turned;
        private bool ended;

        public bool GetEnded()
        {
            return ended;
        }

        public void SetEnded()
        {
            ended = true;
        }
    }

    public void SetupIdlePhase(float time_entered, Vector3 pos)
    {
        IdlePhase idle_new = new IdlePhase();
        idle_new.time_entered = Time.time;
        idle_new.position = pos;
        idle_phases.Add(idle_new);
    }
}
