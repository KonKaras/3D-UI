using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{

    public int step;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        /*if(collision.gameObject.tag == "Player")
        {
            Debug.Log("A");
        }
        else
        {
            Debug.Log("B");
        }*/

        GameObject.Find("GameLoopObj").GetComponent<GameLoop>().nextStep(step);
        
        
    }
}
