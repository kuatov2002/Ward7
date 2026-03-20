using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuUI : MonoBehaviour, IPanelController
{
    const string PanelName = "main-menu-panel";

    void Start()
    {
        UIManager.Instance.RegisterController(PanelName, this);
    }

    public void OnShow()
    {
        var root = UIManager.Instance.GetRoot();
        var panel = root.Q<VisualElement>(PanelName);
        panel.Clear();

        var title = new Label("PROFILE 7");
        title.AddToClassList("title");
        panel.Add(title);

        var sub = new Label("Детективная головоломка");
        sub.AddToClassList("text");
        sub.style.unityTextAlign = TextAnchor.MiddleCenter;
        sub.style.marginBottom = 40;
        panel.Add(sub);

        var btnContainer = new VisualElement();
        btnContainer.style.alignItems = Align.Center;

        var btnNew = new Button(() => {
            GameManager.Instance.StartNewGame();
            UIManager.Instance.ShowPanel("case-briefing-panel");
        });
        btnNew.text = "НОВАЯ ИГРА";
        btnNew.AddToClassList("btn-wide");
        btnContainer.Add(btnNew);

        var save = ServiceLocator.Get<SaveService>();
        var btnCont = new Button(() => {
            GameManager.Instance.ContinueGame();
            UIManager.Instance.HideAllPanels();
            OfficeController.Instance.OnGameStarted();
        });
        btnCont.text = "ПРОДОЛЖИТЬ";
        btnCont.AddToClassList("btn-wide");
        btnCont.SetEnabled(save != null && save.HasSave());
        btnContainer.Add(btnCont);

        panel.Add(btnContainer);
    }

    public void OnHide() { }
}
