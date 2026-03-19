using UnityEngine;
using UnityEngine.UIElements;

public class EvidenceUI : MonoBehaviour, IPanelController
{
    const string PanelName = "evidence-panel";

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

        var title = new Label($"УЛИКИ: {s.displayName}");
        title.AddToClassList("header");
        panel.Add(title);

        bool done = choices.IsChosen(w, ChoiceType.Evidence);
        string sel = choices.GetSelected(w, ChoiceType.Evidence);

        var sub = new Label(done
            ? "Приоритетная улика проанализирована полностью. Остальные — частично."
            : "Все улики будут исследованы, но только ОДНА получит полный анализ. Остальные — частично. Полный анализ может разблокировать вопросы на допросе.");
        sub.AddToClassList("text");
        panel.Add(sub);
        panel.Add(Spacer());

        var scroll = new ScrollView(ScrollViewMode.Vertical);
        scroll.style.maxHeight = 500;
        scroll.style.flexGrow = 1;

        foreach (var ev in s.evidence)
        {
            bool mine = sel == ev.evidenceId;
            var box = new VisualElement();
            box.AddToClassList("box");

            var evTitle = new Label(ev.title);
            evTitle.AddToClassList("text-bold");
            box.Add(evTitle);
            box.Add(Spacer(5));

            // Base description — always visible, clickable for notes
            var baseLabel = new Label(ev.baseDescription);
            baseLabel.AddToClassList("text");
            MakeNoteable(baseLabel, ev.baseDescription, $"evidence_{ev.evidenceId}", w, notes);
            box.Add(baseLabel);

            if (done)
            {
                box.Add(Spacer(5));
                if (mine)
                {
                    var tag = new Label("[ПОЛНАЯ ЭКСПЕРТИЗА]");
                    tag.AddToClassList("text");
                    tag.AddToClassList("text-cyan");
                    box.Add(tag);
                    box.Add(Spacer(3));
                    var expert = new Label(ev.expertDescription);
                    expert.AddToClassList("text");
                    MakeNoteable(expert, ev.expertDescription, $"expert_{ev.evidenceId}", w, notes);
                    box.Add(expert);
                }
                else
                {
                    var tag = new Label("[ЧАСТИЧНЫЙ АНАЛИЗ]");
                    tag.AddToClassList("text");
                    tag.AddToClassList("text-yellow");
                    box.Add(tag);
                    box.Add(Spacer(3));
                    string partial = GetPartialText(ev.expertDescription, 2);
                    var partialLabel = new Label(partial + " [Анализ не завершён — выберите эту улику приоритетной для полного результата]");
                    partialLabel.AddToClassList("text");
                    partialLabel.AddToClassList("text-gray");
                    box.Add(partialLabel);
                }
            }
            else
            {
                box.Add(Spacer(5));
                string eid = ev.evidenceId;
                var btn = new Button(() => {
                    choices.Commit(w, ChoiceType.Evidence, eid);
                    OnShow();
                });
                btn.text = "Приоритет на экспертизу";
                btn.AddToClassList("btn-small");
                btn.style.width = 240;
                box.Add(btn);
            }

            scroll.Add(box);
        }

        panel.Add(scroll);

        var noteHint = new Label("Совет: кликните по тексту чтобы сделать заметку для вердикта");
        noteHint.AddToClassList("text-small");
        noteHint.AddToClassList("text-dim");
        panel.Add(noteHint);
    }

    public void OnHide() { }

    static string GetPartialText(string full, int sentences)
    {
        if (string.IsNullOrEmpty(full)) return "";
        int count = 0;
        for (int i = 0; i < full.Length; i++)
        {
            if (full[i] == '.' || full[i] == '!' || full[i] == '?')
            {
                count++;
                if (count >= sentences)
                    return full.Substring(0, i + 1);
            }
        }
        return full;
    }

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
