using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
public class TimelineDialogue : MonoBehaviour
{
    public TimelineController timelineController;
    [TextArea]
    public List<string> dialogue = new List<string>();

    public void StartDialogue()
    {
        timelineController.PauseTimeline(); // Pause cutscene
        List<UnityAction> l = new List<UnityAction>();
        l.Add(OnDialogueFinished);
        GameUtils.i.dialogueSystem.InitialiseNewText(dialogue, "Test", l);// Your existing UI method
    }

    public void OnDialogueFinished()
    {
        timelineController.ResumeTimeline(); // Resume when done
    }
}