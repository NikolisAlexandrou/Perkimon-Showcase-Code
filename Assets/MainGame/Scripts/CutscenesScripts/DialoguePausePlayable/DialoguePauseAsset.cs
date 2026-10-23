using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;

[System.Serializable]
public class DialoguePauseAsset : PlayableAsset, ITimelineClipAsset
{
    public string npcName;
    public Sprite npcImage;
    [TextArea]
    public List<string> dialogueLine = new List<string>();


    public ClipCaps clipCaps => ClipCaps.None;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<DialoguePauseBehaviour>.Create(graph);
        playable.GetBehaviour().dialogueLine = dialogueLine;
        playable.GetBehaviour().npcName = npcName;
        playable.GetBehaviour().npcImage = npcImage;
        return playable;
    }
}