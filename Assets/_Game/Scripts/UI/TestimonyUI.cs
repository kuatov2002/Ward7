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
        var s = cases.ActiveCase;
        if (s == null) return;
        int w = state.CurrentWeek;

        var closeBtn = new Button(() => UIManager.Instance.HideAllPanels());
        closeBtn.text = "X";
        closeBtn.AddToClassList("btn-close");
        panel.Add(closeBtn);

        var title = new Label("ПОКАЗАНИЯ");
        title.AddToClassList("header");
        panel.Add(title);

        var sub = new Label("Три источника. Запросите уточнение у ОДНОГО.");
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
                box.Add(clar);
            }
            else if (!done)
            {
                box.Add(Spacer(5));
                var btn = new Button(() => {
                    choices.Commit(w, ChoiceType.Testimony, t.witnessName);
                    OnShow();
                });
                btn.text = "Запросить уточнение";
                btn.AddToClassList("btn-small");
                btn.style.width = 220;
                box.Add(btn);
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
