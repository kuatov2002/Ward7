using UnityEngine;
using UnityEngine.UIElements;

public class ContactUI : MonoBehaviour, IPanelController
{
    const string PanelName = "contact-panel";
    float _savedScroll;

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
        var s = cases.ActiveCase;
        if (s == null) return;
        int w = state.CurrentWeek;

        // Close row
        var closeRow = new VisualElement();
        closeRow.AddToClassList("close-row");
        var closeBtn = new Button(() => UIManager.Instance.HideAllPanels());
        closeBtn.text = "\u2715";
        closeBtn.AddToClassList("btn-close");
        closeRow.Add(closeBtn);
        panel.Add(closeRow);

        var title = new Label("КОНТАКТЫ");
        title.AddToClassList("header");
        panel.Add(title);

        var sub = new Label("Выберите одного для звонка. Это единственный звонок за день — выбирайте внимательно.");
        sub.AddToClassList("text");
        panel.Add(sub);

        panel.Add(Spacer());

        var scroll = new ScrollView(ScrollViewMode.Vertical);
        scroll.style.maxHeight = 500;
        scroll.style.flexGrow = 1;

        bool done = choices.IsChosen(w, ChoiceType.Contact);
        string sel = choices.GetSelected(w, ChoiceType.Contact);

        foreach (var c in s.contacts)
        {
            bool mine = sel == c.contactId;
            var box = new VisualElement();
            box.AddToClassList("box");

            var name = new Label(c.displayName);
            name.AddToClassList("text-bold");
            box.Add(name);

            if (mine)
            {
                var tag = new Label("[ЗВОНОК СДЕЛАН]");
                tag.AddToClassList("text");
                tag.AddToClassList("text-green");
                box.Add(tag);
                box.Add(Spacer(5));
                var resp = new Label(c.response);
                resp.name = "resp-" + c.contactId;
                resp.AddToClassList("text");
                box.Add(resp);
            }
            else if (!done)
            {
                string cId = c.contactId;
                string cResp = c.response;
                var btn = new Button(() => {
                    choices.Commit(w, ChoiceType.Contact, cId);
                    OnShow();
                    // Typewriter on the response
                    var respLabel = UIManager.Instance.GetRoot()
                        .Q<VisualElement>(PanelName)
                        .Q<Label>("resp-" + cId);
                    if (respLabel != null)
                        StartCoroutine(TypewriterEffect.Run(respLabel, cResp));
                });
                btn.text = "Позвонить";
                btn.AddToClassList("btn-small");
                btn.style.width = 140;
                box.Add(btn);
            }
            else
            {
                var unavail = new Label("[выбор сделан — недоступен]");
                unavail.AddToClassList("text");
                unavail.AddToClassList("text-gray");
                box.Add(unavail);
            }

            scroll.Add(box);
        }

        panel.Add(scroll);
        scroll.schedule.Execute(() => scroll.scrollOffset = new Vector2(0, _savedScroll));
    }

    public void OnHide() { }

    static VisualElement Spacer(int h = 10)
    {
        var s = new VisualElement();
        s.style.height = h;
        return s;
    }
}
