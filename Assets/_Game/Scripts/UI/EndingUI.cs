using UnityEngine;
using UnityEngine.UIElements;

public class EndingUI : MonoBehaviour, IPanelController
{
    const string PanelName = "ending-panel";

    void Start()
    {
        UIManager.Instance.RegisterController(PanelName, this);
    }

    public void OnShow()
    {
        var root = UIManager.Instance.GetRoot();
        var panel = root.Q<VisualElement>(PanelName);
        panel.Clear();

        var save = ServiceLocator.Get<SaveService>();
        var cases = ServiceLocator.Get<CaseService>();

        var title = new Label("ФИНАЛЬНЫЙ ОТЧЁТ");
        title.AddToClassList("title");
        panel.Add(title);

        panel.Add(Spacer(20));

        var sub = new Label("Все вердикты и их последствия:");
        sub.AddToClassList("header");
        panel.Add(sub);

        panel.Add(Spacer());

        var scroll = new ScrollView(ScrollViewMode.Vertical);
        scroll.style.maxHeight = 500;
        scroll.style.flexGrow = 1;

        foreach (var v in save.Data.verdicts)
        {
            var suspect = cases.GetCase(v.week);
            string name = suspect != null ? suspect.displayName : v.suspectId;
            string vStr = v.verdict == VerdictType.Guilty ? "ВИНОВЕН" : "НЕ ВИНОВЕН";
            bool correct = suspect != null &&
                ((v.verdict == VerdictType.Guilty && suspect.isGuilty) ||
                 (v.verdict == VerdictType.NotGuilty && !suspect.isGuilty));

            var box = new VisualElement();
            box.AddToClassList("box");

            var weekLabel = new Label($"Неделя {v.week}: {name}");
            weekLabel.AddToClassList("text-bold");
            box.Add(weekLabel);

            var verdictLabel = new Label($"Вердикт: {vStr}");
            verdictLabel.AddToClassList("text");
            verdictLabel.AddToClassList(correct ? "text-green" : "text-red");
            box.Add(verdictLabel);

            if (suspect != null)
            {
                string c = v.verdict == VerdictType.Guilty
                    ? suspect.consequenceGuilty
                    : suspect.consequenceNotGuilty;
                if (!string.IsNullOrEmpty(c))
                {
                    box.Add(Spacer(3));
                    var consq = new Label(c);
                    consq.AddToClassList("text");
                    box.Add(consq);
                }
            }

            scroll.Add(box);
        }

        panel.Add(scroll);

        panel.Add(Spacer(20));

        var menuBtn = new Button(() => {
            UIManager.Instance.HideAllPanels();
            UIManager.Instance.ShowPanel("main-menu-panel");
        });
        menuBtn.text = "ГЛАВНОЕ МЕНЮ";
        menuBtn.AddToClassList("btn-wide");
        panel.Add(menuBtn);
    }

    public void OnHide() { }

    static VisualElement Spacer(int h = 10)
    {
        var s = new VisualElement();
        s.style.height = h;
        return s;
    }
}
