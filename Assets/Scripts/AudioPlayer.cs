using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    [SerializeField]
    private Transform target;
    [Header("Repetitions")]
    [SerializeField]
    private float repetitions_per_second_start = 1;
    [SerializeField]
    private float repetitions_per_second_max = 20;    
    [Header("Volume")]
    [SerializeField]
    private float volume_start = 0.05f;
    [SerializeField]
    private float volume_max = 1;

    //public bool playAudio = true;
    [Header("Tools")]
    public bool volume_mode = true;

    private AudioSource source;

    private float starting_dist;
    private float repetitions_per_second_curr;
    private float volume_curr;

    private bool is_playing;
    private bool mode_prev;
    private bool mode_curr;

    // Start is called before the first frame update
    void Start()
    {

    }

    private void OnEnable()
    {
        source = GetComponent<AudioSource>();
        starting_dist = UpdateDistance();
        repetitions_per_second_curr = 3f;
        volume_curr = volume_max;
        mode_prev = volume_mode;

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        mode_curr = volume_mode;
        ChooseMode();
    }

    void ChooseMode()
    {
        if(mode_curr != mode_prev)
        {
            if (mode_curr)
            {
                StopCoroutine(OnPlayRepetition());
                repetitions_per_second_curr = 1f;
                VolumeStep();
            }
            else
            {
                StopCoroutine(OnPlayRepetition());
                source.volume = volume_max;
                RepetitionStep();
            }
            mode_prev = mode_curr;
        }
        else
        {
            if (mode_curr) VolumeStep();
            else RepetitionStep();
        }
    }

    void RepetitionStep()
    {
        AdaptRepetitions();
        StartCoroutine(OnPlayRepetition());
    }

    void VolumeStep()
    {
        AdaptVolume();
        StartCoroutine(OnPlayRepetition());
    }

    private void AdaptRepetitions()
    {
        float current_dist = UpdateDistance();
        float percentage = Mathf.Clamp(current_dist / starting_dist, 0f, 1f);
        //TODO maybe zones instead? Low -> low-mid -> mid -> high-mid -> high
        repetitions_per_second_curr = Mathf.Lerp(repetitions_per_second_max, repetitions_per_second_start, percentage);
    }

    void AdaptVolume()
    {
        float current_dist = UpdateDistance();
        float percentage = Mathf.Clamp(current_dist / starting_dist, 0f, 1f);
        //TODO maybe zones instead? Low -> low-mid -> mid -> high-mid -> high
        source.volume = Mathf.Lerp(volume_max, volume_start, percentage);
    }

    private float UpdateDistance()
    {
        return Mathf.Abs((transform.parent.position - target.transform.position).magnitude);
    }

    private IEnumerator OnPlayRepetition()
    {
        if (!is_playing)
        {
            is_playing = true;
            source.Play();
            yield return new WaitForSeconds(1 / repetitions_per_second_curr);
            is_playing = false;
        }
    }
}
