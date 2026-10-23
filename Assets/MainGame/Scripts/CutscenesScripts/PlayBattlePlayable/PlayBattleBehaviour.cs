using UnityEngine;
using UnityEngine.Playables;
using System.Collections.Generic;
using System.Collections;
public class PlayBattleBehaviour : PlayableBehaviour
{
    public BattleTrainer trainer;

    private bool isWaiting = false;
    private Playable playable;
    // private PlayableDirector director;
    private PlayableGraph graph;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (isWaiting) return;

        this.playable = playable;
        //this.director = GameObject.FindFirstObjectByType<PlayableDirector>();
        graph = playable.GetGraph();
        isWaiting = true;

        graph.GetRootPlayable(0).SetSpeed(0); // Freeze Timeline


        //initialise battle
        GameUtils.i.battleSystem.CurrentTrainer = trainer;
        GameUtils.i.battleSystem.OnBattleEndEvent.AddListener(OnBattleFinished);
        GameUtils.i.battleSystem.TriggerTrainerBattle();

    }

    private void OnBattleFinished()
    {
        isWaiting = false;
        graph.GetRootPlayable(0).SetSpeed(1); // Resume Timeline
        GameUtils.i.battleSystem.OnBattleEndEvent.RemoveListener(OnBattleFinished);
    }
}