using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class OutcomeUI : MonoBehaviour, IPanelController
{
    const string PanelName = "outcome-panel";

    void Start()
    {
        UIManager.Instance.RegisterController(PanelName, this);
    }

    public void OnShow()
    {
        var root = UIManager.Instance.GetRoot();
        var panel = root.Q<VisualElement>(PanelName);
        panel.Clear();

        var state = ServiceLocator.Get<GameStateService>();
        var conseq = ServiceLocator.Get<ConsequenceService>();
        var cases = ServiceLocator.Get<CaseService>();

        var title = new Label($"НЕДЕЛЯ {state.CurrentWeek} — ПОНЕДЕЛЬНИК");
        title.AddToClassList("title");
        panel.Add(title);

        panel.Add(Spacer());

        List<string> headlines = conseq.ResolveWeek(state.CurrentWeek)
            .Where(h => !string.IsNullOrEmpty(h)).ToList();

        if (state.CurrentWeek == 1 && headlines.Count == 0)
        {
            var intro = new Label("На ваш стол легло новое дело.");
            intro.AddToClassList("header-center");
            panel.Add(intro);
        }
        else if (headlines.Count == 0)
        {
            var noNews = new Label("— Новых последствий нет —");
            noNews.AddToClassList("header-center");
            panel.Add(noNews);
        }
        else
        {
            var hdr = new Label("ПОСЛЕДСТВИЯ ВАШИХ РЕШЕНИЙ:");
            hdr.AddToClassList("header");
            panel.Add(hdr);

            var consExplain = new Label("Ваши вердикты предыдущих недель привели к этим событиям:");
            consExplain.AddToClassList("text-small");
            consExplain.AddToClassList("text-dim");
            panel.Add(consExplain);

            panel.Add(Spacer(10));

            foreach (var h in headlines)
            {
                var box = new VisualElement();
                box.AddToClassList("box");
                var lbl = new Label(h);
                lbl.AddToClassList("text");
                box.Add(lbl);
                panel.Add(box);
            }
        }

        var suspect = cases.ActiveCase;
        if (suspect != null)
        {
            panel.Add(Spacer());
            var caseName = new Label($"Дело: {suspect.displayName}");
            caseName.AddToClassList("header-center");
            panel.Add(caseName);
        }

        panel.Add(Spacer(30));

        var btn = new Button(() => {
            state.AdvanceDay();
            UIManager.Instance.HideAllPanels();
            OfficeController.Instance.RefreshDesk();
        });
        btn.text = "ДАЛЕЕ";
        btn.AddToClassList("btn-wide");
        panel.Add(btn);
    }

    public void OnHide() { }

    static VisualElement Spacer(int h = 15)
    {
        var s = new VisualElement();
        s.style.height = h;
        return s;
    }
}
