using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PointerInput : MonoBehaviour
{
    // -------- EVENTS ----------
    public event Action<Vector2> OnClick;
    public event Action<Vector2> OnHoverStart;
    public event Action<Vector2> OnHoverEnd;
    public event Action<Vector2> OnDragStart;
    public event Action<Vector2> OnDrag;
    public event Action<Vector2> OnDragEnd;

    // -------- SETTINGS ----------
    public float clickTime = 0.2f;
    public float dragThreshold = 10f;
    public float moveThreshold = 0.01f;

    // -------- INTERNAL STATE ----------
    private Vector2 startPos;
    private float downTime;
    private bool dragging = false;
    private bool holdStarted = false;
    private bool hovering;
    private Vector2 lastPos;

    private float hoverCooldown;

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 pos = mouse.position.ReadValue();
        bool moved = (pos - lastPos).magnitude > moveThreshold;

        // ---------------- HOVER ----------------
        if (!mouse.leftButton.isPressed)
        {
            if (!moved && !hovering && !dragging)
            {
                hoverCooldown += Time.deltaTime;
                if (hoverCooldown >= 0.3f)
                {
                    hoverCooldown = 0;
                    hovering = true;
                    OnHoverStart?.Invoke(pos);
                }
            }
        }

        // ---------------- BUTTON DOWN ----------------
        if (mouse.leftButton.wasPressedThisFrame)
        {
            startPos = pos;
            downTime = Time.time;
            dragging = false;
            holdStarted = false;

            if (hovering)
            {
                hovering = false;
                OnHoverEnd?.Invoke(pos);
            }
        }

        // ---------------- BUTTON HELD ----------------
        if (mouse.leftButton.isPressed)
        {
            // Drag start
            if (!dragging && Vector2.Distance(pos, startPos) > dragThreshold)
            {
                dragging = true;
                OnDragStart?.Invoke(pos);
            }

            // Drag update
            if (dragging)
            {
                OnDrag?.Invoke(pos);
            }

            // Hold start (if not dragging)
            //float heldTime = Time.time - downTime;
            //if (!holdStarted && !dragging && heldTime >= clickTime)
            //{
            //    holdStarted = true;
            //    OnHoverStart?.Invoke(pos); //TODO instantly invoke with right click
            //}
        }

        // ---------------- BUTTON UP ----------------
        if (mouse.leftButton.wasReleasedThisFrame)
        {
            float heldTime = Time.time - downTime;

            if (dragging)
            {
                dragging = false;
                OnDragEnd?.Invoke(pos);
            }
            else if (holdStarted)
            {
                OnHoverEnd?.Invoke(pos);
            }
            else if (heldTime < clickTime)
            {
                OnClick?.Invoke(pos);
            }
        }
        
        // ---------------- HOVER END (mouse stopped / left area) ----------------
        if (!mouse.leftButton.isPressed && hovering && moved)
        {
            hovering = false;
            hoverCooldown = 0;
            OnHoverEnd?.Invoke(pos);
        }

        lastPos = pos;
    }
}
