using UnityEngine;
using UnityEngine.UIElements;

public class TestimonyUI : MonoBehaviour, IPanelController
{
    const string PanelName = "testimony-panel";

    void Start()
    {
        UIManager.Instance.RegisterController(PanelName, this);
    }

    public void OnShow()
    {
        var root = UIManager.Instance.GetRoot();
        var panel = root.Q<VisualElement>(PanelName);
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

        foreach (var t in s.testimonies)
        {
            bool mine = sel == t.witnessName;
            var box = new VisualElement();
            box.AddToClassList("box");

            var name = new Label(t.witnessName);
            name.AddToClassList("text-bold");
            box.Add(name);
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
            }
            else if (!done)
            {
                box.Add(Spacer(5));
                string wName = t.witnessName;
                var btn = new Button(() => {
                    choices.Commit(w, ChoiceType.Testimony, wName);
                    OnShow();
                });
                btn.text = "Запросить уточнение";
                btn.AddToClassList("btn-small");
                btn.style.width = 220;
                box.Add(btn);
            }

            scroll.Add(box);
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

            foreach (var c in s.contradictions)
            {
                var box = new VisualElement();
                box.AddToClassList("box");
                box.style.borderLeftWidth = 3;
                box.style.borderLeftColor = new Color(1f, 0.3f, 0.3f);

                var header = new Label($"{c.witnessA} \u2260 {c.witnessB}");
                header.AddToClassList("text-bold");
                header.style.color = new Color(1f, 0.5f, 0.4f);
                box.Add(header);
                box.Add(Spacer(3));

                var desc = new Label(c.description);
                desc.AddToClassList("text");
                MakeNoteable(desc, c.description, "contradiction", w, notes);
                box.Add(desc);

                scroll.Add(box);
            }
        }

        panel.Add(scroll);
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
