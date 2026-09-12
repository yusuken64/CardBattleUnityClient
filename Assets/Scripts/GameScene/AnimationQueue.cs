using CardBattleEngine;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationQueue : MonoBehaviour
{
    public List<GameActionAnimationBase> GameActionAnimations;
    public bool IsStopped;
    public bool IsPlaying { get; private set; }
    public event Action PlaybackFailed;
    private readonly Dictionary<string, GameActionAnimationBase> _map = new();
    private readonly Queue<(ActionPresentation Frame, Action Boundary)> _queue = new();
    private GameActionAnimationBase _active;
    private readonly List<ActionPresentation> _visualFrames = new();
    private void Awake()
    {
        foreach (var animation in GameActionAnimations)
            if (animation != null && !_map.ContainsKey(animation.ActionType.Name))
                _map.Add(animation.ActionType.Name, animation);
    }
    public void EnqueueAnimation(GameManager gm, GameState state, (IGameAction action, ActionContext context) current)
        => Enqueue(gm, ActionPresentation.Capture(gm, state, current));
    public void Enqueue(GameManager gm, ActionPresentation frame)
    {
        _queue.Enqueue((frame, null));
        StartIfNeeded(gm);
    }
    public void EnqueueBoundary(GameManager gm, Action boundary)
    {
        _queue.Enqueue((null, boundary));
        StartIfNeeded(gm);
    }
    private void StartIfNeeded(GameManager gm)
    {
        if (IsPlaying) return;
        IsPlaying = true;
        StartCoroutine(Process(gm));
    }
    private IEnumerator Process(GameManager gm)
    {
        yield return null; // Let the caller enqueue the whole batch first.
        while (_queue.Count > 0)
        {
            while (IsStopped) yield return null;
            var job = _queue.Dequeue();
            Exception failure = null;
            var routines = new Stack<IEnumerator>();
            routines.Push(PlayJob(gm, job.Frame, job.Boundary));
            // Flatten nested routines so exceptions from effects are recoverable too.
            while (routines.Count > 0 && failure == null)
            {
                object next = null;
                try
                {
                    var routine = routines.Peek();
                    if (!routine.MoveNext()) { (routines.Pop() as IDisposable)?.Dispose(); continue; }
                    next = routine.Current;
                }
                catch (Exception ex) { failure = ex; }
                if (failure != null) break;
                if (next is IEnumerator nested) routines.Push(nested);
                else yield return next;
            }
            if (_active != null) Destroy(_active.gameObject);
            _active = null;
            if (failure != null)
            {
                Debug.LogError($"Playback failed at {job.Frame?.Event.Sequence} ({job.Frame?.Event.ActionType}): {failure}");
                _queue.Clear();
                IsPlaying = false;
                PlaybackFailed?.Invoke();
                yield break;
            }
        }
        IsPlaying = false;
        gm.OnPresentationQueueDrained();
    }
    private IEnumerator PlayJob(GameManager gm, ActionPresentation frame, Action boundary)
    {
        if (frame == null) { boundary?.Invoke(); yield break; }
        _visualFrames.RemoveAll(x => !x.HasLiveVisuals);
        _visualFrames.Add(frame);
        if (frame.Effect != null) yield return frame.Effect.Routine(frame);
        gm.ApplyPresentationStatus(frame);
        if (_map.TryGetValue(frame.Event.ActionType, out var prefab))
        {
            _active = Instantiate(prefab, transform);
            _active.Init(gm, frame);
            yield return _active.Play();
        }
        frame.Sync(gm);
    }
    public void Cancel(GameManager gm)
    {
        StopAllCoroutines();
        _queue.Clear();
        IsPlaying = false;
        foreach (var frame in _visualFrames) frame.CancelVisuals();
        _visualFrames.Clear();
        if (_active != null) Destroy(_active.gameObject);
        _active = null;
        if (gm != null)
            foreach (var component in gm.GetComponentsInChildren<Component>(true))
                if (component != null) DOTween.Kill(component);
        // Cancellation/scene teardown must never create a new tween (PreviewEnd does).
        var ui = gm == null ? null : FindFirstObjectByType<UI>();
        if (ui != null)
        {
            if (ui.CardPreview != null) ui.CardPreview.gameObject.SetActive(false);
            if (ui.TurnStartObject != null) ui.TurnStartObject.gameObject.SetActive(false);
        }
    }
    private void OnDisable() => Cancel(null);
}

public abstract class GameActionAnimationBase : MonoBehaviour, IAnimation
{
    public GameManager GameManager { get; private set; }
    public ActionPresentation Presentation { get; private set; }
    public abstract Type ActionType { get; }
    public GameObject GameObject => gameObject;
    public void Init(GameManager gm, ActionPresentation presentation) { GameManager = gm; Presentation = presentation; }
    public abstract IEnumerator Play();
}
public abstract class GameActionAnimation<T> : GameActionAnimationBase where T : IGameAction
{
    public override Type ActionType => typeof(T);
}
public interface IAnimation
{
    IEnumerator Play();
    GameObject GameObject { get; }
}
