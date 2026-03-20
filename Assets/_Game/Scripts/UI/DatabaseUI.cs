using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class DatabaseUI : MonoBehaviour, IPanelController
{
    const string PanelName = "database-panel";
    public static string PendingQueryId;

    string _queryId;

    void Start()
    {
        UIManager.Instance.RegisterController(PanelName, this);
    }

    public void OnShow()
    {
        _queryId = PendingQueryId;
        PendingQueryId = null;

        var actions = ServiceLocator.Get<ActionService>();
        if (!string.IsNullOrEmpty(_queryId) && !actions.IsQueryMade(_queryId))
        {
            actions.CommitAction(ActionType.DatabaseQuery, _queryId);
            actions.MarkQueryMade(_queryId);
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
        if (c == null || string.IsNullOrEmpty(_queryId))
        {
            UIManager.Instance.ShowPanel("command-center-panel");
            return;
        }

        var query = c.databaseQueries?.FirstOrDefault(q => q.queryId == _queryId);
        if (query == null)
        {
            UIManager.Instance.ShowPanel("command-center-panel");
            return;
        }

        var closeRow = new VisualElement();
        closeRow.AddToClassList("close-row");
        var closeBtn = new Button(() => UIManager.Instance.ShowPanel("command-center-panel"));
        closeBtn.text = "\u2715";
        closeBtn.AddToClassList("btn-close");
        closeRow.Add(closeBtn);
        panel.Add(closeRow);

        var title = new Label("ЗАПРОС ПО БАЗЕ ДАННЫХ");
        title.AddToClassList("header");
        panel.Add(title);

        panel.Add(Spacer(5));

        var queryLabel = new Label($"Запрос: {query.displayName}");
        queryLabel.AddToClassList("text-bold");
        queryLabel.AddToClassList("text-amber");
        panel.Add(queryLabel);

        panel.Add(Spacer(10));

        var resultBox = new VisualElement();
        resultBox.AddToClassList("box");

        var resultLabel = new Label(query.resultText);
        resultLabel.AddToClassList("text");
        resultBox.Add(resultLabel);

        // Reveal fragment
        if (!string.IsNullOrEmpty(query.revealedFragmentId))
        {
            deduction.RevealFragment(query.revealedFragmentId);

            panel.Add(Spacer(5));
            var fragLabel = new Label("[+ Новый фрагмент добавлен на доску дедукции]");
            fragLabel.AddToClassList("text-small");
            fragLabel.AddToClassList("text-cyan");
            resultBox.Add(fragLabel);
        }

        panel.Add(resultBox);
        UIAnimations.FadeIn(resultBox, 400);

        panel.Add(Spacer(20));

        var backBtn = new Button(() => UIManager.Instance.ShowPanel("command-center-panel"));
        backBtn.text = "ВЕРНУТЬСЯ В КОМАНДНЫЙ ЦЕНТР";
        backBtn.AddToClassList("btn-wide");
        panel.Add(backBtn);
    }

    public void OnHide() { }

    static VisualElement Spacer(int h = 10)
    {
        var s = new VisualElement();
        s.style.height = h;
        return s;
    }
}
