using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AssignMaterialToSpritesAndTilemapsEditor : EditorWindow
{
    public Material newMaterial;  // Material que quieres asignar

    [MenuItem("Tools/Assign Material to All Sprites and Tilemaps")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(AssignMaterialToSpritesAndTilemapsEditor), false, "Asignar Material a Sprites y Tilemaps");
    }

    private void OnGUI()
    {
        GUILayout.Label("Asignar Material a Todos los Sprites y Tilemaps", EditorStyles.boldLabel);

        newMaterial = (Material)EditorGUILayout.ObjectField("Nuevo Material", newMaterial, typeof(Material), false);

        if (GUILayout.Button("Aplicar a Todos los Sprites y Tilemaps"))
        {
            if (newMaterial != null)
            {
                AssignMaterialToAllSpritesAndTilemaps();
            }
            else
            {
                Debug.LogWarning("Por favor, asigna un material.");
            }
        }
    }

    private void AssignMaterialToAllSpritesAndTilemaps()
    {
        // Asigna material a todos los SpriteRenderers
        SpriteRenderer[] spriteRenderers = FindObjectsOfType<SpriteRenderer>();
        foreach (SpriteRenderer sr in spriteRenderers)
        {
            Undo.RecordObject(sr, "Cambio de material");
            sr.material = newMaterial;
        }

        // Asigna material a todos los Tilemap Renderers
        TilemapRenderer[] tilemapRenderers = FindObjectsOfType<TilemapRenderer>();
        foreach (TilemapRenderer tr in tilemapRenderers)
        {
            Undo.RecordObject(tr, "Cambio de material");
            tr.material = newMaterial;
        }

        Debug.Log("Material asignado a " + spriteRenderers.Length + " sprites y " + tilemapRenderers.Length + " tilemaps.");
    }
}
