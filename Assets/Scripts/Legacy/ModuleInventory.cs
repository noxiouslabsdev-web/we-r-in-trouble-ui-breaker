using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks which UI Modules the player has collected.
/// Simple list-based inventory — no weight, no slots.
/// </summary>
public class ModuleInventory : MonoBehaviour
{
    [Tooltip("Modules currently held by the player.")]
    public List<ModuleData> modules = new List<ModuleData>();

    /// <summary>
    /// Add a module to inventory.
    /// </summary>
    public void AddModule(ModuleData module)
    {
        if (module != null && !modules.Contains(module))
        {
            modules.Add(module);
            Debug.Log($"[ModuleInventory] Picked up: {module.displayName}");
        }
    }

    /// <summary>
    /// Check if the player owns a module with the given ID.
    /// </summary>
    public bool HasModule(string moduleID)
    {
        foreach (ModuleData m in modules)
        {
            if (m.moduleID == moduleID)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Remove a module by ID (used when installing into a socket).
    /// </summary>
    public void RemoveModule(string moduleID)
    {
        for (int i = 0; i < modules.Count; i++)
        {
            if (modules[i].moduleID == moduleID)
            {
                Debug.Log($"[ModuleInventory] Removed: {modules[i].displayName}");
                modules.RemoveAt(i);
                return;
            }
        }
    }
}
