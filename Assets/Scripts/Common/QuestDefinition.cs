using CardBattleEngine;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewQuest",
    menuName = "Game/Quest/Quest Definition"
)]
public class QuestDefinition : ScriptableObject
{
    public string QuestId;
    public string QuestTitle;
    public string QuestObjective;
    public string QuestDescription;
    public string QuestOutcome;
    public int MaxProgress;
    public QuestPrereq QuestPrereq;
    public QuestEffectWrapper QuestEffectWrapper;
}

[Serializable]
public class QuestPrereq
{
    public List<string> PrereqQuestIds;
}

[Serializable]
public class QuestEffectWrapper
{
    public string Description;
    public EffectTrigger EffectTrigger;

    [SerializeReference]
    public ITriggerConditionWrapperBase Condition;

    // The real TriggeredEffect instance is generated at runtime
    public TriggeredQuestEffect CreateEffect()
    {
        TriggeredQuestEffect effect = new TriggeredQuestEffect()
        {
            EffectTrigger = this.EffectTrigger,
            Condition = Condition?.Create(),
        };

        return effect;
    }
}

public class TriggeredQuestEffect
{
    public EffectTrigger EffectTrigger;
    public ITriggerCondition Condition;
}