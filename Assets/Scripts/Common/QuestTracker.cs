using CardBattleEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuestTracker : MonoBehaviour
{
    public List<QuestDefinition> QuestDefinitions;

    private GameManager _currentManager;
    private GameEngine _currentEngine;

    private List<QuestEffect> _inProgressQuestEffects = new();
    private Dictionary<string, QuestProgress> _questProgressMap = new();

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnhookManager();
        UnhookEngine();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HookIntoScene();
    }

    private void HookIntoScene()
    {
        UnhookManager();
        RebuildInProgressQuests();

        _currentManager = FindFirstObjectByType<GameManager>();

        if (_currentManager != null)
        {
            _currentManager.GameInitialized += OnGameInitialized;
        }
    }

    private void OnGameInitialized(GameEngine engine)
    {
        UnhookEngine();

        _currentEngine = engine;

        _currentEngine.ActionPlaybackCallback += GameEngineActionPlayback;
        _currentEngine.ActionResolvedCallback += GameEngineActionResolved;
    }

    private void UnhookManager()
    {
        if (_currentManager != null)
        {
            _currentManager.GameInitialized -= OnGameInitialized;
            _currentManager = null;
        }
    }

    private void UnhookEngine()
    {
        if (_currentEngine != null)
        {
            _currentEngine.ActionPlaybackCallback -= GameEngineActionPlayback;
            _currentEngine.ActionResolvedCallback -= GameEngineActionResolved;
            _currentEngine = null;
        }
    }

    private void GameEngineActionResolved(GameState gameState)
    {
        // quest logic here
    }

    private void RebuildInProgressQuests()
    {
        GameSaveData gameSaveData = Common.Instance.SaveManager.SaveData.GameSaveData;
        if (gameSaveData == null) { return; }

        _questProgressMap = gameSaveData.QuestSaveData
           .QuestProgressList
           .Where(x => !x.Collected)
           .ToDictionary(x => x.questId);

        _inProgressQuestEffects.Clear();

        foreach (var quest in QuestDefinitions)
        {
            if (!IsInProgressQuest(quest))
                continue;

            _inProgressQuestEffects.Add(new QuestEffect
            {
                questDefinition = quest,
                effect = quest.QuestEffectWrapper.CreateEffect()
            });
        }
    }

    private void GameEngineActionPlayback(GameState gameState, (IGameAction action, ActionContext context) current)
    {
        // increment matching quests
        foreach (var quest in _inProgressQuestEffects)
        {
            if (quest.effect.EffectTrigger == current.action.EffectTrigger)
            {
                var questPlayer = gameState.Players[0];
                var context = current.context;

                var effectContext = new ActionContext()
                {
                    SourcePlayer = questPlayer.Entity.Owner,
                    Source = questPlayer.Entity,
                    Target = context.Target,
                    SummonedMinion = context.SummonedMinion,
                    PlayIndex = context.PlayIndex,
                    SourceCard = context.SourceCard,
                    OriginalAction = context.OriginalAction,
                    OriginalSource = context.Source
                };

                if (quest.effect.Condition != null &&
                    !quest.effect.Condition.Evaluate(effectContext))
                {
                    continue;
                }

                IncrementQuest(quest);
            }
        }
    }

    private bool IsInProgressQuest(QuestDefinition quest)
    {
        if (!_questProgressMap.TryGetValue(quest.QuestId, out var progress))
            return false;

        return progress.questProgress < quest.MaxProgres;
    }

    private void IncrementQuest(QuestEffect quest)
    {
        QuestProgress questProgress = Common.Instance.SaveManager.SaveData.GameSaveData.QuestSaveData
            .QuestProgressList.FirstOrDefault(x => x.questId == quest.questDefinition.QuestId);

        if (questProgress.questProgress >= quest.questDefinition.MaxProgres)
        {
            return;
        }
        questProgress.questProgress++;

        if (questProgress.questProgress == quest.questDefinition.MaxProgres)
        {
            ///TODO show quest complete;
        }
    }
}

public class QuestEffect
{
    public QuestDefinition questDefinition;
    public TriggeredQuestEffect effect;
}