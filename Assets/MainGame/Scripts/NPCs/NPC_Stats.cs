using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains general stats and properties for an NPC like its name, dialogue sprite, dialogue, etc
/// </summary>
public class NPC_Stats : MonoBehaviour
{
    [Header("NPC Properties")]

    [Tooltip("The name of the NPC")]
    [SerializeField] private string _npcName = "John";

    [Tooltip("The sprite displayed in dialogue when NPC is talking")]
    [SerializeField] private Sprite _npcDialogueSprite;

    [TextArea]
    [Tooltip("Dialogue to display when NPC is interacted")]
    [SerializeField] private List<string> _interactDialogue = new List<string>();

    [Header("ONLY FOR SHOPKEEPER NPCs")]

    [Tooltip("Contains the list of items and prices the NPC is currently selling")]
    [SerializeField] private List<ShopkeeperItem> _shopkeeperItemsList = new List<ShopkeeperItem>();

    // /!\
    //
    // For healers make sure you add UnfreezeGame after healing the player's team
    // For Shopkeepers DO NOT Unfreeze the game after displaying UI
    //
    // /!\

    /// <summary>
    /// Returns the name of the npc
    /// </summary>
    /// <returns></returns>
    public string NPCName => _npcName;

    /// <summary>
    /// Returns the sprite displayed in dialogue while NPC is talking
    /// </summary>
    /// <returns></returns>
    public Sprite NPCDialogueSprite => _npcDialogueSprite;

    /// <summary>
    /// Get or set the dialogue text the NPC will say when interacted
    /// </summary>
    /// <returns></returns>
    public List<string> NPCInteractDialogue { get => _interactDialogue; set => _interactDialogue = value; }

    #region Methods used for UI Buttons or EventSystem

    /// <summary>
    /// Initialises UI of shopkeeper
    /// </summary>
    public void InitialiseShopkeeper()
    {
        UIManager.i.ShopkeeperUI.SetActive(true);
        ShopkeeperUIScript.i.InitialiseShopkeeperList(_shopkeeperItemsList);
    }

    /// <summary>
    /// Saves the current player position as the last healed position where if the player loses a battle,
    /// he will teleport here
    /// </summary>
    public void SavePositionHealed() => GameUtils.i.ChangeHealedNPCPosition();

    /// <summary>
    /// Heals all monsters in player party
    /// </summary>
    public void HealPlayerParty() => GameUtils.i.HealPlayerTeam();

    /// <summary>
    /// Unfreezes the game
    /// </summary>
    public void UnFreezeGame() => GameUtils.i.UnFreezeGame();

    #endregion
}
