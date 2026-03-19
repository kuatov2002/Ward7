using UnityEngine;
using UnityEngine.UIElements;

public class SceneSetup : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] Material baseMaterial;

    [Header("UI")]
    [SerializeField] VisualTreeAsset gameUIAsset;
    [SerializeField] PanelSettings panelSettings;
    [SerializeField] StyleSheet gameUIStyles;

    // PSX color palette
    static readonly Color FloorColor = new(0.08f, 0.07f, 0.06f);
    static readonly Color WallColor = new(0.14f, 0.13f, 0.12f);
    static readonly Color CeilingColor = new(0.1f, 0.1f, 0.1f);
    static readonly Color DeskColor = new(0.22f, 0.14f, 0.07f);
    static readonly Color DeskLegColor = new(0.18f, 0.1f, 0.05f);
    static readonly Color WoodDark = new(0.15f, 0.08f, 0.04f);
    static readonly Color WoodMed = new(0.2f, 0.12f, 0.06f);
    static readonly Color Metal = new(0.12f, 0.12f, 0.14f);
    static readonly Color MetalLight = new(0.2f, 0.2f, 0.22f);
    static readonly Color Paper = new(0.7f, 0.68f, 0.6f);
    static readonly Color PaperDark = new(0.5f, 0.45f, 0.35f);
    static readonly Color Leather = new(0.12f, 0.08f, 0.05f);
    static readonly Color DoorColor = new(0.18f, 0.12f, 0.08f);
    static readonly Color FrameColor = new(0.25f, 0.2f, 0.1f);
    static readonly Color GreenTint = new(0.05f, 0.12f, 0.05f);
    static readonly Color RedDark = new(0.35f, 0.08f, 0.05f);
    static readonly Color PhoneBlack = new(0.05f, 0.05f, 0.05f);
    static readonly Color MonitorGray = new(0.15f, 0.15f, 0.18f);
    static readonly Color ScreenGlow = new(0.06f, 0.12f, 0.08f);
    static readonly Color CalendarWhite = new(0.75f, 0.72f, 0.65f);

    void Awake()
    {
        BuildRoom();
        BuildFurniture();
        BuildCamera();
        BuildDeskObjects();
        BuildUI();
        BuildLighting();
        BuildAudio();
        BuildEvidenceBoard();
    }

    // ─── ROOM ───
    void BuildRoom()
    {
        // Floor
        var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.localScale = new Vector3(1.2f, 1f, 1f);
        SetColor(floor, FloorColor);

        // Ceiling
        var ceiling = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ceiling.name = "Ceiling";
        ceiling.transform.position = new Vector3(0f, 3f, 2.5f);
        ceiling.transform.localScale = new Vector3(1.2f, 1f, 0.6f);
        ceiling.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
        SetColor(ceiling, CeilingColor);

        // Back wall
        Box("BackWall", new Vector3(0f, 1.5f, 2.5f), new Vector3(6f, 3f, 0.1f), WallColor);

        // Left wall
        Box("LeftWall", new Vector3(-3f, 1.5f, 1.25f), new Vector3(0.1f, 3f, 2.6f), WallColor);

        // Right wall
        Box("RightWall", new Vector3(3f, 1.5f, 1.25f), new Vector3(0.1f, 3f, 2.6f), WallColor);

        // Back wall behind player
        Box("BackWallBehind", new Vector3(0f, 1.5f, -0.8f), new Vector3(6f, 3f, 0.1f), WallColor);

        // Baseboard (плинтус) — back wall
        Box("Baseboard_Back", new Vector3(0f, 0.04f, 2.44f), new Vector3(5.9f, 0.08f, 0.04f), WoodDark);
        // Left wall baseboard
        Box("Baseboard_Left", new Vector3(-2.94f, 0.04f, 1.25f), new Vector3(0.04f, 0.08f, 2.5f), WoodDark);
        // Right wall baseboard
        Box("Baseboard_Right", new Vector3(2.94f, 0.04f, 1.25f), new Vector3(0.04f, 0.08f, 2.5f), WoodDark);

        // Desk surface
        Box("Desk", new Vector3(0f, 0.72f, 0.6f), new Vector3(2.2f, 0.06f, 1f), DeskColor);

        // Desk front panel
        Box("DeskFront", new Vector3(0f, 0.36f, 0.11f), new Vector3(2.2f, 0.72f, 0.03f), DeskLegColor);

        // Desk side panels
        Box("DeskSideL", new Vector3(-1.09f, 0.36f, 0.6f), new Vector3(0.03f, 0.72f, 0.98f), DeskLegColor);
        Box("DeskSideR", new Vector3(1.09f, 0.36f, 0.6f), new Vector3(0.03f, 0.72f, 0.98f), DeskLegColor);

        // Desk drawer handles
        Box("DrawerHandle1", new Vector3(-0.4f, 0.5f, 0.09f), new Vector3(0.1f, 0.02f, 0.02f), MetalLight);
        Box("DrawerHandle2", new Vector3(-0.4f, 0.3f, 0.09f), new Vector3(0.1f, 0.02f, 0.02f), MetalLight);
        Box("DrawerHandle3", new Vector3(0.4f, 0.5f, 0.09f), new Vector3(0.1f, 0.02f, 0.02f), MetalLight);
    }

    // ─── FURNITURE ───
    void BuildFurniture()
    {
        // ─── CHAIR (behind desk, player's implied seat) ───
        // Seat
        Box("ChairSeat", new Vector3(0f, 0.48f, -0.15f), new Vector3(0.45f, 0.04f, 0.4f), Leather);
        // Backrest
        Box("ChairBack", new Vector3(0f, 0.78f, -0.34f), new Vector3(0.45f, 0.56f, 0.03f), Leather);
        // Chair legs
        Cyl("ChairLeg1", new Vector3(-0.18f, 0.24f, -0.32f), new Vector3(0.03f, 0.24f, 0.03f), Metal);
        Cyl("ChairLeg2", new Vector3(0.18f, 0.24f, -0.32f), new Vector3(0.03f, 0.24f, 0.03f), Metal);
        Cyl("ChairLeg3", new Vector3(-0.18f, 0.24f, 0.02f), new Vector3(0.03f, 0.24f, 0.03f), Metal);
        Cyl("ChairLeg4", new Vector3(0.18f, 0.24f, 0.02f), new Vector3(0.03f, 0.24f, 0.03f), Metal);

        // ─── BOOKSHELF (left wall) ───
        var shelfParent = new GameObject("Bookshelf");
        shelfParent.transform.position = new Vector3(-2.5f, 0f, 1.8f);

        // Shelf frame
        Box("ShelfSideL", new Vector3(-2.8f, 0.9f, 1.8f), new Vector3(0.04f, 1.8f, 0.35f), WoodDark);
        Box("ShelfSideR", new Vector3(-2.2f, 0.9f, 1.8f), new Vector3(0.04f, 1.8f, 0.35f), WoodDark);
        Box("ShelfBack", new Vector3(-2.5f, 0.9f, 1.97f), new Vector3(0.64f, 1.8f, 0.02f), WoodDark);

        // Shelves
        for (int i = 0; i < 5; i++)
        {
            float y = 0.02f + i * 0.42f;
            Box($"Shelf_{i}", new Vector3(-2.5f, y, 1.8f), new Vector3(0.6f, 0.03f, 0.35f), WoodMed);
        }

        // Books on shelves (random colored blocks)
        Color[] bookColors = { RedDark, GreenTint, new Color(0.15f, 0.1f, 0.25f), DeskColor, new Color(0.1f, 0.15f, 0.2f) };
        for (int shelf = 0; shelf < 4; shelf++)
        {
            float shelfY = 0.04f + shelf * 0.42f;
            float x = -2.75f;
            for (int b = 0; b < 4; b++)
            {
                float w = Random.Range(0.06f, 0.1f);
                float h = Random.Range(0.28f, 0.38f);
                Box($"Book_{shelf}_{b}", new Vector3(x + w / 2f, shelfY + h / 2f + 0.02f, 1.8f),
                    new Vector3(w, h, 0.2f), bookColors[Random.Range(0, bookColors.Length)]);
                x += w + 0.02f;
            }
        }

        // ─── DESK LAMP (physical object, not the light) ───
        Cyl("LampBase", new Vector3(-0.7f, 0.76f, 0.85f), new Vector3(0.1f, 0.02f, 0.1f), Metal);
        Cyl("LampArm", new Vector3(-0.7f, 0.93f, 0.85f), new Vector3(0.015f, 0.15f, 0.015f), Metal);
        // Lamp shade (cone-like: wide cube on top)
        Box("LampShade", new Vector3(-0.7f, 1.1f, 0.85f), new Vector3(0.16f, 0.08f, 0.12f), new Color(0.25f, 0.2f, 0.1f));

        // ─── TRASH CAN under desk ───
        Cyl("TrashCan", new Vector3(0.8f, 0.15f, 0.3f), new Vector3(0.12f, 0.15f, 0.12f), Metal);

        // ─── PAPER STACKS on desk ───
        for (int i = 0; i < 4; i++)
        {
            float ox = Random.Range(-0.02f, 0.02f);
            float oz = Random.Range(-0.01f, 0.01f);
            Box($"PaperStack_{i}", new Vector3(0.15f + ox, 0.765f + i * 0.008f, 0.3f + oz),
                new Vector3(0.15f, 0.005f, 0.2f), Color.Lerp(Paper, PaperDark, Random.Range(0f, 0.3f)));
        }

        // ─── PEN HOLDER ───
        Cyl("PenHolder", new Vector3(-0.55f, 0.79f, 0.65f), new Vector3(0.04f, 0.05f, 0.04f), Metal);
        // Pens sticking out
        Cyl("Pen1", new Vector3(-0.55f, 0.87f, 0.64f), new Vector3(0.006f, 0.06f, 0.006f), new Color(0.1f, 0.1f, 0.4f));
        Cyl("Pen2", new Vector3(-0.54f, 0.86f, 0.66f), new Vector3(0.006f, 0.055f, 0.006f), RedDark);

        // ─── COFFEE MUG ───
        Cyl("Mug", new Vector3(0.65f, 0.78f, 0.35f), new Vector3(0.04f, 0.04f, 0.04f), new Color(0.3f, 0.15f, 0.08f));

        // ─── DOOR on right wall ───
        // Door frame
        Box("DoorFrameL", new Vector3(2.93f, 1.1f, 0.5f), new Vector3(0.06f, 2.2f, 0.1f), WoodDark);
        Box("DoorFrameR", new Vector3(2.93f, 1.1f, 1.3f), new Vector3(0.06f, 2.2f, 0.1f), WoodDark);
        Box("DoorFrameTop", new Vector3(2.93f, 2.2f, 0.9f), new Vector3(0.06f, 0.08f, 0.9f), WoodDark);
        // Door panel
        Box("DoorPanel", new Vector3(2.91f, 1.08f, 0.9f), new Vector3(0.04f, 2.1f, 0.76f), DoorColor);
        // Door handle
        Box("DoorHandle", new Vector3(2.88f, 1.0f, 0.6f), new Vector3(0.04f, 0.02f, 0.08f), MetalLight);

        // ─── WALL FRAMES / PICTURES ───
        Box("WallFrame1", new Vector3(-0.5f, 1.6f, 2.44f), new Vector3(0.4f, 0.3f, 0.02f), FrameColor);
        Box("WallPic1", new Vector3(-0.5f, 1.6f, 2.43f), new Vector3(0.34f, 0.24f, 0.01f), new Color(0.08f, 0.1f, 0.08f));

        Box("WallFrame2", new Vector3(0.5f, 1.8f, 2.44f), new Vector3(0.3f, 0.4f, 0.02f), FrameColor);
        Box("WallPic2", new Vector3(0.5f, 1.8f, 2.43f), new Vector3(0.24f, 0.34f, 0.01f), new Color(0.1f, 0.08f, 0.06f));

        // ─── FILING CABINET (right of desk) ───
        Box("FileCabinet", new Vector3(1.5f, 0.45f, 1.8f), new Vector3(0.4f, 0.9f, 0.35f), MetalLight);
        Box("FileCabDraw1", new Vector3(1.5f, 0.7f, 1.62f), new Vector3(0.35f, 0.25f, 0.01f), Metal);
        Box("FileCabDraw2", new Vector3(1.5f, 0.35f, 1.62f), new Vector3(0.35f, 0.25f, 0.01f), Metal);
        Box("FileCabHandle1", new Vector3(1.5f, 0.7f, 1.61f), new Vector3(0.08f, 0.02f, 0.02f), MetalLight);
        Box("FileCabHandle2", new Vector3(1.5f, 0.35f, 1.61f), new Vector3(0.08f, 0.02f, 0.02f), MetalLight);

        // ─── COAT RACK near door ───
        Cyl("CoatRackPole", new Vector3(2.4f, 0.85f, 0.3f), new Vector3(0.03f, 0.85f, 0.03f), WoodDark);
        Cyl("CoatRackBase", new Vector3(2.4f, 0.02f, 0.3f), new Vector3(0.15f, 0.02f, 0.15f), WoodDark);
        // Coat hook
        Box("CoatHook", new Vector3(2.4f, 1.6f, 0.28f), new Vector3(0.08f, 0.02f, 0.02f), Metal);
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
        cam.backgroundColor = new Color(0.02f, 0.02f, 0.03f);
        cam.nearClipPlane = 0.1f;
        cam.fieldOfView = 60f;
        camGo.transform.position = new Vector3(0f, 1.2f, -0.3f);
        camGo.transform.rotation = Quaternion.Euler(10f, 0f, 0f);

        camGo.AddComponent<AudioListener>();
        camGo.AddComponent<FirstPersonLook>();
        camGo.AddComponent<RaycastInteraction>();
        camGo.AddComponent<PSXEffect>();
    }

    // ─── DESK OBJECTS ───
    void BuildDeskObjects()
    {
        var parent = new GameObject("DeskObjects");
        var officeCtrl = parent.AddComponent<OfficeController>();
        var objects = new System.Collections.Generic.List<DeskObject>();

        // 1. Dossier folder
        var dossier = Box("Dossier", new Vector3(-0.8f, 0.78f, 0.7f),
            new Vector3(0.25f, 0.03f, 0.35f), new Color(0.5f, 0.4f, 0.15f));
        dossier.transform.SetParent(parent.transform);
        var dossierObj = dossier.AddComponent<DeskObject>();
        dossierObj.panelName = "dossier-panel";
        dossierObj.displayName = "Досье";
        dossierObj.visibleOnDay = 1;
        dossierObj.visibleFromDayOnward = true;
        objects.Add(dossierObj);

        // 2. Phone
        var phoneBase = Cyl("Phone", new Vector3(-0.45f, 0.76f, 0.4f), new Vector3(0.15f, 0.02f, 0.15f), PhoneBlack);
        var phoneHandset = Box("PhoneHandset", new Vector3(-0.45f, 0.8f, 0.4f), new Vector3(0.04f, 0.02f, 0.18f), PhoneBlack);
        phoneHandset.transform.SetParent(phoneBase.transform);
        phoneBase.transform.SetParent(parent.transform);
        var phoneObj = phoneBase.AddComponent<DeskObject>();
        phoneObj.panelName = "contact-panel";
        phoneObj.displayName = "Телефон — Контакты";
        phoneObj.visibleOnDay = 1;
        var phoneAnim = phoneBase.AddComponent<DeskObjectAnimator>();
        phoneAnim.animType = DeskObjectAnimator.AnimationType.PhoneVibrate;
        objects.Add(phoneObj);

        // 3. Evidence folders
        var evidenceParent = new GameObject("EvidenceFolders");
        evidenceParent.transform.position = new Vector3(-0.15f, 0.78f, 0.8f);
        evidenceParent.transform.SetParent(parent.transform);
        Color[] evColors = { new(0.4f, 0.3f, 0.15f), new(0.45f, 0.32f, 0.18f), new(0.5f, 0.35f, 0.2f) };
        for (int i = 0; i < 3; i++)
        {
            var folder = Box($"EvidenceFolder_{i}", new Vector3(0f, i * 0.025f, 0f),
                new Vector3(0.2f, 0.02f, 0.28f), evColors[i]);
            folder.transform.SetParent(evidenceParent.transform, false);
        }
        var evBox = evidenceParent.AddComponent<BoxCollider>();
        evBox.size = new Vector3(0.2f, 0.08f, 0.28f);
        evBox.center = new Vector3(0f, 0.03f, 0f);
        var evObj = evidenceParent.AddComponent<DeskObject>();
        evObj.panelName = "evidence-panel";
        evObj.displayName = "Папки с уликами";
        evObj.visibleOnDay = 2;
        var evAnim = evidenceParent.AddComponent<DeskObjectAnimator>();
        evAnim.animType = DeskObjectAnimator.AnimationType.GlowPulse;
        objects.Add(evObj);

        // 4. Computer monitor
        Cyl("MonitorStand", new Vector3(0.35f, 0.77f, 1.0f), new Vector3(0.06f, 0.03f, 0.06f), Metal);
        Box("MonitorNeck", new Vector3(0.35f, 0.82f, 1.0f), new Vector3(0.03f, 0.06f, 0.03f), Metal);
        var monitorScreen = Box("Monitor", new Vector3(0.35f, 0.98f, 1.0f),
            new Vector3(0.5f, 0.32f, 0.03f), MonitorGray);
        // Screen glow area
        Box("ScreenGlow", new Vector3(0.35f, 0.98f, 0.98f), new Vector3(0.44f, 0.26f, 0.005f), ScreenGlow);
        monitorScreen.transform.SetParent(parent.transform);
        var monObj = monitorScreen.AddComponent<DeskObject>();
        monObj.panelName = "testimony-panel";
        monObj.displayName = "Компьютер — Показания";
        monObj.visibleOnDay = 3;
        objects.Add(monObj);

        // 5. Interrogation notepad
        var notepad = Box("Notepad", new Vector3(0.5f, 0.77f, 0.5f),
            new Vector3(0.18f, 0.015f, 0.24f), Paper);
        notepad.transform.SetParent(parent.transform);
        var noteObj = notepad.AddComponent<DeskObject>();
        noteObj.panelName = "interrogation-panel";
        noteObj.displayName = "Протокол допроса";
        noteObj.visibleOnDay = 4;
        objects.Add(noteObj);

        // 6. Verdict stamp
        var stamp = Cyl("Stamp", new Vector3(0.75f, 0.78f, 0.6f),
            new Vector3(0.08f, 0.04f, 0.08f), RedDark);
        stamp.transform.SetParent(parent.transform);
        var stampObj = stamp.AddComponent<DeskObject>();
        stampObj.panelName = "briefing-panel";
        stampObj.displayName = "Печать — Вердикт";
        stampObj.visibleOnDay = 5;
        var stampAnim = stamp.AddComponent<DeskObjectAnimator>();
        stampAnim.animType = DeskObjectAnimator.AnimationType.StampReady;
        objects.Add(stampObj);

        // 7. Calendar
        var calendar = Box("Calendar", new Vector3(0.9f, 0.82f, 0.85f),
            new Vector3(0.12f, 0.16f, 0.02f), CalendarWhite);
        calendar.transform.rotation = Quaternion.Euler(-10f, 0f, 0f);
        calendar.transform.SetParent(parent.transform);
        var calObj = calendar.AddComponent<DeskObject>();
        calObj.isCalendar = true;
        calObj.displayName = "Календарь — Следующий день";
        calObj.visibleOnDay = -1;
        objects.Add(calObj);

        // 8. Connection board (the evidence board on wall — always available)
        // We make the existing board clickable
        var boardTrigger = Box("BoardTrigger", new Vector3(-1.8f, 1.5f, 2.43f),
            new Vector3(1.2f, 0.8f, 0.04f), new Color(0.55f, 0.4f, 0.2f));
        boardTrigger.transform.SetParent(parent.transform);
        var boardObj = boardTrigger.AddComponent<DeskObject>();
        boardObj.panelName = "connection-panel";
        boardObj.displayName = "Доска связей";
        boardObj.visibleOnDay = 1;
        boardObj.visibleFromDayOnward = true;
        objects.Add(boardObj);

        // 9. Timeline clipboard (on desk, available from Thursday onward)
        var timeline = Box("Timeline", new Vector3(0.3f, 0.775f, 0.35f),
            new Vector3(0.22f, 0.01f, 0.16f), new Color(0.6f, 0.55f, 0.4f));
        timeline.transform.SetParent(parent.transform);
        var tlObj = timeline.AddComponent<DeskObject>();
        tlObj.panelName = "timeline-panel";
        tlObj.displayName = "Хронология событий";
        tlObj.visibleOnDay = 4;
        tlObj.visibleFromDayOnward = true;
        objects.Add(tlObj);

        officeCtrl.deskObjects = objects.ToArray();
    }

    // ─── UI ───
    void BuildUI()
    {
        var uiGo = new GameObject("UIDocument");
        var uiDoc = uiGo.AddComponent<UIDocument>();
        uiDoc.panelSettings = panelSettings;
        uiDoc.visualTreeAsset = gameUIAsset;
        if (gameUIStyles != null)
            uiDoc.rootVisualElement.styleSheets.Add(gameUIStyles);

        uiGo.AddComponent<UIManager>();
        uiGo.AddComponent<MainMenuUI>();
        uiGo.AddComponent<OutcomeUI>();
        uiGo.AddComponent<DossierUI>();
        uiGo.AddComponent<ContactUI>();
        uiGo.AddComponent<EvidenceUI>();
        uiGo.AddComponent<TestimonyUI>();
        uiGo.AddComponent<InterrogationUI>();
        uiGo.AddComponent<ConnectionBoardUI>();
        uiGo.AddComponent<TimelineUI>();
        uiGo.AddComponent<BriefingUI>();
        uiGo.AddComponent<EndingUI>();
    }

    // ─── LIGHTING ───
    void BuildLighting()
    {
        var dirLight = new GameObject("DirectionalLight");
        var dl = dirLight.AddComponent<Light>();
        dl.type = LightType.Directional;
        dl.color = new Color(0.7f, 0.65f, 0.55f);
        dl.intensity = 0.35f;
        dl.shadows = LightShadows.Hard; // PSX = hard shadows
        dirLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        var deskLamp = new GameObject("DeskLamp");
        var pl = deskLamp.AddComponent<Light>();
        pl.type = LightType.Point;
        pl.color = new Color(1f, 0.85f, 0.5f);
        pl.intensity = 1.5f;
        pl.range = 3.5f;
        deskLamp.transform.position = new Vector3(-0.7f, 1.15f, 0.85f);

        var atmosGo = new GameObject("[Atmosphere]");
        var atmos = atmosGo.AddComponent<AtmosphereController>();
        var cam = Camera.main != null ? Camera.main.transform : null;
        atmos.Init(dl, pl, cam);
    }

    void BuildAudio()
    {
        var audioGo = new GameObject("[ProceduralAudio]");
        audioGo.AddComponent<ProceduralAudio>();
        audioGo.AddComponent<ProceduralMusic>();
    }

    void BuildEvidenceBoard()
    {
        var boardGo = new GameObject("[EvidenceBoard]");
        var board = boardGo.AddComponent<EvidenceBoard>();
        board.Init(baseMaterial);
    }

    // ─── HELPERS ───
    GameObject Box(string name, Vector3 pos, Vector3 scale, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.position = pos;
        go.transform.localScale = scale;
        SetColor(go, color);
        return go;
    }

    GameObject Cyl(string name, Vector3 pos, Vector3 scale, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.position = pos;
        go.transform.localScale = scale;
        SetColor(go, color);
        return go;
    }

    void SetColor(GameObject go, Color color)
    {
        var renderer = go.GetComponent<Renderer>();
        if (renderer == null) return;
        if (baseMaterial != null)
        {
            var mat = new Material(baseMaterial);
            mat.color = color;
            mat.SetFloat("_Metallic", 0f);
            mat.SetFloat("_Smoothness", 0f);
            renderer.material = mat;
        }
        else
        {
            renderer.material.color = color;
        }
    }
}
