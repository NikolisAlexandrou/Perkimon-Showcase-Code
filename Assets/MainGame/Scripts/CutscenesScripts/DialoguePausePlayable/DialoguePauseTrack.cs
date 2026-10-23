using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

[TrackColor(0.2f, 0.8f, 1f)]
[TrackClipType(typeof(DialoguePauseAsset))]
[TrackBindingType(typeof(PlayableDirector))]
public class DialoguePauseTrack : TrackAsset
{
    // No need to override unless you're customizing further.
}