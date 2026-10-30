using System.Collections.Generic;
using UnityEngine;

// One line of the credits: a role ("Game Design") and the people credited for it. Leave
// role empty for a names-only line. Separate names with commas or new lines: each one is
// shown on its own line.
[System.Serializable]
public class CreditsEntry
{
    public string role;
    [TextArea(3, 6)] public string names;
}

// A block of the credits under a common heading ("Programming", "Music", "Special Thanks").
// Every field is optional: an empty title shows no heading, a null image shows no image.
[System.Serializable]
public class CreditsSection
{
    public string title;
    public Sprite image;
    public float imageHeight = 160f;
    public List<CreditsEntry> entries = new List<CreditsEntry>();
}

// Pure data, filled in the Inspector: what the end credits say and how fast they scroll.
// EndingSequenceManager reads it and builds the scrolling list from the scene's templates, so
// changing the credits never needs touching the scene hierarchy.
public class CreditsConfig : MonoBehaviour
{
    [Header("Opening")]
    public Sprite logo;
    public float logoHeight = 220f;
    public string gameTitle = "Project ZGR";

    [Header("Sections")]
    public List<CreditsSection> sections = new List<CreditsSection>();

    [Header("Closing")]
    // Last line of the credits; the scroll stops with it centered on screen instead of
    // scrolling it away. Leave empty to scroll everything off screen.
    [TextArea(1, 4)] public string closingMessage = "Thanks for playing!";

    [Header("Layout")]
    public float sectionSpacing = 120f;
    public float entrySpacing = 30f;

    [Header("Timing")]
    // Pixels per second, in the credits canvas' reference resolution.
    public float scrollSpeed = 90f;
    // Scroll speed multiplier while the player holds any button.
    public float fastForwardMultiplier = 5f;
    // How long the closing message stays centered before going back to the menu.
    public float endHoldDuration = 3f;
}
