using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class ConfrontationUI : MonoBehaviour, IPanelController
{
    const string PanelName = "confrontation-panel";
    public static string PendingPersonA;
    public static string PendingPersonB;

    string _personA;
    string _personB;

    void Start()
    {
        UIManager.Instance.RegisterController(PanelName, this);
    }

    public void OnShow()
    {
        _personA = PendingPersonA;
        _personB = PendingPersonB;
        PendingPersonA = null;
        PendingPersonB = null;

        var actions = ServiceLocator.Get<ActionService>();
        if (!string.IsNullOrEmpty(_personA) && !string.IsNullOrEmpty(_personB)
            && !actions.IsConfrontationDone(_personA, _personB))
        {
            actions.CommitAction(ActionType.Confrontation, $"{_personA}|{_personB}");
            actions.MarkConfrontationDone(_personA, _personB);
            var state = ServiceLocator.Get<GameStateService>();
            UIManager.Instance.UpdateMovesCounter(state.MovesRemaining, state.PressPenalty);
        }

        BuildPanel();
    }

    void BuildPanel()
    {
        var root = UIManager.Instance.GetRoot();
        var panel = root.Q<VisualElement>(PanelName);
        panel.Clear();

        var cases = ServiceLocator.Get<CaseService>();
        var deduction = ServiceLocator.Get<DeductionService>();
        var c = cases.ActiveCase;
        if (c == null || string.IsNullOrEmpty(_personA) || string.IsNullOrEmpty(_personB))
        {
            UIManager.Instance.ShowPanel("command-center-panel");
            return;
        }

        var conf = c.confrontations?.FirstOrDefault(x =>
            (x.personA == _personA && x.personB == _personB) ||
            (x.personA == _personB && x.personB == _personA));
        if (conf == null)
        {
            UIManager.Instance.ShowPanel("command-center-panel");
            return;
        }

        string nameA = GetPersonName(c, conf.personA);
        string nameB = GetPersonName(c, conf.personB);

        var closeRow = new VisualElement();
        closeRow.AddToClassList("close-row");
        var closeBtn = new Button(() => UIManager.Instance.ShowPanel("command-center-panel"));
        closeBtn.text = "\u2715";
        closeBtn.AddToClassList("btn-close");
        closeRow.Add(closeBtn);
        panel.Add(closeRow);

        var title = new Label($"ОЧНАЯ СТАВКА");
        title.AddToClassList("title");
        panel.Add(title);

        var vsLabel = new Label($"{nameA}  vs  {nameB}");
        vsLabel.AddToClassList("header-center");
        panel.Add(vsLabel);

        panel.Add(Spacer(15));

        // Result text with typewriter effect
        var resultBox = new VisualElement();
        resultBox.AddToClassList("box");
        resultBox.style.borderLeftWidth = 3;
        resultBox.style.borderLeftColor = new Color(1f, 0.4f, 0.2f);

        var resultLabel = new Label(conf.resultText);
        resultLabel.AddToClassList("text");
        resultBox.Add(resultLabel);

        // Who breaks
        if (!string.IsNullOrEmpty(conf.whoBreaks))
        {
            string breakName = GetPersonName(c, conf.whoBreaks);
            panel.Add(Spacer(5));
            var breakLabel = new Label($"{breakName} не выдержал давления.");
            breakLabel.AddToClassList("text-bold");
            breakLabel.AddToClassList("text-red");
            resultBox.Add(breakLabel);
        }

        panel.Add(resultBox);
        UIAnimations.FadeIn(resultBox, 500);

        // Reveal fragment
        if (!string.IsNullOrEmpty(conf.revealedFragmentId))
        {
            deduction.RevealFragment(conf.revealedFragmentId);

            panel.Add(Spacer(5));
            var fragBox = new VisualElement();
            fragBox.AddToClassList("box");
            fragBox.style.borderLeftColor = new Color(0.2f, 0.8f, 0.8f);
            var fragLabel = new Label("[+ Новый фрагмент добавлен на доску дедукции]");
            fragLabel.AddToClassList("text-bold");
            fragLabel.AddToClassList("text-cyan");
            fragBox.Add(fragLabel);
            panel.Add(fragBox);
            UIAnimations.FadeIn(fragBox, 600);
        }

        panel.Add(Spacer(20));

        var backBtn = new Button(() => UIManager.Instance.ShowPanel("command-center-panel"));
        backBtn.text = "ВЕРНУТЬСЯ В КОМАНДНЫЙ ЦЕНТР";
        backBtn.AddToClassList("btn-wide");
        panel.Add(backBtn);
    }

    string GetPersonName(CaseSO c, string personId)
    {
        if (c.persons == null) return personId;
        var p = c.persons.FirstOrDefault(x => x.personId == personId);
        return p != null ? p.displayName : personId;
    }

    public void OnHide() { }

    static VisualElement Spacer(int h = 10)
    {
        var s = new VisualElement();
        s.style.height = h;
        return s;
    }
}
