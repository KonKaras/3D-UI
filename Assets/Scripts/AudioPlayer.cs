using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    [SerializeField]
    private Transform target;
    [SerializeField]
    private float repetitions_per_second_start = 1;
    [SerializeField]
    private float repetitions_per_second_max = 20;

    public bool playAudio = true;

    private AudioSource source;

    private float starting_dist;
    private float repetitions_per_second_curr = 0.2f;

    private bool isPlaying;

    // Start is called before the first frame update
    void Start()
    {

    }

    private void OnEnable()
    {
        source = GetComponent<AudioSource>();
        starting_dist = UpdateDistance();
        StartCoroutine(OnPlayPing());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        AdaptRepetitions();
        StartCoroutine(OnPlayPing());
    }

    private void AdaptRepetitions()
    {
        float current_dist = UpdateDistance();
        float percentage = Mathf.InverseLerp(0f, starting_dist, current_dist);
        repetitions_per_second_curr = Mathf.Lerp(repetitions_per_second_max, repetitions_per_second_start, percentage);
    }

    private float UpdateDistance()
    {
        return Mathf.Abs((transform.parent.position - target.transform.position).magnitude);
    }

    private IEnumerator OnPlayPing()
    {
        if (!isPlaying)
        {
            source.Play();

            isPlaying = true;
            yield return new WaitForSeconds(1 / repetitions_per_second_curr);
            isPlaying = false;

        }
    }
}
