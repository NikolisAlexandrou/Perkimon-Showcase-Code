using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "New Item", fileName = "item")]
public class ItemBase : ScriptableObject
{
    [Header("General Settings")]

    [Tooltip("Display name of the item")]
    public string DisplayName;

    [Tooltip("Image of item used in UI")]
    public Sprite Image;

    [Tooltip("Description of item used in UI")]
    [TextArea]
    public string Description;

    [Tooltip("Category of item, used for automatic bag sorting")]
    public ITEM_CATEGORY Category;

    [Header("For Healing Items")]

    [Tooltip("If true, when interacting in bag UI, party UI is displayed to heal a monster the healAmount")]
    public bool IsHealingMonster;

    [Tooltip("Amount to heal the selected monster, uses up the item")]
    public float HealAmount;

    [Header("For Scrolls")]

    [Tooltip("If true, when interacting in bag UI, party UI is displayed to teach a monster the move")]
    public bool IsMoveScroll;

    [Tooltip("If true, can interact with item in bag UI")]
    public bool IsInteractable;

    [Tooltip("The move that the item scroll teaches to selected monster")]
    public BattleMove TeachableMove;

    [Header("For Catching Objects")]

    [Tooltip("If true, can be used to catch monsters when defeating them in battle")]
    public bool IsCatchingObject;

    [Tooltip("Number of interactive minigames the player has to win in catching system consecutively to catch monster")]
    public int MaxTrials;

    [Tooltip("Multiplier for green zone in interactive minigame for catching system (eg 1.5f which increases by 50%)")]
    public float GreenZoneIncreaseScale = 1;//as percentage, eg 1.5f which increases by 50%

    [Tooltip("Multiplier for pointer speed in interactive minigame for catching system (eg 0.5f which decreases by 50%)")]
    public float PointerSpeedIncrease = 1;//as percentage, eg 0.5f which decreases by 50%

    [Tooltip("Capsule used for catching (Better create an actual capsule object")]
    public Capsule CatchingObject;

}

/// <summary>
/// Creates a new object of a shopkeeper item which contains the item selling and its price cost
/// </summary>
[Serializable]
public class ShopkeeperItem
{
    [SerializeField] Item _item;
    [SerializeField] int _cost;

    /// <summary>
    /// Get current selling item
    /// </summary>
    /// <returns></returns>
    public Item ShopItem => _item;

    /// <summary>
    /// Get current price cost of that item
    /// </summary>
    /// <returns></returns>
    public int ItemCost => _cost;

}
