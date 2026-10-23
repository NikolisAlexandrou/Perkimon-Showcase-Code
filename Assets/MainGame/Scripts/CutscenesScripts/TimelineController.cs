using UnityEngine;
using UnityEngine.Playables;

public class TimelineController : MonoBehaviour
{
    public PlayableDirector director;

    public void PauseTimeline()
    {
        director.Pause();
    }

    public void ResumeTimeline()
    {
        director.Play();
    }
}