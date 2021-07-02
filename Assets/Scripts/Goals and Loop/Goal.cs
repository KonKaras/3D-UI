using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    [SerializeField] private GameLoop _loopInstance;

    private void OnTriggerEnter(Collider other)
    {
        _loopInstance.FinishTest();
    }
}