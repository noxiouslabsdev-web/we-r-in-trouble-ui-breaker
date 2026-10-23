using UnityEngine;

/// <summary>
/// ScriptableObject describing a UI Module (Volume, Brightness, Settings, etc.)
/// Lightweight data container — no complex behavior.
/// </summary>
[CreateAssetMenu(fileName = "NewModule", menuName = "TGC/UI Module")]
public class ModuleData : ScriptableObject
{
    [Header("Module Info")]
    [Tooltip("Display name shown to the player.")]
    public string displayName = "Volume Module";

    [Tooltip("Internal ID used by sockets to check compatibility.")]
    public string moduleID = "volume";

    [Tooltip("Short description shown in HUD or pickup prompt.")]
    public string description = "Controls audio volume levels.";

    [Tooltip("Icon sprite for HUD display (optional).")]
    public Sprite icon;
}
