using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Rendering.PostProcessing;

/// <summary>
/// Teleports player after some time delay in a position after entering a trigger.
/// Teleport method is used as UI Event in TransitionFunctions
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class TeleportScript : MonoBehaviour
{
    [Tooltip("Position to teleport")]
    [SerializeField] private Vector2 _teleportPosition;

    [Tooltip("Delay in seconds before teleporting")]
    [SerializeField] private float _teleportDelay = 0.1f;

    [Tooltip("True for if player is going to teleport in a cave")]
    [SerializeField] private bool _goingInCave;

    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        //  Player enters trigger and plays UI transition to teleport
        if (collision.CompareTag("Player"))
            Teleport();
            //StartCoroutine(teleport(collision.gameObject));
    }

    /// <summary>
    /// Play the UI teleport transition which contains an event for teleporting
    /// </summary>
    public void Teleport()
    {
        GameUtils.i.currentCaveEntrance = this;
        UIManager.i.caveTransition.SetActive(true);
    }

    /// <summary>
    /// USED IN ANIMATOR EVENTS | hides the transition UI and waits some seconds 
    /// before teleporting the player to teleport positio
    /// </summary>
    /// <param name="teleportObject"></param>
    /// <returns></returns>
    public IEnumerator TeleportObject(GameObject teleportObject)
    {
        UIManager.i.EnableLoadingScreen(false);
        yield return new WaitForSeconds(_teleportDelay);
        teleportObject.transform.position = _teleportPosition;

        if (_goingInCave == true)
            DebugLogs.Log($"ENTRANCE > {teleportObject.name} going in cave");
           // GameUtils.i.mainCamera.GetComponent<PostProcessVolume>().profile = GameUtils.i.player.GetComponent<PlayerController>().caveAmbient;
        else
            DebugLogs.Log($"ENTRANCE > {teleportObject.name} going in an entrace");
            // GameUtils.i.mainCamera.GetComponent<PostProcessVolume>().profile = GameUtils.i.player.GetComponent<PlayerController>().overworldAmbient;
    }
}
