using UnityEngine;
using UnityEngine.UIElements;

public class InterrogationUI : MonoBehaviour, IPanelController
{
    const string PanelName = "interrogation-panel";

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

        var title = new Label($"ДОПРОС: {s.displayName}");
        title.AddToClassList("header");
        panel.Add(title);

        panel.Add(Spacer());

        var scroll = new ScrollView(ScrollViewMode.Vertical);
        scroll.style.maxHeight = 550;
        scroll.style.flexGrow = 1;

        // Standard questions
        var stdHeader = new Label("СТАНДАРТНЫЕ ВОПРОСЫ:");
        stdHeader.AddToClassList("text-bold");
        scroll.Add(stdHeader);
        scroll.Add(Spacer(5));

        if (s.standardQuestions != null)
        {
            foreach (var qa in s.standardQuestions)
            {
                var box = new VisualElement();
                box.AddToClassList("box");
                var q = new Label($"\u2014 {qa.question}");
                q.AddToClassList("text-bold");
                box.Add(q);
                box.Add(Spacer(3));
                var a = new Label(qa.answer);
                a.AddToClassList("text");
                box.Add(a);
                scroll.Add(box);
            }
        }

        // Conditional questions
        if (s.conditionalQuestions != null)
        {
            bool hasAny = false;
            foreach (var cq in s.conditionalQuestions)
            {
                if (choices.GetSelected(w, cq.requiredChoiceType) == cq.requiredChoiceId)
                {
                    if (!hasAny)
                    {
                        scroll.Add(Spacer());
                        var condHeader = new Label("РАЗБЛОКИРОВАННЫЕ ВОПРОСЫ:");
                        condHeader.AddToClassList("text-bold");
                        condHeader.AddToClassList("text-yellow");
                        scroll.Add(condHeader);
                        scroll.Add(Spacer(5));
                        hasAny = true;
                    }

                    var box = new VisualElement();
                    box.AddToClassList("box");
                    var q = new Label($"\u2014 {cq.question}");
                    q.AddToClassList("text-bold");
                    box.Add(q);
                    box.Add(Spacer(3));
                    var a = new Label(cq.answer);
                    a.AddToClassList("text");
                    box.Add(a);
                    scroll.Add(box);
                }
            }
        }

        // Follow-up questions
        scroll.Add(Spacer(15));
        var fuHeader = new Label("ВОПРОС ВНЕ ПРОТОКОЛА (выберите один):");
        fuHeader.AddToClassList("text-bold");
        fuHeader.AddToClassList("text-accent");
        scroll.Add(fuHeader);
        scroll.Add(Spacer(5));

        bool fuDone = choices.IsChosen(w, ChoiceType.FollowUp);
        string fuSel = choices.GetSelected(w, ChoiceType.FollowUp);

        if (s.followUps != null)
        {
            foreach (var fu in s.followUps)
            {
                bool mine = fuSel == fu.followUpId;
                var box = new VisualElement();
                box.AddToClassList("box");

                if (mine)
                {
                    var q = new Label($"\u2014 {fu.question}");
                    q.AddToClassList("text-bold");
                    q.AddToClassList("text-green");
                    box.Add(q);
                    box.Add(Spacer(3));
                    var a = new Label(fu.answer);
                    a.AddToClassList("text");
                    box.Add(a);
                }
                else if (!fuDone)
                {
                    var btn = new Button(() => {
                        choices.Commit(w, ChoiceType.FollowUp, fu.followUpId);
                        OnShow();
                    });
                    btn.text = fu.question;
                    btn.AddToClassList("btn-small");
                    box.Add(btn);
                }
                else
                {
                    var q = new Label($"\u2014 {fu.question}");
                    q.AddToClassList("text");
                    q.AddToClassList("text-gray");
                    box.Add(q);
                }

                scroll.Add(box);
            }
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
