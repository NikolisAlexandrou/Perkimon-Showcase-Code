using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles player party switching in battle:
/// - Switch cooldown
/// - Applying the actual switch (hiding player, capsule, etc.)
/// 
/// BattleSystem:
/// - calls Tick() each frame in battle
/// - uses CanSwitch / CooldownTime to update HUD
/// - calls NotifyPlayerChoseSwitch() when the player confirms a new monster
/// </summary>
public class BattlePartyService : MonoBehaviour
{
    [Header("Owner / Dependencies")]
    [SerializeField] private BattleSystem _battleSystem;

    [Header("Switching")]

    [Tooltip("Holds the monster switch in battle cooldown")]
    [SerializeField] private float _partySwitchCooldown = 4f;

    private float _cooldownTimer;        //  current timer for monster switching
    private bool _canSwitch = true;      //  Flag to determine if player can switch monsters in battle
    private bool _playerSwitchPending;   //  Flag to determine if player is currently switching monsters

    public bool PlayerSwitchInProgress { get; private set; }    //  Flag to determine if player is currently switching monsters
    public bool CanSwitch => _canSwitch;
    public float CooldownTime => _cooldownTimer;

    void Awake()
    {
        if (!_battleSystem)
            _battleSystem = GetComponent<BattleSystem>();
    }

    /// <summary>
    /// Called to finish monster switching
    /// </summary>
    public void MarkPlayerSwitchFinished() => PlayerSwitchInProgress = false;

    /// <summary>
    /// Called once per frame while in battle.
    /// </summary>
    public void Tick(float deltaTime)
    {
        HandleCooldown(deltaTime);

        if (_playerSwitchPending)
            HandlePlayerSwitch();
    }

    /// <summary>
    /// Called by BattleSystem/PartyScript when the player selected a new party member
    /// to switch into battle.
    /// </summary>
    public void NotifyPlayerChoseSwitch()
    {
        if (!_canSwitch) return;

        _playerSwitchPending = true;
        PlayerSwitchInProgress = true;
    }

    /// <summary>
    /// Reset switching state (called when battle resets).
    /// </summary>
    public void ResetSwitchState()
    {
        _cooldownTimer = 0f;
        _canSwitch = true;
        _playerSwitchPending = false;
        PlayerSwitchInProgress = false;
    }

    /// <summary>
    /// When player switches monster, countdown and enable canSwitch flag if countdown = 0
    /// </summary>
    void HandleCooldown(float deltatTime)
    {
        if (_canSwitch) return;

        _cooldownTimer -= deltatTime;
        if (_cooldownTimer <= 0f)
        {
            _cooldownTimer = 0f;
            _canSwitch = true;
        }
    }

    /// <summary>
    /// Switches current battle monster with selected party monster in battle
    /// </summary>
    void HandlePlayerSwitch()
    {
        //  Start cooldown
        _cooldownTimer = _partySwitchCooldown + 1f;
        _canSwitch = false;

        var playerUnit = _battleSystem.PlayerUnit;

        //  Undo death state & animation on old unit
        playerUnit.IsDead = false;
        playerUnit?.StopAllCoroutines();
        if (playerUnit != null)
            playerUnit.gameObject.SetActive(false);

        //  Show capsule and mark battle as animating
        var capsule = _battleSystem.PlayerCatchingObject;
        if (capsule) capsule.SetActive(true);

        _battleSystem.IsAnimating = true;

        //  ReInitialise party member moves on UI
        var partyScene = UIManager.i.PartyScene;
        if (partyScene != null)
        {
            var partyScript = partyScene.GetComponent<PartyScript>();
            if (partyScript != null)
            {
                foreach (var member in partyScript.members)
                {
                    var partyMember = member.GetComponent<PartyMember>();
                    if (partyMember != null)
                        partyMember.InitialiseMoves();
                }
            }
        }

        //  Close party menu and fire event
        UIManager.i.EnablePartyMenu(false);
        _battleSystem.OnPlayerSwitchedMonsterEvent?.Invoke();

        //  Remove immunity after switch
        playerUnit.IsImmune = false;

        //  This request has been fully processed; we remain "in progress"
        _playerSwitchPending = false;
    }
}
