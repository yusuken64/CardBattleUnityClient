using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PointerInput : MonoBehaviour
{
    // -------- EVENTS ----------
    public event Action<Vector2> OnClick;
    public event Action<Vector2> OnHoldStart;
    public event Action<Vector2> OnHoldEnd;
    public event Action<Vector2> OnDragStart;
    public event Action<Vector2> OnDrag;
    public event Action<Vector2> OnDragEnd;

    // -------- SETTINGS ----------
    public float clickTime = 0.2f;
    public float dragThreshold = 10f;

    // -------- INTERNAL STATE ----------
    private Vector2 startPos;
    private float downTime;
    private bool dragging = false;
    private bool holdStarted = false;

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 pos = mouse.position.ReadValue();

        // ---------------- BUTTON DOWN ----------------
        if (mouse.leftButton.wasPressedThisFrame)
        {
            startPos = pos;
            downTime = Time.time;
            dragging = false;
            holdStarted = false;
        }

        // ---------------- BUTTON HELD ----------------
        if (mouse.leftButton.isPressed)
        {
            // Drag start
            if (!dragging && Vector2.Distance(pos, startPos) > dragThreshold)
            {
                dragging = true;
                OnDragStart?.Invoke(pos);
                OnHoldEnd?.Invoke(pos);
            }

            // Drag update
            if (dragging)
            {
                OnDrag?.Invoke(pos);
                OnHoldEnd?.Invoke(pos);
            }

            // Hold start (if not dragging)
            float heldTime = Time.time - downTime;
            if (!holdStarted && !dragging && heldTime >= clickTime)
            {
                holdStarted = true;
                OnHoldStart?.Invoke(pos); //TODO instantly invoke with right click
            }
        }

        // ---------------- BUTTON UP ----------------
        if (mouse.leftButton.wasReleasedThisFrame)
        {
            float heldTime = Time.time - downTime;

            if (dragging)
            {
                OnDragEnd?.Invoke(pos);
            }
            else if (holdStarted)
            {
                OnHoldEnd?.Invoke(pos);
            }
            else if (heldTime < clickTime)
            {
                OnClick?.Invoke(pos);
            }
        }
    }
}
