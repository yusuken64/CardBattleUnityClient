using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PointerInput : MonoBehaviour
{
    // -------- EVENTS ----------
    public event Action<Vector2> OnClick;
    public event Action<Vector2> OnHoverStart;
    public event Action<Vector2> OnHoverEnd;
    public event Action<Vector2> OnHoverMove;
    public event Action<Vector2> OnDragStart;
    public event Action<Vector2> OnDrag;
    public event Action<Vector2> OnDragEnd;
    public event Action<Vector2> OnRightClick;

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
    private bool pressedInViewport;

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 pos = mouse.position.ReadValue();
        bool inside = GameViewport.Contains(pos);
        if (!inside)
        {
            hoverCooldown = 0;
            if (hovering) { hovering = false; OnHoverEnd?.Invoke(pos); }
        }
        bool moved = (pos - lastPos).magnitude > moveThreshold;

        // ---------------- HOVER ----------------
        if (inside && !mouse.leftButton.isPressed)
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
            pressedInViewport = inside;
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
        if (mouse.leftButton.isPressed && pressedInViewport)
        {
            // Drag start
            float scaledDragThreshold = dragThreshold * GameViewport.Pixels.height / 1080f;
            if (inside && !dragging && Vector2.Distance(pos, startPos) > scaledDragThreshold)
            {
                dragging = true;
                OnDragStart?.Invoke(pos);
            }

            // Drag update
            if (dragging)
            {
                OnDrag?.Invoke(pos);
            }
        }

        // ---------------- BUTTON UP ----------------
        if (mouse.leftButton.wasReleasedThisFrame && pressedInViewport)
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
            else if (inside && heldTime < clickTime)
            {
                OnClick?.Invoke(pos);
            }
            pressedInViewport = false;
        }
        
        // ---------------- RIGHT CLICK ----------------
        if (inside && mouse.rightButton.wasPressedThisFrame)
        {
            OnRightClick?.Invoke(pos);
        }

        // ---------------- HOVER END (mouse stopped / left area) ----------------
        if (hovering && moved)
        {
            //hovering = false;
            //hoverCooldown = 0;
            OnHoverMove?.Invoke(pos);
        }

        lastPos = pos;
    }
}
