using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;

/// <summary>
/// Handles UI animation logic for a monster when it is evolving and moves back to next post battle state
/// when monster evolves and animation ends.
/// - Attach this script to UI parent object containing the UI animations objects as children
/// </summary>
public class EvolutionAnimation : MonoBehaviour
{
    [Header("UI")]

    [Tooltip("Background game object of evolution UI")]
    [SerializeField] private GameObject _background;

    [Tooltip("Game object of evolving monster in evolution UI")]
    [SerializeField] private GameObject _monsterObject;

    [Header("Timing")] 

    [Tooltip("Time delay before animation starts")]
    [SerializeField] private float _startDelay = 0;

    [Tooltip("Duration of the evolution animation")]
    [SerializeField] private float _duration = 10;

    [Tooltip("Number of frames of evolution animation")]
    [SerializeField] private int _framesCount = 16;

    [Header("Dependencies")]

    [Tooltip("Battle Evolution Service script")]
    [SerializeField] private BattleEvolutionService _evolutionService;

    //private bool isAnimating;   //  Flag to see wether the animation is playing
    private BattleMonster _monsterToEvolve;  //  Current monster that is going to evolve
    private Image _monsterImage;     //  UI Image of sprite of evolving monster
    private Color _originalColor;    //  Starting color of the monster UI image
    private Sprite _currentSprite;   //  Current sprite of evolving monster BEFORE EVOLVE
    private Sprite _evolvedSprite;   //  Current sprite of evolving monster AFTER EVOLVE

    private readonly List<string> _evolvedIntoDialogue = new List<string>(); //  Dialogue that displays at end of evolution

    void Awake()
    {
        if (!_evolutionService && GameUtils.i != null && GameUtils.i.battleSystem != null)
            _evolutionService = GameUtils.i.battleSystem.BattleEvolutionService;

        if (_monsterObject)
            _monsterImage = _monsterObject.GetComponent<Image>();
    }

    /// <summary>
    /// Disables all the evolution UI objects and checks if there are more
    /// evolutions pending otherwise continue to next state of post battle flow
    /// </summary>
    private void DisableObjects()
    {
        //isAnimating = false;

        if (_background) _background.SetActive(false);
        if (_monsterObject) _monsterObject.SetActive(false);

        // Tell the evolution service that this evolution animation is done
        _evolutionService?.OnEvolutionAnimationFinished();

        // If there are more evolutions in the queue, play the next one.
        if (_evolutionService != null && _evolutionService.HasPendingEvolutions)
        {
            _evolutionService.TryPlayNext();
        }
        else
        {
            // No more evolutions left for this battle:
            // let BattleSystem finish the post-battle flow
            GameUtils.i.battleSystem.CompletePostBattleFlow();
        }
    }

    /// <summary>
    /// Runs the evolution animation
    /// </summary>
    private IEnumerator StartAnimation()
    {
        //isAnimating = true;

        if (_background) _background.SetActive(true);
        if (_monsterObject) _monsterObject.SetActive(true);

        if (_startDelay > 0f)
            yield return new WaitForSeconds(_startDelay);    //  Start animation after delay

        // Phase 1: fade to black
        float phaseDuration = _duration / 3f;
        _monsterImage.DOColor(Color.black, phaseDuration);
        yield return new WaitForSeconds(phaseDuration);

        // Phase 2: flashing between old and new sprite
        float flashPhaseDuration = _duration / 3f;
        float flashInterval = flashPhaseDuration / Mathf.Max(1, _framesCount);

        for (int i = 0; i < _framesCount; i++)
        {
            bool showEvolved = (i % 2 == 0);
            _monsterImage.sprite = showEvolved ? _evolvedSprite : _currentSprite;
            yield return new WaitForSeconds(flashInterval);
        }

        // Ensure final sprite is evolved
        _monsterImage.sprite = _evolvedSprite;

        // Phase 3: fade color back to original
        _monsterImage.DOColor(_originalColor, phaseDuration);
        yield return new WaitForSeconds(phaseDuration);

        // Apply evolution to the party and then trigger dialogue/cleanup
        ApplyEvolution(PlayerStats.i.PlayerParty);
        StopAnimation();
    }

    /// <summary>
    /// Stops the animation and displays dialogue
    /// </summary>
    public void StopAnimation()
    {
        var events = new List<UnityAction> { DisableObjects };

        // Show evolution text, then DisableObjects
        GameUtils.i.dialogueSystem.InitialiseNewText(_evolvedIntoDialogue, "", events);

        // Tell the evolution service we're done with this evolution
        // evolutionService?.OnEvolutionAnimationFinished();
    }

    /// <summary>
    /// Changes the current monster to the evolved version in the party
    /// </summary>
    public void ApplyEvolution(List<BattleMonster> party)
    {
        if (party == null || party.Count == 0 || _monsterToEvolve == null)
            return;

        for (int i = 0; i < party.Count; i++)   //  Search through the party
        {
            if (party[i] == _monsterToEvolve)    //  check if the member is the current evolving monster
            {
                party[i].SetMonster(_monsterToEvolve.Base.evolution.MonsterEvolve);  //  Set that monster to its evolved form
                break;
            }
        }
    }

    /// <summary>
    /// Sets up the animation objects used in evolution animation
    /// </summary>
    public void InitialiseAnimation(BattleMonster monToEvolve)
    {
        if (monToEvolve == null || monToEvolve.Base == null || _monsterImage == null)
            return;

        _evolvedIntoDialogue.Clear();    //  Clears the dialogue to avoid duplications

        _monsterToEvolve = monToEvolve;  //  Sets the current monster to evolve
        _currentSprite = _monsterToEvolve.Base.frontSprite;   //  Sets up the sprite for evolving monster
        _evolvedSprite = _monsterToEvolve.Base.evolution.MonsterEvolve.frontSprite;   //  Sets up the sprite for evolved version of evolving monster

        _monsterImage.sprite = _currentSprite;    //  Sets image sprite to current evolving monster sprite
        _originalColor = _monsterImage.color;

        _evolvedIntoDialogue.Add(monToEvolve.Base.Name + " has evolved into " +
                 _monsterToEvolve.Base.evolution.MonsterEvolve.Name);    //  Sets up the dialogue

        StartCoroutine(StartAnimation());   //  Start animation
    }

}
