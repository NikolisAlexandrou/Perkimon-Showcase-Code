using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;

[System.Serializable]
public class DisableObjectAsset : PlayableAsset
{
    public ExposedReference<GameObject> ob; //  Correct type
    public bool active;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<DisableObjectBehaviour>.Create(graph);
        var behaviour = playable.GetBehaviour();

        // Resolve the exposed reference at runtime
        behaviour.ob = ob.Resolve(graph.GetResolver());
        playable.GetBehaviour().activeSelf = active;
        return playable;
    }
}