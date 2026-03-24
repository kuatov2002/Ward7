using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Free-form deduction board.
/// Player selects a suspect, then picks 3 supporting fragments.
/// No type labels visible — player must reason what is motive/opportunity/evidence.
/// Only fragments from physical evidence + confirmed confrontations are available.
/// </summary>
public class DeductionBoardUI : MonoBehaviour, IPanelController
{
    const string PanelName = "deduction-panel";

    const int RequiredFragments = 3;

    void Start() => UIManager.Instance.RegisterController(PanelName, this);

    public void OnShow() => BuildPanel();

    void BuildPanel()
    {
        var root  = UIManager.Instance.GetRoot();
        var panel = root.Q<VisualElement>(PanelName);
        panel.Clear();

        var cases         = ServiceLocator.Get<CaseService>();
        var deduction     = ServiceLocator.Get<DeductionService>();
        var contradictions = ServiceLocator.Get<ContradictionService>();
        var c = cases.ActiveCase;
        if (c == null) return;

        // ── Close ──
        var closeRow = new VisualElement(); closeRow.AddToClassList("close-row");
        var closeBtn = new Button(() => UIManager.Instance.ShowPanel("command-center-panel"));
        closeBtn.text = "✕"; closeBtn.AddToClassList("btn-close");
        closeRow.Add(closeBtn); panel.Add(closeRow);

        var title = new Label("ДОСКА ДЕДУКЦИИ");
        title.AddToClassList("title"); panel.Add(title);

        var instr = new Label(
            "Выберите подозреваемого и три факта, которые доказывают его вину. " +
            "Какой факт является мотивом, какой — возможностью, а какой — уликой — решаете вы.");
        instr.AddToClassList("text-small"); instr.AddToClassList("text-dim");
        instr.style.whiteSpace = WhiteSpace.Normal; instr.style.marginBottom = 10;
        panel.Add(instr);

        // ── Separate available fragments: physical vs testimony ──
        var revealed    = deduction.GetRevealedFragments();
        var physicalIds = ServiceLocator.Get<SaveService>().Data.physicalFragments;

        var availableFragments = c.fragments?
            .Where(f => revealed.Contains(f.fragmentId) && f.fragmentType != FragmentType.Suspect)
            .ToList() ?? new List<DeductionFragmentData>();

        var physicalFragments  = availableFragments.Where(f => physicalIds.Contains(f.fragmentId)).ToList();
        var testimonyFragments = availableFragments.Where(f => !physicalIds.Contains(f.fragmentId)).ToList();

        // ── Current selection ──
        var currentFrags    = deduction.GetAccusationFragments();
        var currentAccused  = deduction.GetAccused();

        // ── Suspect selection ──
        panel.Add(SectionHeader("ОБВИНЯЕМЫЙ"));

        var suspGrid = new VisualElement();
        suspGrid.style.flexDirection = FlexDirection.Row;
        suspGrid.style.flexWrap      = Wrap.Wrap;
        suspGrid.style.marginBottom  = 12;

        var suspectFragments = c.fragments?
            .Where(f => revealed.Contains(f.fragmentId) && f.fragmentType == FragmentType.Suspect)
            .ToList() ?? new List<DeductionFragmentData>();

        if (suspectFragments.Count == 0)
        {
            var noSusp = new Label("Подозреваемые не идентифицированы. Продолжайте расследование.");
            noSusp.AddToClassList("text-dim"); suspGrid.Add(noSusp);
        }
        else
        {
            foreach (var sf in suspectFragments)
            {
                bool selected = currentAccused == sf.relatedPersonId;
                var btn = new Button(() => {
                    deduction.SetAccused(sf.relatedPersonId);
                    BuildPanel();
                });
                btn.text = GetPersonName(c, sf.relatedPersonId);
                btn.style.height    = 44;
                btn.style.minWidth  = 120;
                btn.style.fontSize  = 13;
                btn.style.unityFontStyleAndWeight = FontStyle.Bold;
                btn.style.borderTopLeftRadius     = 0;
                btn.style.borderTopRightRadius    = 0;
                btn.style.borderBottomLeftRadius  = 0;
                btn.style.borderBottomRightRadius = 0;
                btn.style.borderTopWidth    = 2;
                btn.style.borderBottomWidth = 2;
                btn.style.borderLeftWidth   = 2;
                btn.style.borderRightWidth  = 2;

                if (selected)
                {
                    btn.style.backgroundColor = new Color(0.4f, 0.05f, 0.05f);
                    var rc = new Color(0.9f, 0.3f, 0.3f);
                    btn.style.borderTopColor    = rc;
                    btn.style.borderBottomColor = rc;
                    btn.style.borderLeftColor   = rc;
                    btn.style.borderRightColor  = rc;
                    btn.style.color = new Color(1f, 0.6f, 0.6f);
                }
                else
                {
                    btn.style.backgroundColor = new Color(0.05f, 0.1f, 0.05f);
                    var bc = new Color(0.2f, 0.4f, 0.2f);
                    btn.style.borderTopColor    = bc;
                    btn.style.borderBottomColor = bc;
                    btn.style.borderLeftColor   = bc;
                    btn.style.borderRightColor  = bc;
                    btn.style.color = new Color(0.5f, 0.8f, 0.5f);
                }
                suspGrid.Add(btn);
            }
        }
        panel.Add(suspGrid);

        // ── Selected fragments display ──
        panel.Add(SectionHeader($"ДОКАЗАТЕЛЬСТВА ({currentFrags.Count}/{RequiredFragments})"));

        var selectedBox = new VisualElement();
        selectedBox.AddToClassList("box");
        selectedBox.style.minHeight    = 60;
        selectedBox.style.marginBottom = 10;
        selectedBox.style.flexDirection = FlexDirection.Row;
        selectedBox.style.flexWrap     = Wrap.Wrap;
        selectedBox.style.alignItems   = Align.FlexStart;

        if (currentFrags.Count == 0)
        {
            var empty = new Label("Выберите три факта из списка ниже.");
            empty.AddToClassList("text-dim");
            empty.style.unityTextAlign = TextAnchor.MiddleCenter;
            empty.style.flexGrow = 1;
            selectedBox.Add(empty);
        }
        else
        {
            foreach (var fid in currentFrags)
            {
                var frag = c.fragments?.FirstOrDefault(f => f.fragmentId == fid);
                if (frag == null) continue;

                bool isPhys = physicalIds.Contains(fid);
                var card    = new VisualElement();
                card.style.maxWidth        = 200;
                card.style.borderTopWidth  = 1; card.style.borderBottomWidth = 1;
                card.style.borderLeftWidth = 2; card.style.borderRightWidth  = 1;
                card.style.backgroundColor = new Color(0.08f, 0.15f, 0.08f);

                var cardColor = isPhys ? new Color(0.4f, 0.8f, 0.4f) : new Color(0.8f, 0.6f, 0.2f);
                card.style.borderLeftColor   = cardColor;
                card.style.borderTopColor    = new Color(0.15f, 0.3f, 0.15f);
                card.style.borderBottomColor = new Color(0.15f, 0.3f, 0.15f);
                card.style.borderRightColor  = new Color(0.15f, 0.3f, 0.15f);

                var typeTag = new Label(isPhys ? "УЛИКА" : "ПОКАЗАНИЕ");
                typeTag.style.fontSize  = 9;
                typeTag.style.color     = cardColor;
                typeTag.style.letterSpacing = 2;
                card.Add(typeTag);

                var cardText = new Label(frag.displayText);
                cardText.AddToClassList("text-small");
                cardText.style.whiteSpace = WhiteSpace.Normal;
                cardText.style.marginTop  = 2;
                card.Add(cardText);

                // Remove button
                var removeBtn = new Button(() => {
                    var list = new List<string>(currentFrags);
                    list.Remove(fid);
                    deduction.SetAccusationFragments(list);
                    BuildPanel();
                });
                removeBtn.text = "✕ убрать";
                removeBtn.style.fontSize        = 10;
                removeBtn.style.marginTop       = 4;
                removeBtn.style.height          = 20;
                removeBtn.style.backgroundColor = new Color(0.2f, 0.05f, 0.05f);
                removeBtn.style.color           = new Color(0.6f, 0.3f, 0.3f);
                removeBtn.style.borderTopWidth  = 0; removeBtn.style.borderBottomWidth = 0;
                removeBtn.style.borderLeftWidth = 0; removeBtn.style.borderRightWidth  = 0;
                card.Add(removeBtn);

                selectedBox.Add(card);
            }
        }
        panel.Add(selectedBox);

        // ── Accusation readiness ──
        bool ready = deduction.IsAccusationReady();

        if (ready)
        {
            var readyLabel = new Label("★ Обвинение сформулировано. Предъявите его через Печать.");
            readyLabel.AddToClassList("text-bold");
            readyLabel.style.color = new Color(0.8f, 0.7f, 0.1f);
            readyLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            readyLabel.style.marginBottom   = 10;
            panel.Add(readyLabel);
        }

        // ── Available evidence (scrollable) ──
        var scroll = new ScrollView(ScrollViewMode.Vertical);
        scroll.style.flexGrow  = 1;
        scroll.style.flexShrink = 1;

        // Physical evidence
        if (physicalFragments.Count > 0)
        {
            scroll.Add(SectionHeader("ФИЗИЧЕСКИЕ УЛИКИ"));
            scroll.Add(BuildFragmentGrid(physicalFragments, currentFrags, deduction, new Color(0.3f, 0.8f, 0.3f)));
        }

        // Testimony fragments
        if (testimonyFragments.Count > 0)
        {
            scroll.Add(SectionHeader("ПОДТВЕРЖДЁННЫЕ ПОКАЗАНИЯ"));
            scroll.Add(BuildFragmentGrid(testimonyFragments, currentFrags, deduction, new Color(0.8f, 0.6f, 0.2f)));
        }

        if (physicalFragments.Count == 0 && testimonyFragments.Count == 0)
        {
            var noFrag = new Label("Пока нет собранных улик. Осмотрите места и сделайте запросы.");
            noFrag.AddToClassList("text-dim"); noFrag.style.unityTextAlign = TextAnchor.MiddleCenter;
            scroll.Add(noFrag);
        }

        panel.Add(scroll);

        panel.Add(Spacer(8));
        var backBtn = new Button(() => UIManager.Instance.ShowPanel("command-center-panel"));
        backBtn.text = "ВЕРНУТЬСЯ"; backBtn.AddToClassList("btn-wide");
        panel.Add(backBtn);
    }

    VisualElement BuildFragmentGrid(
        List<DeductionFragmentData> frags,
        List<string> currentSelected,
        DeductionService deduction,
        Color borderColor)
    {
        var grid = new VisualElement();
        grid.style.flexDirection = FlexDirection.Row;
        grid.style.flexWrap      = Wrap.Wrap;
        grid.style.marginBottom  = 10;

        foreach (var frag in frags)
        {
            bool alreadySelected = currentSelected.Contains(frag.fragmentId);
            bool full            = currentSelected.Count >= RequiredFragments;

            var btn = new Button(() => {
                if (alreadySelected) return;
                if (full) return;
                var list = new List<string>(currentSelected) { frag.fragmentId };
                deduction.SetAccusationFragments(list);
                BuildPanel();
            });

            btn.text = frag.displayText;
            btn.style.width     = 160;
            btn.style.minHeight = 50;
            btn.style.fontSize  = 11;
            btn.style.unityFontStyleAndWeight = FontStyle.Normal;
            btn.style.whiteSpace = WhiteSpace.Normal;
            btn.style.borderTopLeftRadius     = 0;
            btn.style.borderTopRightRadius    = 0;
            btn.style.borderBottomLeftRadius  = 0;
            btn.style.borderBottomRightRadius = 0;
            btn.style.borderTopWidth    = 1;
            btn.style.borderBottomWidth = 1;
            btn.style.borderLeftWidth   = 2;
            btn.style.borderRightWidth  = 1;

            if (alreadySelected)
            {
                btn.style.backgroundColor = new Color(0.1f, 0.2f, 0.1f);
                btn.style.opacity = 0.4f;
                btn.style.borderLeftColor   = borderColor;
                btn.style.borderTopColor    = borderColor;
                btn.style.borderBottomColor = borderColor;
                btn.style.borderRightColor  = borderColor;
                btn.style.color = new Color(0.3f, 0.6f, 0.3f);
            }
            else if (full)
            {
                btn.style.backgroundColor = new Color(0.04f, 0.08f, 0.04f);
                btn.style.opacity = 0.5f;
                btn.style.borderLeftColor   = new Color(0.15f, 0.3f, 0.15f);
                btn.style.borderTopColor    = new Color(0.1f, 0.2f, 0.1f);
                btn.style.borderBottomColor = new Color(0.1f, 0.2f, 0.1f);
                btn.style.borderRightColor  = new Color(0.1f, 0.2f, 0.1f);
                btn.style.color = new Color(0.25f, 0.5f, 0.25f);
            }
            else
            {
                btn.style.backgroundColor = new Color(0.06f, 0.12f, 0.06f);
                btn.style.borderLeftColor   = borderColor;
                btn.style.borderTopColor    = new Color(0.1f, 0.2f, 0.1f);
                btn.style.borderBottomColor = new Color(0.1f, 0.2f, 0.1f);
                btn.style.borderRightColor  = new Color(0.1f, 0.2f, 0.1f);
                btn.style.color = new Color(0.5f, 0.85f, 0.5f);
            }

            grid.Add(btn);
        }

        return grid;
    }

    static VisualElement SectionHeader(string text)
    {
        var l = new Label(text);
        l.AddToClassList("text-small"); l.AddToClassList("text-amber");
        l.style.letterSpacing = 2; l.style.marginTop = 6; l.style.marginBottom = 4;
        return l;
    }

    string GetPersonName(CaseSO c, string id)
    {
        var p = c.persons?.FirstOrDefault(x => x.personId == id);
        return p != null ? p.displayName : id;
    }

    public void OnHide() { }

    static VisualElement Spacer(int h = 10)
    { var s = new VisualElement(); s.style.height = h; return s; }
}