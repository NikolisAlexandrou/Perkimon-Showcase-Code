using UnityEngine;

[CreateAssetMenu(fileName= "BerserkAbility", menuName = "Abilities/Berserk")]
public class BerserkAbility : Abilities
{
    public StatIncreaseEffect StatIncreaseSelf;

    public override void OnEnemyFainted(AbilityContext ctx)
    {
        ctx.User.StartCoroutine(ctx.User.ChangeStatsSelf(StatIncreaseSelf));
    }
}
