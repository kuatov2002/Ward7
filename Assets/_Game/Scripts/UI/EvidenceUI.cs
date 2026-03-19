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

        var sub = new Label("Изучите все улики. Отправьте ОДНУ на экспертизу.");
        sub.AddToClassList("text");
        panel.Add(sub);

        panel.Add(Spacer());

        var scroll = new ScrollView(ScrollViewMode.Vertical);
        scroll.style.maxHeight = 500;
        scroll.style.flexGrow = 1;

        bool done = choices.IsChosen(w, ChoiceType.Evidence);
        string sel = choices.GetSelected(w, ChoiceType.Evidence);

        foreach (var ev in s.evidence)
        {
            bool mine = sel == ev.evidenceId;
            var box = new VisualElement();
            box.AddToClassList("box");

            var evTitle = new Label(ev.title);
            evTitle.AddToClassList("text-bold");
            box.Add(evTitle);
            box.Add(Spacer(5));

            if (mine)
            {
                var tag = new Label("[ОТПРАВЛЕНО НА ЭКСПЕРТИЗУ]");
                tag.AddToClassList("text");
                tag.AddToClassList("text-cyan");
                box.Add(tag);
                box.Add(Spacer(5));
                var expert = new Label(ev.expertDescription);
                expert.AddToClassList("text");
                box.Add(expert);
            }
            else
            {
                var desc = new Label(ev.baseDescription);
                desc.AddToClassList("text");
                box.Add(desc);

                if (!done)
                {
                    box.Add(Spacer(5));
                    var btn = new Button(() => {
                        choices.Commit(w, ChoiceType.Evidence, ev.evidenceId);
                        OnShow();
                    });
                    btn.text = "Отправить на экспертизу";
                    btn.AddToClassList("btn-small");
                    btn.style.width = 220;
                    box.Add(btn);
                }
            }

            scroll.Add(box);
        }

        panel.Add(scroll);
    }

    public void OnHide() { }

    static VisualElement Spacer(int h = 10)
    {
        var s = new VisualElement();
        s.style.height = h;
        return s;
    }
}
