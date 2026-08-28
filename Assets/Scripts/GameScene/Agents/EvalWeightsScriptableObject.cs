using System;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewEvalWeights",
    menuName = "Game/AI/EvalWeights"
)]
public class EvalWeightsScriptableObject : ScriptableObject
{
    [Header("Global")]
    public float Bias = 0f;

    [Header("Hero health / win pressure")]
    public float MyHealth = 2.5f;
    public float EnemyHealth = 6f;
    public float EnemyHpBelow15 = 8f;
    public float EnemyHpBelow10 = 15f;
    public float EnemyHpBelow5 = 30f;

    [Header("Board presence")]
    public float MyBoardStats = 1f;
    public float EnemyBoardStats = 1f;

    [Header("Future damage")]
    public float MyNextTurnAttack = 1.5f;
    public float EnemyNextTurnAttack = 2.0f;
    public float EnemyBoardAttack = 1.2f;

    [Header("Taunts")]
    public float EnemyTauntHealth = 1.5f;
    public float MyTauntHealth = 1.5f;

    [Header("Card advantage")]
    public float MyHandSize = 1.5f;
    public float EnemyHandSize = 1.5f;

    [Header("Immediate damage")]
    public float MyReadyAttack = 1.2f;

    [Header("Mana")]
    public float ManaAdvantage = 5f;

    [Header("Lethal")]
    public float HasLethalBonus = 100000f;
    public float EnemyHasLethalPenalty = 100000f;

    // ----- Minion weights -----
    [Header("Minion Base Stats")]
    public float MinionAttack = 1.6f;
    public float MinionHealth = 1.2f;
    public float MinionBonusHealth = 0.3f;

    [Header("Minion Tempo")]
    public float ReadyNextTurnBonus = 1.2f;
    public float SummoningSickPenalty = 0.6f;

    [Header("Minion Keywords")]
    public float DivineShield = 1.3f;
    public float TauntMinionHealth = 1.5f;
    public float TauntFlatBonus = 2.5f;

    public float PoisonousBase = 6f;
    public float PoisonousAttack = 1.5f;

    public float Windfury = 1.4f;
    public float StealthAttack = 1.8f;
    public float StealthFlat = 3f;
    public float Lifesteal = 1.0f;

    public float RebornAttack = 1.2f;
    public float RebornHealth = 1.2f;
    public float RebornFlat = 4f;

    public EvalWeights Create()
    {
        return new EvalWeights
        {
            Bias = Bias,

            MyHealth = MyHealth,
            EnemyHealth = EnemyHealth,
            EnemyHpBelow15 = EnemyHpBelow15,
            EnemyHpBelow10 = EnemyHpBelow10,
            EnemyHpBelow5 = EnemyHpBelow5,

            MyBoardStats = MyBoardStats,
            EnemyBoardStats = EnemyBoardStats,

            MyNextTurnAttack = MyNextTurnAttack,
            EnemyNextTurnAttack = EnemyNextTurnAttack,
            EnemyBoardAttack = EnemyBoardAttack,

            EnemyTauntHealth = EnemyTauntHealth,
            MyTauntHealth = MyTauntHealth,

            MyHandSize = MyHandSize,
            EnemyHandSize = EnemyHandSize,

            MyReadyAttack = MyReadyAttack,
            ManaAdvantage = ManaAdvantage,

            HasLethalBonus = HasLethalBonus,
            EnemyHasLethalPenalty = EnemyHasLethalPenalty,

            MinionAttack = MinionAttack,
            MinionHealth = MinionHealth,
            MinionBonusHealth = MinionBonusHealth,

            ReadyNextTurnBonus = ReadyNextTurnBonus,
            SummoningSickPenalty = SummoningSickPenalty,

            DivineShield = DivineShield,
            TauntMinionHealth = TauntMinionHealth,
            TauntFlatBonus = TauntFlatBonus,

            PoisonousBase = PoisonousBase,
            PoisonousAttack = PoisonousAttack,

            Windfury = Windfury,
            StealthAttack = StealthAttack,
            StealthFlat = StealthFlat,
            Lifesteal = Lifesteal,

            RebornAttack = RebornAttack,
            RebornHealth = RebornHealth,
            RebornFlat = RebornFlat
        };
    }
}

[Serializable]
public class EvalWeights
{
    // Global bias (baseline score)
    public float Bias = 0f;

    // Hero health / win pressure
    public float MyHealth = 2.5f;
    public float EnemyHealth = 6f;
    public float EnemyHpBelow15 = 8f;
    public float EnemyHpBelow10 = 15f;
    public float EnemyHpBelow5 = 30f;

    // Board presence
    public float MyBoardStats = 1f;
    public float EnemyBoardStats = 1f;

    // Future damage
    public float MyNextTurnAttack = 1.5f;
    public float EnemyNextTurnAttack = 2.0f;
    public float EnemyBoardAttack = 1.2f;

    // Taunts
    public float EnemyTauntHealth = 1.5f;
    public float MyTauntHealth = 1.5f;

    // Card advantage
    public float MyHandSize = 1.5f;
    public float EnemyHandSize = 1.5f;

    // Immediate damage
    public float MyReadyAttack = 1.2f;

    // Mana advantage
    public float ManaAdvantage = 5f;

    // Lethal bonuses (keep large but tunable)
    public float HasLethalBonus = 100_000f;
    public float EnemyHasLethalPenalty = 100_000f;

    // -------- Minion value weights --------
    public float MinionAttack = 1.6f;
    public float MinionHealth = 1.2f;
    public float MinionBonusHealth = 0.3f;

    public float ReadyNextTurnBonus = 1.2f;
    public float SummoningSickPenalty = 0.6f;

    public float DivineShield = 1.3f;
    public float TauntMinionHealth = 1.5f;
    public float TauntFlatBonus = 2.5f;

    public float PoisonousBase = 6f;
    public float PoisonousAttack = 1.5f;

    public float Windfury = 1.4f;
    public float StealthAttack = 1.8f;
    public float StealthFlat = 3f;

    public float Lifesteal = 1.0f;

    public float RebornAttack = 1.2f;
    public float RebornHealth = 1.2f;
    public float RebornFlat = 4f;
}
