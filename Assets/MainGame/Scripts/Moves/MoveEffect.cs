using UnityEngine;

[CreateAssetMenu(fileName = "MoveEffect", menuName = "Battle Moves/MoveEffect")]
public abstract class MoveEffect : ScriptableObject
{
    public virtual void OnMoveUsed(MoveContext context) { }

    public virtual void OnMoveHitTarget(MoveContext context) { }
}
