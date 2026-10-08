using AlmaGame.Systems;
using UnityEditor;
using UnityEngine;

// Top menu shortcut for testing: wipe the save (eggs, abilities, checkpoints, completed levels).
public static class SaveMenu
{
    [MenuItem("AlmaDino/Borrar partida guardada")]
    private static void DeleteSave()
    {
        SaveSystem.Delete();
        Debug.Log($"Partida guardada borrada ({SaveSystem.FilePath}).");
        if (Application.isPlaying)
            Debug.Log("Estás en Play: sal y vuelve a darle a Play para empezar sin partida.");
    }
}
