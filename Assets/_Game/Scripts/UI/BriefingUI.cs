using UnityEngine;
using UnityEngine.UIElements;

public class BriefingUI : MonoBehaviour, IPanelController
{
    const string PanelName = "briefing-panel";
    VerdictType _selected = VerdictType.None;

    void Start()
    {
        UIManager.Instance.RegisterController(PanelName, this);
    }

    public void OnShow()
    {
        _selected = VerdictType.None;
        BuildPanel();
    }

    void BuildPanel()
    {
        var root = UIManager.Instance.GetRoot();
        var panel = root.Q<VisualElement>(PanelName);
        panel.Clear();

        var cases = ServiceLocator.Get<CaseService>();
        var s = cases.ActiveCase;
        if (s == null) return;

        var title = new Label("ПЯТНИЦА — КОМИССИЯ");
        title.AddToClassList("title");
        panel.Add(title);

        panel.Add(Spacer());

        var caseName = new Label($"Дело: {s.displayName}");
        caseName.AddToClassList("header-center");
        panel.Add(caseName);

        panel.Add(Spacer(30));

        var verdictLabel = new Label("Ваш вердикт:");
        verdictLabel.AddToClassList("text");
        verdictLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        verdictLabel.style.fontSize = 18;
        panel.Add(verdictLabel);

        panel.Add(Spacer(20));

        // Verdict buttons
        var row = new VisualElement();
        row.AddToClassList("row-center");

        var guiltyBtn = new Button(() => {
            _selected = VerdictType.Guilty;
            BuildPanel();
        });
        guiltyBtn.text = "ВИНОВЕН";
        guiltyBtn.AddToClassList("btn-verdict");
        if (_selected == VerdictType.Guilty)
            guiltyBtn.AddToClassList("btn-verdict-guilty-selected");
        row.Add(guiltyBtn);

        var ngBtn = new Button(() => {
            _selected = VerdictType.NotGuilty;
            BuildPanel();
        });
        ngBtn.text = "НЕ ВИНОВЕН";
        ngBtn.AddToClassList("btn-verdict");
        if (_selected == VerdictType.NotGuilty)
            ngBtn.AddToClassList("btn-verdict-not-guilty-selected");
        row.Add(ngBtn);

        panel.Add(row);

        panel.Add(Spacer(30));

        // Sign button
        var signBtn = new Button(() => {
            var verdicts = ServiceLocator.Get<VerdictService>();
            verdicts.SetVerdict(_selected);
            verdicts.CommitAll(s.suspectId, ServiceLocator.Get<GameStateService>().CurrentWeek, s);
            UIManager.Instance.HideAllPanels();
            OfficeController.Instance.AfterVerdictCommit();
        });
        signBtn.text = "ПОДПИСАТЬ ПРОТОКОЛ";
        signBtn.AddToClassList("btn-sign");
        signBtn.SetEnabled(_selected != VerdictType.None);
        panel.Add(signBtn);
    }

    public void OnHide()
    {
        _selected = VerdictType.None;
    }

    static VisualElement Spacer(int h = 15)
    {
        var s = new VisualElement();
        s.style.height = h;
        return s;
    }
}
