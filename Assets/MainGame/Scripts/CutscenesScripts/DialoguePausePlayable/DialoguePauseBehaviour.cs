using UnityEngine;
using UnityEngine.Playables;
using System.Collections.Generic;
using System.Collections;
public class DialoguePauseBehaviour : PlayableBehaviour
{
    public string npcName;
    public Sprite npcImage;
    [TextArea]
    public List<string> dialogueLine = new List<string>();

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

     

        GameUtils.i.dialogueSystem.InitialiseNewTextNPC(dialogueLine, npcName,npcImage, OnDialogueFinished);
    }

    private void OnDialogueFinished()
    {
        isWaiting = false;
        graph.GetRootPlayable(0).SetSpeed(1); // Resume Timeline
    }
}