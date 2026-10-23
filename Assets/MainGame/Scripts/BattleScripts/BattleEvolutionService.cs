using System.Collections.Generic;
using UnityEngine;

public class BattleEvolutionService : MonoBehaviour
{
    [Tooltip("UI object that will play animation of evolution")]
    [SerializeField] private EvolutionAnimation _evolutionAnimation;

    readonly Queue<BattleMonster> _pending = new Queue<BattleMonster>(); //  Holds all evolutions if any in a queue

    public bool HasPendingEvolutions => _pending.Count > 0;  //  Flag to know if there are any more evolutions pending
    public bool IsAnimating { get; private set; }   //  Flag when The evolution animation is playing

    void Awake()
    {
        if (!_evolutionAnimation)
            _evolutionAnimation = FindAnyObjectByType<EvolutionAnimation>(); //  Assign the evolution Animation script if not assigned in inspector
    }

    // Called from BattleSystem.EnqueueEvolutionCandidate
    /// <summary>
    /// Adds monster to be evolved in the evolution queue
    /// </summary>
    public void EnqueueEvolution(BattleMonster mon)
    {
        if (mon == null) return;
        _pending.Enqueue(mon);

        // IMPORTANT:
        // Do NOT call TryPlayNext() here anymore.
        // We only want to start evolutions when BattleSystem
        // says the post-battle flow is ready.
    }

    // Called from BattleSystem.EvolveMonster()
    /// <summary>
    /// When current evolution finishes, try get the next monster to evolve
    /// </summary>
    public void TryPlayNext()
    {
        if (IsAnimating) return;
        if (_pending.Count == 0) return;
        if (!_evolutionAnimation) return;

        var mon = _pending.Dequeue();
        IsAnimating = true;
        _evolutionAnimation.InitialiseAnimation(mon);
    }

    // Called by EvolutionAnimation.StopAnimation()
    /// <summary>
    /// Sets evolution animation flag to false
    /// </summary>
    public void OnEvolutionAnimationFinished()
    {
        IsAnimating = false;
        // BattleSystem.EvolveMonster() will be called from EvolutionAnimation
        // after the dialogue, and that will decide whether to call TryPlayNext
        // again or move on to end-of-battle dialogue / catching.
    }

    /// <summary>
    /// Clears all pending evolutions and resets animation state
    /// </summary>
    public void ResetQueue()
    {
        _pending.Clear();
        IsAnimating = false;
    }
}
