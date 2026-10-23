using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

[TrackColor(0.2f, 0.2f, 0.2f)]
[TrackClipType(typeof(DisableObjectAsset))]
[TrackBindingType(typeof(PlayableDirector))]
public class DisableObjectTrack : TrackAsset
{
    // No need to override unless you're customizing further.
}