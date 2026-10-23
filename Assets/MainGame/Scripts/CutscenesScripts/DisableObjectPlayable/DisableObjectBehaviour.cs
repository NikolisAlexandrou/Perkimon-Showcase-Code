using UnityEngine;
using UnityEngine.Playables;
using System.Collections.Generic;
using System.Collections;
public class DisableObjectBehaviour : PlayableBehaviour
{
    public GameObject ob;
    public bool activeSelf;
    private Playable playable;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        this.playable = playable;
        ob.SetActive(activeSelf);
    }
}