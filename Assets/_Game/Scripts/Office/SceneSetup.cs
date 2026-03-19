using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Builds the entire 3D detective office scene from primitives at runtime.
/// Attach to an empty GameObject in Cabinet scene. Creates camera, desk, room, desk objects, UI.
/// </summary>
public class SceneSetup : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] VisualTreeAsset gameUIAsset;
    [SerializeField] PanelSettings panelSettings;
    [SerializeField] StyleSheet gameUIStyles;

    void Awake()
    {
        BuildRoom();
        BuildCamera();
        BuildDeskObjects();
        BuildUI();
        BuildLighting();
    }

    // ─── ROOM ───
    void BuildRoom()
    {
        // Floor
        var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.position = Vector3.zero;
        floor.transform.localScale = new Vector3(1f, 1f, 1f);
        SetColor(floor, new Color(0.15f, 0.13f, 0.12f));

        // Back wall
        var backWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backWall.name = "BackWall";
        backWall.transform.position = new Vector3(0f, 1.5f, 2.5f);
        backWall.transform.localScale = new Vector3(6f, 3f, 0.1f);
        SetColor(backWall, new Color(0.25f, 0.23f, 0.22f));

        // Left wall
        var leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftWall.name = "LeftWall";
        leftWall.transform.position = new Vector3(-3f, 1.5f, 1.25f);
        leftWall.transform.localScale = new Vector3(0.1f, 3f, 2.6f);
        SetColor(leftWall, new Color(0.22f, 0.2f, 0.2f));

        // Right wall
        var rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightWall.name = "RightWall";
        rightWall.transform.position = new Vector3(3f, 1.5f, 1.25f);
        rightWall.transform.localScale = new Vector3(0.1f, 3f, 2.6f);
        SetColor(rightWall, new Color(0.22f, 0.2f, 0.2f));

        // Desk surface
        var desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
        desk.name = "Desk";
        desk.transform.position = new Vector3(0f, 0.72f, 0.6f);
        desk.transform.localScale = new Vector3(2.2f, 0.06f, 1f);
        SetColor(desk, new Color(0.3f, 0.2f, 0.12f));

        // Desk legs
        for (int i = 0; i < 4; i++)
        {
            var leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leg.name = $"DeskLeg_{i}";
            float x = (i % 2 == 0) ? -0.95f : 0.95f;
            float z = (i < 2) ? 0.2f : 1.0f;
            leg.transform.position = new Vector3(x, 0.36f, z);
            leg.transform.localScale = new Vector3(0.06f, 0.36f, 0.06f);
            SetColor(leg, new Color(0.25f, 0.17f, 0.1f));
        }
    }

    // ─── CAMERA ───
    void BuildCamera()
    {
        var existing = Camera.main;
        if (existing != null) Destroy(existing.gameObject);

        var camGo = new GameObject("MainCamera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.08f, 0.1f);
        cam.nearClipPlane = 0.1f;
        cam.fieldOfView = 60f;
        camGo.transform.position = new Vector3(0f, 1.2f, -0.3f);
        camGo.transform.rotation = Quaternion.Euler(10f, 0f, 0f);

        camGo.AddComponent<AudioListener>();
        camGo.AddComponent<FirstPersonLook>();
        camGo.AddComponent<RaycastInteraction>();
    }

    // ─── DESK OBJECTS ───
    void BuildDeskObjects()
    {
        var parent = new GameObject("DeskObjects");
        var officeCtrl = parent.AddComponent<OfficeController>();
        var objects = new System.Collections.Generic.List<DeskObject>();

        // 1. Dossier folder (always visible from day 1+)
        var dossier = CreateBox("Dossier", new Vector3(-0.8f, 0.78f, 0.7f),
            new Vector3(0.25f, 0.03f, 0.35f), new Color(0.6f, 0.5f, 0.2f));
        dossier.transform.SetParent(parent.transform);
        var dossierObj = dossier.AddComponent<DeskObject>();
        dossierObj.panelName = "dossier-panel";
        dossierObj.visibleOnDay = 1;
        dossierObj.visibleFromDayOnward = true;
        objects.Add(dossierObj);

        // 2. Phone (day 1 - contacts)
        var phoneBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        phoneBase.name = "Phone";
        phoneBase.transform.position = new Vector3(-0.45f, 0.76f, 0.4f);
        phoneBase.transform.localScale = new Vector3(0.15f, 0.02f, 0.15f);
        SetColor(phoneBase, new Color(0.15f, 0.15f, 0.15f));
        var phoneHandset = GameObject.CreatePrimitive(PrimitiveType.Cube);
        phoneHandset.name = "PhoneHandset";
        phoneHandset.transform.position = new Vector3(-0.45f, 0.8f, 0.4f);
        phoneHandset.transform.localScale = new Vector3(0.04f, 0.02f, 0.18f);
        SetColor(phoneHandset, new Color(0.1f, 0.1f, 0.1f));
        phoneHandset.transform.SetParent(phoneBase.transform);
        phoneBase.transform.SetParent(parent.transform);
        var phoneObj = phoneBase.AddComponent<DeskObject>();
        phoneObj.panelName = "contact-panel";
        phoneObj.visibleOnDay = 1;
        objects.Add(phoneObj);

        // 3. Evidence folders (day 2)
        var evidenceParent = new GameObject("EvidenceFolders");
        evidenceParent.transform.position = new Vector3(-0.15f, 0.78f, 0.8f);
        evidenceParent.transform.SetParent(parent.transform);
        for (int i = 0; i < 3; i++)
        {
            var folder = CreateBox($"EvidenceFolder_{i}",
                new Vector3(0f, i * 0.025f, 0f),
                new Vector3(0.2f, 0.02f, 0.28f),
                new Color(0.5f + i * 0.05f, 0.35f, 0.2f));
            folder.transform.SetParent(evidenceParent.transform, false);
        }
        var evBox = evidenceParent.AddComponent<BoxCollider>();
        evBox.size = new Vector3(0.2f, 0.08f, 0.28f);
        evBox.center = new Vector3(0f, 0.03f, 0f);
        var evObj = evidenceParent.AddComponent<DeskObject>();
        evObj.panelName = "evidence-panel";
        evObj.visibleOnDay = 2;
        objects.Add(evObj);

        // 4. Computer monitor (day 3 - testimony/messenger)
        var monitorStand = GameObject.CreatePrimitive(PrimitiveType.Cube);
        monitorStand.name = "MonitorStand";
        monitorStand.transform.position = new Vector3(0.35f, 0.79f, 1.0f);
        monitorStand.transform.localScale = new Vector3(0.06f, 0.08f, 0.06f);
        SetColor(monitorStand, new Color(0.2f, 0.2f, 0.2f));
        monitorStand.transform.SetParent(parent.transform);

        var monitorScreen = GameObject.CreatePrimitive(PrimitiveType.Cube);
        monitorScreen.name = "Monitor";
        monitorScreen.transform.position = new Vector3(0.35f, 0.98f, 1.0f);
        monitorScreen.transform.localScale = new Vector3(0.5f, 0.3f, 0.03f);
        SetColor(monitorScreen, new Color(0.1f, 0.15f, 0.2f));
        monitorScreen.transform.SetParent(parent.transform);
        var monObj = monitorScreen.AddComponent<DeskObject>();
        monObj.panelName = "testimony-panel";
        monObj.visibleOnDay = 3;
        objects.Add(monObj);

        // 5. Interrogation notepad (day 4)
        var notepad = CreateBox("Notepad", new Vector3(0.5f, 0.77f, 0.5f),
            new Vector3(0.18f, 0.015f, 0.24f), new Color(0.9f, 0.88f, 0.75f));
        notepad.transform.SetParent(parent.transform);
        var noteObj = notepad.AddComponent<DeskObject>();
        noteObj.panelName = "interrogation-panel";
        noteObj.visibleOnDay = 4;
        objects.Add(noteObj);

        // 6. Verdict stamp (day 5)
        var stamp = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stamp.name = "Stamp";
        stamp.transform.position = new Vector3(0.75f, 0.78f, 0.6f);
        stamp.transform.localScale = new Vector3(0.08f, 0.04f, 0.08f);
        SetColor(stamp, new Color(0.5f, 0.1f, 0.1f));
        stamp.transform.SetParent(parent.transform);
        var stampObj = stamp.AddComponent<DeskObject>();
        stampObj.panelName = "briefing-panel";
        stampObj.visibleOnDay = 5;
        objects.Add(stampObj);

        // 7. Calendar (always visible - advance day)
        var calendar = CreateBox("Calendar", new Vector3(0.9f, 0.82f, 0.85f),
            new Vector3(0.12f, 0.16f, 0.02f), new Color(0.9f, 0.9f, 0.95f));
        calendar.transform.rotation = Quaternion.Euler(-10f, 0f, 0f);
        calendar.transform.SetParent(parent.transform);
        var calObj = calendar.AddComponent<DeskObject>();
        calObj.isCalendar = true;
        calObj.visibleOnDay = -1; // always
        objects.Add(calObj);

        // Set desk objects array on controller
        officeCtrl.deskObjects = objects.ToArray();
    }

    // ─── UI ───
    void BuildUI()
    {
        var uiGo = new GameObject("UIDocument");
        var uiDoc = uiGo.AddComponent<UIDocument>();
        uiDoc.panelSettings = panelSettings;
        uiDoc.visualTreeAsset = gameUIAsset;

        // Apply stylesheet after the document is ready
        if (gameUIStyles != null)
        {
            uiDoc.rootVisualElement.styleSheets.Add(gameUIStyles);
        }

        var uiManager = uiGo.AddComponent<UIManager>();

        // Add all panel controllers
        uiGo.AddComponent<MainMenuUI>();
        uiGo.AddComponent<OutcomeUI>();
        uiGo.AddComponent<DossierUI>();
        uiGo.AddComponent<ContactUI>();
        uiGo.AddComponent<EvidenceUI>();
        uiGo.AddComponent<TestimonyUI>();
        uiGo.AddComponent<InterrogationUI>();
        uiGo.AddComponent<BriefingUI>();
        uiGo.AddComponent<EndingUI>();
    }

    // ─── LIGHTING ───
    void BuildLighting()
    {
        // Main directional light
        var dirLight = new GameObject("DirectionalLight");
        var dl = dirLight.AddComponent<Light>();
        dl.type = LightType.Directional;
        dl.color = new Color(0.9f, 0.85f, 0.8f);
        dl.intensity = 0.6f;
        dl.shadows = LightShadows.Soft;
        dirLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // Desk lamp (point light)
        var deskLamp = new GameObject("DeskLamp");
        var pl = deskLamp.AddComponent<Light>();
        pl.type = LightType.Point;
        pl.color = new Color(1f, 0.9f, 0.7f);
        pl.intensity = 1.2f;
        pl.range = 4f;
        deskLamp.transform.position = new Vector3(-0.3f, 1.3f, 0.6f);
    }

    // ─── HELPERS ───
    static GameObject CreateBox(string name, Vector3 pos, Vector3 scale, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.position = pos;
        go.transform.localScale = scale;
        SetColor(go, color);
        return go;
    }

    static void SetColor(GameObject go, Color color)
    {
        var renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            renderer.material.color = color;
        }
    }
}
