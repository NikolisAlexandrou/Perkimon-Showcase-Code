using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Set up events to play when NPC is interacted by player and display dialogue of NPC or
/// Battle NPC if it is a trainer NPC
/// </summary>

[RequireComponent(typeof(NPC_Stats))]
public class NPC_Interact_Script : MonoBehaviour
{
    [Tooltip("Contains all methods that will be played after dialogue is shown after interacting the NPC")]
    public UnityEvent interactEvents;

    [Tooltip("Cooldown in seconds for when player can interact NPC again")]
    [SerializeField] private float _canInteractCooldown;

    private UnityEvent _currentEvents = new UnityEvent(); // Stores current events playing when interacting NPC
    private bool _hasBeenInteracted = false; // Flag to indicate whether the NPC has been interacted
    private bool _isInRangeToInteract;

    /// <summary>
    /// Get or set if the npc has been interacted
    /// </summary>
    /// <returns></returns>
    public bool HasBeenInteracted { get => _hasBeenInteracted; set => _hasBeenInteracted = value; }

    private void Update()
    {
        if (_isInRangeToInteract == false)
            return;

        if (Input.GetButtonDown("Interact"))
        {
            if (GameUtils.i.IsInCutscene() == true)
                return;

            if (GameUtils.i.battleSystem.CurrentState is FreezeState || GameUtils.i.battleSystem.IsInBattle == true )
                return;

            if (_hasBeenInteracted == true)
                return;

            // force trainer to face direction of player
            if (GetComponent<NPC_Behaviour>() != null)
            {
                Vector2 direction = ((Vector2)GameUtils.i.player.transform.position - (Vector2)transform.position).normalized;
                GetComponent<NPC_Behaviour>().DirectionLooking = direction;
            }

            // Plays only if npc is trainer
            if (GetComponent<BattleTrainer>() != null)
            {
                // if player interacts with trainer after player defeates trainer
                if (GetComponent<BattleTrainer>().IsDefeated == false)
                {
                    // If trainer is not yet battled, detect player and initiate a battle
                    GetComponent<BattleTrainer>().DetectPlayer();
                    return;
                }
                else
                {
                    // - add if trainer wins, add trainer win dialogue
                    GetComponent<NPC_Stats>().NPCInteractDialogue = GetComponent<BattleTrainer>().TrainerDefeatDialogue;
                }
            }

            // Interact behaviour of NPC
            NPCInteract();
            _hasBeenInteracted = true;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // If a cutscene is not playing and player is in range to interact, display interact object
            if (GameUtils.i.IsInCutscene() == true)
                return;

            _isInRangeToInteract = true;
            GameUtils.i.DisplayInteractOb();
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // If a cutscene is not playing and player is not in range to interact, hide the interact object
            if (GameUtils.i.IsInCutscene() == true)
                return;

            _isInRangeToInteract = false;
            GameUtils.i.InteractDisplayOb.SetActive(false);
        }
    }

    /// <summary>
    /// When player interacts NPC, freeze game, display dialogue and after dialogue is done
    /// play custom events if any and unfreeze the game again
    /// </summary>
    private void NPCInteract()
    {
        //  freeze game, add dialogue and custom events if any
        GameUtils.i.FreezeGame();
        _currentEvents?.RemoveAllListeners();
        _currentEvents.AddListener(interactEvents.Invoke);
        _currentEvents.AddListener(ResetInteractedDelayed);

        //  Initialise new dialogue to display and play events after dialogue ends
        if (GetComponent<NPC_Stats>().NPCInteractDialogue.Count > 0)
            GameUtils.i.dialogueSystem.InitialiseNewTextNPC(GetComponent<NPC_Stats>().NPCInteractDialogue, GetComponent<NPC_Stats>().NPCName, GetComponent<NPC_Stats>().NPCDialogueSprite, _currentEvents);
        else
            DebugLogs.Warning($"NPC: {name} does not have dialogue in its NPC_Stats component and may not work as intended.");
    }
    /// <summary>
    /// Re-enables player's ability to interact NPC after some canInteractCooldown time
    /// </summary>
    public void ResetInteractedDelayed() => StartCoroutine(resetInteract());

    IEnumerator resetInteract()
    {
        yield return new WaitForSeconds(_canInteractCooldown);
        _hasBeenInteracted = false;
    }
}