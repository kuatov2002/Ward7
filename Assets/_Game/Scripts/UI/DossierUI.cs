using UnityEngine;
using UnityEngine.UIElements;

public class DossierUI : MonoBehaviour, IPanelController
{
    const string PanelName = "dossier-panel";

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
        var s = cases.ActiveCase;
        if (s == null) return;

        // Close button
        var closeBtn = new Button(() => UIManager.Instance.HideAllPanels());
        closeBtn.text = "X";
        closeBtn.AddToClassList("btn-close");
        panel.Add(closeBtn);

        var title = new Label($"ДОСЬЕ: {s.displayName}");
        title.AddToClassList("header");
        panel.Add(title);

        var spacer = new VisualElement();
        spacer.style.height = 10;
        panel.Add(spacer);

        var scroll = new ScrollView(ScrollViewMode.Vertical);
        scroll.style.maxHeight = 500;
        scroll.style.flexGrow = 1;

        var box = new VisualElement();
        box.AddToClassList("box");
        var text = new Label(s.dossierText);
        text.AddToClassList("text");
        box.Add(text);
        scroll.Add(box);

        panel.Add(scroll);
    }

    public void OnHide() { }
}
