using UnityEngine;

/// <summary>
/// Describes a detachable UI control (a button, slider, etc.) that can be
/// removed from a source panel, carried by the player, and installed into a target.
/// Replaces the old ModuleData concept.
/// </summary>
[CreateAssetMenu(fileName = "NewUIControl", menuName = "TGC/UI Control")]
public class UIControlData : ScriptableObject
{
    [Header("Control Info")]
    [Tooltip("Display name shown to the player (e.g. 'Plus Button', 'Volume Dial').")]
    public string displayName = "Plus Button";

    [Tooltip("Internal ID used by targets to check compatibility.")]
    public string controlID = "plus";

    [Tooltip("Short description of what this control does.")]
    public string description = "Increases a value.";

    [Tooltip("Icon or visual reference for this control (optional).")]
    public Sprite icon;

    [Tooltip("Color used for the 3D button prop when carried.")]
    public Color controlColor = Color.white;
}
