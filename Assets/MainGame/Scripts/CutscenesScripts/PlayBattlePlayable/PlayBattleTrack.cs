using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

[TrackColor(0.2f, 0.8f, .5f)]
[TrackClipType(typeof(PlayBattleAsset))]
[TrackBindingType(typeof(PlayableDirector))]
public class PlayBattleTrack : TrackAsset
{
    // No need to override unless you're customizing further.
}