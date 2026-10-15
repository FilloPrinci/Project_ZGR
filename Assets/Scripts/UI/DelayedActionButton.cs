using System.Collections;
using UnityEngine;

// NOTE: deliberately no "using UnityEngine.UI;" - this project has its own global-namespace
// "Button" class (Assets/Scripts/UI/Custom3D_UI/LogicComponents/Button.cs, for the 3D menu),
// which would otherwise shadow UnityEngine.UI.Button for an unqualified "Button" below. Every
// UI type here is fully qualified instead.

// Drop-in replacement for UnityEngine.UI.Button, used globally across the project's standard UI
// buttons (pause menu, results screen, options, main menu). Same onClick wiring/navigation as a
// regular Button - only the interactive-state visuals and click timing differ, unified here so
// every button in the game behaves the same way:
//   Normal      - as dark as possible (plain fill, no border).
//   Highlighted/Selected (hover, keyboard/gamepad focus) - fill stays dark, only the border lights up.
//   Pressed     - fills entirely with the accent color.
// accentColor is per-button (not a shared constant) - it's meant to carry whatever semantic color
// that button already had before migrating to this type (e.g. green Continue, red Quit), not one
// flat color for every button in the game.
// The actual onClick action is delayed by pressDelay after the press is registered, so the
// pressed feedback has time to read before anything happens.
//
// IMPORTANT when migrating an existing Button to this type in code: remove the old Button
// component BEFORE adding this one, never after. Adding this (or any Button subclass) to a
// GameObject that still has another UnityEngine.UI.Button on it throws a NullReferenceException
// out of Selectable.OnEnable (confirmed live via the Editor) - harmless with the old component
// gone first.
//
// transition is forced to None in Awake() because DoStateTransition below drives the fill color
// directly - Selectable's built-in ColorTint/SpriteSwap/Animation transitions would just fight it.
public class DelayedActionButton : UnityEngine.UI.Button
{
    [Header("State colors")]
    public Color normalColor = new Color(0.1254902f, 0.1254902f, 0.1254902f, 1f);
    [Tooltip("This button's own accent color (border on hover/selected, full fill when pressed) - carries over whatever color this button used to highlight with, not a shared project-wide color.")]
    public Color accentColor = new Color(0.0627451f, 0.2509804f, 0.2509804f, 1f);

    [Header("Border (hover/selected)")]
    public float borderThickness = 3f;

    [Header("Delayed action")]
    [Tooltip("Seconds between the press being registered and onClick actually firing.")]
    public float pressDelay = 0.5f;

    // Children ALWAYS render in front of their parent's own Graphic in Unity UI, regardless of
    // sibling index - confirmed live: a child "border" sized larger than the button rendered IN
    // FRONT of and completely covered the button's own fill whenever enabled (looked like a full
    // solid color, not a frame). There's no way to make a child render behind a Graphic that's a
    // component on its own parent. So this is inverted instead: targetGraphic (the button's own,
    // original Image - already correctly positioned, no RectTransform surgery needed) now IS the
    // outer/border layer, and a child Image INSET by borderThickness is the inner fill - as a
    // child it naturally renders in front, so the outer layer's color only ever peeks out as a
    // ring around it, exactly the border effect wanted, with zero z-order tricks.
    private const string FillChildName = "_Fill";
    private UnityEngine.UI.Image fill;

    private UnityEngine.UI.Image Fill
    {
        get
        {
            if (fill == null)
            {
                Transform existing = transform.Find(FillChildName);
                GameObject fillGo = existing != null
                    ? existing.gameObject
                    : new GameObject(FillChildName, typeof(RectTransform), typeof(UnityEngine.UI.Image));

                fillGo.transform.SetParent(transform, false);
                // First sibling: behind any existing children (e.g. the button's own "Text"
                // label), but still in front of the outer/border layer (the parent's own
                // Graphic, always behind every child regardless of sibling order) - putting it
                // last instead covered the label entirely (confirmed live: button text vanished).
                fillGo.transform.SetAsFirstSibling();

                RectTransform rt = (RectTransform)fillGo.transform;
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.localScale = Vector3.one;
                rt.localRotation = Quaternion.identity;

                fill = fillGo.GetComponent<UnityEngine.UI.Image>();
                fill.raycastTarget = false; // purely visual - never steal clicks from the button

                ApplyFillSettings();
            }
            return fill;
        }
    }

    protected override void Awake()
    {
        base.Awake();

        transition = Transition.None;
        ApplyFillSettings();

        // Establish the resting (Normal) look immediately instead of waiting for the first
        // pointer/selection event.
        DoStateTransition(SelectionState.Normal, true);
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        if (transform.Find(FillChildName) != null)
        {
            ApplyFillSettings();
        }
    }
#endif

    private void ApplyFillSettings()
    {
        UnityEngine.UI.Image f = Fill;
        f.rectTransform.offsetMin = new Vector2(borderThickness, borderThickness);
        f.rectTransform.offsetMax = new Vector2(-borderThickness, -borderThickness);

        // Match the button's own sprite/type so the inset fill follows the same shape (e.g.
        // rounded corners from a Sliced sprite) as the outer/border layer.
        if (targetGraphic is UnityEngine.UI.Image mainImage)
        {
            f.sprite = mainImage.sprite;
            f.type = mainImage.type;
            f.pixelsPerUnitMultiplier = mainImage.pixelsPerUnitMultiplier;
        }
    }

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        if (targetGraphic == null) return;

        // Set directly rather than through CrossFadeColor/TweenRunner: that path silently no-ops
        // whenever the Graphic isn't considered "active and enabled" yet by Unity's internal
        // tween runner (confirmed live - the fill color never actually changed through it, even
        // across Normal/Highlighted/Pressed), which is exactly the unreliable territory this
        // method gets called from (Awake, OnEnable, OnValidate, straight after AddComponent).
        // Direct assignment has no such precondition.
        //
        // targetGraphic is now the OUTER/border layer, Fill is the INNER layer sitting on top of
        // it - same color on both = uniformly flat (no visible ring); outer=accent/inner=normal
        // is what actually produces the border-only look.
        switch (state)
        {
            case SelectionState.Normal:
                targetGraphic.color = normalColor;
                Fill.color = normalColor;
                break;
            case SelectionState.Highlighted:
            case SelectionState.Selected:
                targetGraphic.color = accentColor;
                Fill.color = normalColor;
                break;
            case SelectionState.Pressed:
                targetGraphic.color = accentColor;
                Fill.color = accentColor;
                break;
            case SelectionState.Disabled:
                targetGraphic.color = colors.disabledColor;
                Fill.color = colors.disabledColor;
                break;
        }
    }

    // Button's own click-to-invoke path (Press()) is private, so it can't be overridden - both
    // OnPointerClick (mouse/touch) and OnSubmit (keyboard/gamepad) are reimplemented here instead
    // of calling base, so neither invokes onClick immediately. isPressPending guards against a
    // second click/submit re-triggering the action while one is already waiting out its delay.
    private bool isPressPending;

    public override void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (eventData.button != UnityEngine.EventSystems.PointerEventData.InputButton.Left) return;
        if (!IsActive() || !IsInteractable() || isPressPending) return;

        StartCoroutine(PressHoldThenInvoke());
    }

    public override void OnSubmit(UnityEngine.EventSystems.BaseEventData eventData)
    {
        if (!IsActive() || !IsInteractable() || isPressPending) return;

        StartCoroutine(PressHoldThenInvoke());
    }

    private IEnumerator PressHoldThenInvoke()
    {
        isPressPending = true;
        DoStateTransition(SelectionState.Pressed, true);

        yield return new WaitForSecondsRealtime(pressDelay);

        isPressPending = false;

        // The button may have been disabled or deactivated during the wait (e.g. the panel it's
        // on got hidden) - don't fire into that state, and don't fight whatever visual that state
        // change already applied.
        if (IsActive() && IsInteractable())
        {
            DoStateTransition(currentSelectionState, true);
            onClick.Invoke();
        }
    }
}
