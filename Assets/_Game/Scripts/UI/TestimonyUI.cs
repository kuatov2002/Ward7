using UnityEngine;
using UnityEngine.UIElements;

public class TestimonyUI : MonoBehaviour, IPanelController
{
    const string PanelName = "testimony-panel";
    float _savedScroll;

    // Distinct colors for each witness (up to 3)
    static readonly Color[] WitnessColors = {
        new Color(0.3f, 0.6f, 0.9f),   // Blue
        new Color(0.9f, 0.6f, 0.2f),   // Orange
        new Color(0.6f, 0.3f, 0.8f),   // Purple
    };

    static readonly string[] WitnessIcons = { "[A]", "[B]", "[C]" };

    void Start()
    {
        UIManager.Instance.RegisterController(PanelName, this);
    }

    public void OnShow()
    {
        var root = UIManager.Instance.GetRoot();
        var panel = root.Q<VisualElement>(PanelName);
        var oldScroll = panel.Q<ScrollView>();
        if (oldScroll != null) _savedScroll = oldScroll.scrollOffset.y;
        panel.Clear();

        var cases = ServiceLocator.Get<CaseService>();
        var choices = ServiceLocator.Get<DailyChoiceService>();
        var state = ServiceLocator.Get<GameStateService>();
        var notes = ServiceLocator.Get<NoteService>();
        var s = cases.ActiveCase;
        if (s == null) return;
        int w = state.CurrentWeek;

        var closeRow = new VisualElement();
        closeRow.AddToClassList("close-row");
        var closeBtn = new Button(() => UIManager.Instance.HideAllPanels());
        closeBtn.text = "\u2715";
        closeBtn.AddToClassList("btn-close");
        closeRow.Add(closeBtn);
        panel.Add(closeRow);

        var title = new Label("ПОКАЗАНИЯ");
        title.AddToClassList("header");
        panel.Add(title);

        var sub = new Label("Три источника дали показания. Вы можете запросить уточнение только у ОДНОГО. Базовые показания видны всегда — уточнение раскрывает скрытые детали.");
        sub.AddToClassList("text");
        panel.Add(sub);

        panel.Add(Spacer());

        var scroll = new ScrollView(ScrollViewMode.Vertical);
        scroll.style.maxHeight = 500;
        scroll.style.flexGrow = 1;

        bool done = choices.IsChosen(w, ChoiceType.Testimony);
        string sel = choices.GetSelected(w, ChoiceType.Testimony);

        for (int wi = 0; wi < s.testimonies.Length; wi++)
        {
            var t = s.testimonies[wi];
            bool mine = sel == t.witnessName;
            Color witnessColor = WitnessColors[wi % WitnessColors.Length];
            string witnessIcon = WitnessIcons[wi % WitnessIcons.Length];

            var box = new VisualElement();
            box.AddToClassList("box");

            // Color-coded left border per witness
            box.style.borderLeftWidth = 3;
            box.style.borderLeftColor = witnessColor;

            // Dim non-selected witnesses after choice is made
            if (done && !mine)
                box.style.opacity = 0.5f;

            // ─── Witness header with icon ───
            var headerRow = new VisualElement();
            headerRow.style.flexDirection = FlexDirection.Row;
            headerRow.style.alignItems = Align.Center;
            headerRow.style.marginBottom = 4;

            var iconLbl = new Label(witnessIcon);
            iconLbl.style.color = witnessColor;
            iconLbl.style.fontSize = 16;
            iconLbl.AddToClassList("text-bold");
            iconLbl.style.marginRight = 6;
            headerRow.Add(iconLbl);

            var name = new Label(t.witnessName);
            name.AddToClassList("text-bold");
            name.style.color = witnessColor;
            name.style.fontSize = 15;
            headerRow.Add(name);

            if (mine)
            {
                var selectedTag = new Label(" [ВЫБРАН]");
                selectedTag.AddToClassList("text-small");
                selectedTag.style.color = new Color(0.3f, 0.9f, 0.3f);
                headerRow.Add(selectedTag);
            }

            box.Add(headerRow);
            box.Add(Spacer(5));

            var baseText = new Label(t.baseTestimony);
            baseText.AddToClassList("text");
            MakeNoteable(baseText, t.baseTestimony, $"testimony_{t.witnessName}", w, notes);
            box.Add(baseText);

            if (mine)
            {
                box.Add(Spacer(8));
                var tag = new Label("[УТОЧНЕНИЕ ЗАПРОШЕНО]");
                tag.AddToClassList("text");
                tag.AddToClassList("text-yellow");
                box.Add(tag);
                box.Add(Spacer(5));
                var clar = new Label(t.clarification);
                clar.AddToClassList("text");
                MakeNoteable(clar, t.clarification, $"clarification_{t.witnessName}", w, notes);
                box.Add(clar);
                UIAnimations.FadeIn(clar, 300);
            }
            else if (!done)
            {
                box.Add(Spacer(5));
                string wName = t.witnessName;
                var capturedT = t;
                var btn = new Button(() => {
                    choices.Commit(w, ChoiceType.Testimony, wName);
                    // Launch lie detector if clarification lines exist
                    if (capturedT.clarificationLines != null && capturedT.clarificationLines.Length > 0)
                    {
                        var ld = FindFirstObjectByType<LieDetectorUI>();
                        if (ld != null)
                        {
                            ld.StartForWitness(wName, capturedT.clarificationLines, capturedT.startingTrust);
                            return;
                        }
                    }
                    OnShow();
                });
                btn.text = "Запросить уточнение";
                btn.AddToClassList("btn-small");
                btn.style.width = 220;
                btn.style.borderTopColor = witnessColor;
                btn.style.borderBottomColor = witnessColor;
                btn.style.borderLeftColor = witnessColor;
                btn.style.borderRightColor = witnessColor;
                box.Add(btn);
            }

            scroll.Add(box);

            // Staggered entrance animation
            UIAnimations.SlideInLeft(box, 150 + wi * 100);
        }

        // ─── CONTRADICTIONS ───
        if (s.contradictions != null && s.contradictions.Length > 0)
        {
            scroll.Add(Spacer(15));
            var contrExplain = new Label("Система автоматически сравнила показания и нашла расхождения:");
            contrExplain.AddToClassList("text-small");
            contrExplain.AddToClassList("text-dim");
            scroll.Add(contrExplain);

            var contrHeader = new Label("ОБНАРУЖЕННЫЕ ПРОТИВОРЕЧИЯ:");
            contrHeader.AddToClassList("text-bold");
            contrHeader.style.color = new Color(1f, 0.4f, 0.3f);
            scroll.Add(contrHeader);
            scroll.Add(Spacer(5));

            for (int i = 0; i < s.contradictions.Length; i++)
            {
                var c = s.contradictions[i];
                var cbox = new VisualElement();
                cbox.AddToClassList("box");
                cbox.style.borderLeftWidth = 3;
                cbox.style.borderLeftColor = new Color(1f, 0.3f, 0.3f);

                var header = new Label($"{c.witnessA} \u2260 {c.witnessB}");
                header.AddToClassList("text-bold");
                header.style.color = new Color(1f, 0.5f, 0.4f);
                cbox.Add(header);
                cbox.Add(Spacer(3));

                var desc = new Label(c.description);
                desc.AddToClassList("text");
                MakeNoteable(desc, c.description, "contradiction", w, notes);
                cbox.Add(desc);

                scroll.Add(cbox);
                UIAnimations.FadeIn(cbox, 300 + i * 100);
            }
        }

        panel.Add(scroll);
        scroll.schedule.Execute(() => scroll.scrollOffset = new Vector2(0, _savedScroll));
    }

    public void OnHide() { }

    static void MakeNoteable(Label label, string text, string source, int week, NoteService notes)
    {
        if (notes.HasNote(week, text))
            label.AddToClassList("text-noted");

        label.RegisterCallback<ClickEvent>(evt => {
            if (notes.HasNote(week, text))
            {
                notes.RemoveNote(week, text);
                label.RemoveFromClassList("text-noted");
            }
            else
            {
                notes.AddNote(week, text, source);
                label.AddToClassList("text-noted");
                if (ProceduralAudio.Instance != null)
                    ProceduralAudio.Instance.PlayPaperFlip();
            }
            if (EvidenceBoard.Instance != null)
                EvidenceBoard.Instance.RefreshFromChoices();
        });
    }

    static VisualElement Spacer(int h = 10)
    {
        var s = new VisualElement();
        s.style.height = h;
        return s;
    }
}
