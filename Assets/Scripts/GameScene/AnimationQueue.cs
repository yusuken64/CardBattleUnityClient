using CardBattleEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnimationQueue : MonoBehaviour
{
    public List<GameActionAnimationBase> GameActionAnimations;

    public bool IsStopped; // used in tutorial to stop animations

    private Dictionary<Type, GameActionAnimationBase> animationMap;
    private Queue<IAnimation> queue = new();
    private bool isPlaying = false;

    private void Awake()
    {
        animationMap = new Dictionary<Type, GameActionAnimationBase>();

        foreach (var anim in GameActionAnimations)
        {
            if (anim == null)
                continue;

            var actionType = anim.ActionType;

            if (!animationMap.ContainsKey(actionType))
            {
                animationMap.Add(actionType, anim);
            }
            else
            {
                Debug.LogWarning($"Duplicate animation for {actionType.Name}. Keeping first one.");
            }
        }
    }

    public void EnqueueAnimation(GameManager gameManager, GameState state, (IGameAction action, ActionContext context) current)
    {
        IAnimation newAnimation = CreateAnimationForAction(gameManager, state, current);
        if (newAnimation is null)
        {
            Debug.Log($"No animation for {current.action}");
            return;
        }
        queue.Enqueue(newAnimation);

        if (!isPlaying)
            StartCoroutine(ProcessQueue());
    }

    private IAnimation CreateAnimationForAction(
        GameManager gm,
        GameState state,
        (IGameAction action, ActionContext context) current)
    {
        var actionType = current.action.GetType();

        if (!animationMap.TryGetValue(actionType, out var prefab))
        {
            return null;
        }

        var instance = Instantiate(prefab, transform);
        var newContext = current.context.ShallowCopy();
        current.context = newContext;

        instance.Init(gm, state.Clone(), current);
        return instance;
    }

    private IEnumerator ProcessQueue()
    {
        isPlaying = true;

        while (queue.Count > 0)
        {
            while (IsStopped)
            {
                yield return null;
            }

            IAnimation anim = queue.Dequeue();
            var sfxRoutine = anim.CustomSFX();
            if (sfxRoutine != null)
            {
                yield return StartCoroutine(sfxRoutine);
            }

            yield return StartCoroutine(anim.ApplyStatus());
            yield return StartCoroutine(anim.Play());
            anim.SyncData();
#if UNITY_EDITOR
            var animation = (GameActionAnimationBase)anim;
            //Debug.Log($"Validate {animation.Action.GetType().Name}");

            if (animation.Context.SourcePlayer == null)
            {
                Debug.Log($"Source player null for {animation.Action.GetType().Name}");
                continue;
            }
            var playerData = animation.ClonedState.Players.First(x => x.Id == animation.Context.SourcePlayer.Id);
            var player = animation.GameManager.GetPlayerFor(playerData);
            GameManager.ValidateState(playerData, player);
#endif
            if (anim != null) Destroy(anim.GameObject);
        }

        isPlaying = false;
    }
}

public abstract class GameActionAnimationBase : MonoBehaviour, IAnimation
{
    public GameManager GameManager { get; private set; }
    public GameState ClonedState { get; private set; }
    public IGameAction Action { get; set; }
    public ActionContext Context { get; private set; }
    public abstract Type ActionType { get; }

    public GameObject GameObject => this.gameObject;

    public abstract IEnumerator Play();

    public virtual void Init(GameManager gm, GameState clonedState, (IGameAction action, ActionContext context) current)
    {
        GameManager = gm;
        ClonedState = clonedState;
        Action = current.action;
        Context = current.context;
    }

    public abstract IEnumerator CustomSFX();
    public abstract IEnumerator ApplyStatus();
    public abstract void SyncData();
}

public abstract class GameActionAnimation<T> : GameActionAnimationBase where T : IGameAction
{
    public override Type ActionType => typeof(T);
	protected new T Action { get; private set; }

	public override void Init(GameManager gm, GameState state, (IGameAction action, ActionContext context) current)
    {
        base.Init(gm, state, current);
        Action = (T)current.action;
    }

	public override IEnumerator CustomSFX()
    {
		if (Action.CustomSFX == null)
			yield break;

        CustomSFX SFXScriptableObject = Action.CustomSFX as CustomSFX;
        if (SFXScriptableObject == null)
            yield break;

        yield return SFXScriptableObject.Routine(Action, Context);
	}

    public override IEnumerator ApplyStatus()
    {
        foreach (var change in Context.ResolvedStatusChanges)
		{
            var target = GameManager.GetObjectFor(change.Target);
            if (target == null) { continue; }

            //TODO: Have each IGameEntity implement a view interface like IStatusView
            //and call ApplyStatusDelta on it
            Minion minion = target.GetComponent<Minion>();
			switch (change.Status)
			{
				case StatusType.Freeze:
					minion.IsFrozen = change.Gained;
					break;
				case StatusType.Stealth:
					minion.HasStealth = change.Gained;
                    break;
			}
            minion.UpdateUI();
		}

		yield return null;
	}

	public override void SyncData()
	{
        var allEnitities = ClonedState.GetAllEntities();
        foreach (var entity in allEnitities)
        {
            var unityEntity = GameManager.GetObjectByID(entity.Id);
            if (unityEntity?.Entity == null) { continue; }
            unityEntity?.SyncData(entity);
        }
	}
}

public interface IAnimation
{
	IEnumerator Play();
    IEnumerator CustomSFX();
    IEnumerator ApplyStatus();
    void SyncData();

    GameObject GameObject { get; }
}