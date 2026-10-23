using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;

[System.Serializable]
public class PlayBattleAsset : PlayableAsset
{
    public ExposedReference<BattleTrainer> trainer; //  Correct type

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable <PlayBattleBehaviour>.Create(graph);
        var behaviour = playable.GetBehaviour();

        // Resolve the exposed reference at runtime
        behaviour.trainer = trainer.Resolve(graph.GetResolver());

        return playable;
    }
}