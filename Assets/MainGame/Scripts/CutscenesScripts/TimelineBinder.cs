using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimelineBinder : MonoBehaviour
{
    public PlayableDirector director;
    public GameObject externalTarget; // This is the object in the other scene
    public string animationName;
    public string FolderName = "Player";


    public void Bind()
    {
        externalTarget = GameUtils.i.player;
        TimelineAsset timeline = director.playableAsset as TimelineAsset;
        // Find the track to bind (optional: use ExposedReference key)
        foreach (var track in timeline.GetRootTracks())
        {
            if (track.name == FolderName)
            {

                // Iterate over tracks within the group
                foreach (var subTrack in track.GetChildTracks())
                {
                    foreach (var output in subTrack.outputs)
                    {
                        director.SetGenericBinding(output.sourceObject, externalTarget);
                    }
                }
            }
        }
    }
}
