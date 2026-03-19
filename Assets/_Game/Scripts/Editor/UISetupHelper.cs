using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public static class UISetupHelper
{
    [MenuItem("Profile7/Setup Cabinet Scene")]
    static void SetupCabinetScene()
    {
        // Create PanelSettings if missing
        const string panelSettingsPath = "Assets/_Game/UI/GamePanelSettings.asset";
        var ps = AssetDatabase.LoadAssetAtPath<PanelSettings>(panelSettingsPath);
        if (ps == null)
        {
            ps = ScriptableObject.CreateInstance<PanelSettings>();
            ps.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            ps.referenceResolution = new Vector2Int(1920, 1080);
            ps.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            ps.match = 0.5f;
            AssetDatabase.CreateAsset(ps, panelSettingsPath);
            Debug.Log($"Created PanelSettings at {panelSettingsPath}");
        }

        // Find or create SceneSetup object
        var setup = Object.FindFirstObjectByType<SceneSetup>();
        if (setup == null)
        {
            var go = new GameObject("[SceneSetup]");
            setup = go.AddComponent<SceneSetup>();
            Debug.Log("Created [SceneSetup] GameObject");
        }

        // Assign references
        var so = new SerializedObject(setup);
        var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/_Game/UI/GameUI.uxml");
        var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/_Game/UI/GameUI.uss");

        so.FindProperty("gameUIAsset").objectReferenceValue = uxml;
        so.FindProperty("panelSettings").objectReferenceValue = ps;
        so.FindProperty("gameUIStyles").objectReferenceValue = uss;
        so.ApplyModifiedProperties();

        Debug.Log("Cabinet scene setup complete! Assigned UXML, USS, and PanelSettings to SceneSetup.");
        EditorUtility.SetDirty(setup);
    }
}
