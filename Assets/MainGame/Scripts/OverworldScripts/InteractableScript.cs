using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static UnityEditor.Progress;

/// <summary>
/// Displays Interact button Image above player if player enters near an interactable object which has this script attached to it
/// and play certain events if player interacts with the object
/// </summary>
public class InteractableScript : MonoBehaviour
{
    [Tooltip("Events to play when interact")]
    [SerializeField] private UnityEvent _eventToPlay;

    [Tooltip("Cooldown for re-enabling interacting of this object")]
    [SerializeField] private float _interactCooldown = 0.1f;

    [Tooltip("Item to give to player IF GiveItem method is added in events")]
    [SerializeField] private Item _itemToGive;

    private bool _canInteract;   //  Wether or not the player can interact with the object
    private bool _isInCooldown;  //  Current cooldown timer for interacting with object

    private void Update()
    {
        //  Check if player is in overworld state
        if (GameUtils.i.battleSystem.CurrentState is not OverworldState)
            return;

        if (_canInteract == false)
            return;

        //  Check for interact pressed when no cooldown
        if (Input.GetButtonDown("Interact") && _isInCooldown == false)
        {
            //  Play event and add cooldown
            _eventToPlay?.Invoke();
            _isInCooldown = true;
            StartCoroutine(resetCD());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            //  Displays the interact button image above player head and enables interact
            GameUtils.i.DisplayInteractOb();
            _canInteract = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            //  Hides the interact button image above player head and disables interact
            GameUtils.i.InteractDisplayOb.SetActive(false);
            _canInteract = false;
        }
    }

    //  Reset cooldown after interactCoolDown seconds
    private IEnumerator resetCD()
    {
        yield return new WaitForSeconds(_interactCooldown);
        _isInCooldown = false;
    }

    /// <summary>
    /// ----- Used as an event in the editor -----
    /// Shows Monster storage UI
    /// </summary>
    public void ShowMonsterStorage() => UIManager.i.EnableMonsterStorage();

    /// <summary>
    /// ----- Used as an event in the editor -----
    /// Give an item to the player
    /// </summary>
    public void GiveItem()
    {
        _itemToGive.Initialise();
        DebugLogs.Log($"INTERACTABLE > Player interacted with item: {_itemToGive._name}");
        GameUtils.i.dialogueSystem.InitialiseNewFoundItemText(_itemToGive._name, AddToBag);
        AudioSystem.i.PlaySoundClip(AudioSystem.i.PlayRandomSound(AudioSystem.i.PickUpItemSounds), AudioMaster.SFX);
        GameUtils.i.battleSystem.SetState(GameState.FREEZE);
    }

    /// <summary>
    /// Add the item to player bag
    /// </summary>
    void AddToBag()
    {
        GameUtils.i.battleSystem.SetState(GameState.OVERWORLD);
        BagManager.i.AddItemToBag(_itemToGive);
        Destroy(gameObject);
    }
}
