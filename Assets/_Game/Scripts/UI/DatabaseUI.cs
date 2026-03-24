using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class DatabaseUI : MonoBehaviour, IPanelController
{
    const string PanelName = "database-panel";
    public static string PendingQueryId;
 
    string _queryId;
 
    void Start() => UIManager.Instance.RegisterController(PanelName, this);
 
    public void OnShow()
    {
        _queryId = PendingQueryId;
        PendingQueryId = null;
 
        var actions = ServiceLocator.Get<ActionService>();
        if (!string.IsNullOrEmpty(_queryId) && !actions.IsQueryMade(_queryId))
        {
            actions.CommitAction(ActionType.DatabaseQuery, _queryId);
            actions.MarkQueryMade(_queryId);
            UIManager.Instance.UpdateMovesCounter(
                ServiceLocator.Get<GameStateService>().MovesRemaining,
                ServiceLocator.Get<GameStateService>().PressPenalty);
        }
        BuildPanel();
    }
 
    void BuildPanel()
    {
        var root  = UIManager.Instance.GetRoot();
        var panel = root.Q<VisualElement>(PanelName);
        panel.Clear();
 
        var cases         = ServiceLocator.Get<CaseService>();
        var deduction     = ServiceLocator.Get<DeductionService>();
        var contradictions = ServiceLocator.Get<ContradictionService>();
        var c = cases.ActiveCase;
 
        if (c == null || string.IsNullOrEmpty(_queryId))
        { UIManager.Instance.ShowPanel("command-center-panel"); return; }
 
        var query = c.databaseQueries?.FirstOrDefault(q => q.queryId == _queryId);
        if (query == null)
        { UIManager.Instance.ShowPanel("command-center-panel"); return; }
 
        var closeRow = new VisualElement(); closeRow.AddToClassList("close-row");
        var closeBtn = new Button(() => UIManager.Instance.ShowPanel("command-center-panel"));
        closeBtn.text = "✕"; closeBtn.AddToClassList("btn-close");
        closeRow.Add(closeBtn); panel.Add(closeRow);
 
        var title = new Label("ЗАПРОС ПО БАЗЕ ДАННЫХ");
        title.AddToClassList("header"); panel.Add(title);
        panel.Add(Spacer(5));
 
        var queryLabel = new Label($"Запрос: {query.displayName}");
        queryLabel.AddToClassList("text-bold"); queryLabel.AddToClassList("text-amber");
        panel.Add(queryLabel);
        panel.Add(Spacer(10));
 
        var resultBox = new VisualElement();
        resultBox.AddToClassList("box");
        resultBox.style.borderLeftWidth = 3;
        resultBox.style.borderLeftColor = new Color(0.3f, 0.8f, 0.3f);
 
        var resultLabel = new Label(query.resultText);
        resultLabel.AddToClassList("text"); resultBox.Add(resultLabel);
 
        if (!string.IsNullOrEmpty(query.revealedFragmentId))
        {
            // Physical — database records are objective facts
            deduction.RevealFragment(query.revealedFragmentId);
            contradictions.RegisterPhysicalFragment(query.revealedFragmentId);
 
            var fragLabel = new Label("[+ физическая улика из базы данных]");
            fragLabel.AddToClassList("text-small");
            fragLabel.style.color = new Color(0.3f, 0.8f, 0.3f);
            fragLabel.style.marginTop = 6;
            resultBox.Add(fragLabel);
        }
 
        panel.Add(resultBox);
        UIAnimations.FadeIn(resultBox, 400);
        panel.Add(Spacer(20));
 
        var backBtn = new Button(() => UIManager.Instance.ShowPanel("command-center-panel"));
        backBtn.text = "ВЕРНУТЬСЯ"; backBtn.AddToClassList("btn-wide");
        panel.Add(backBtn);
    }
 
    public void OnHide() { }
    static VisualElement Spacer(int h = 10) { var s = new VisualElement(); s.style.height = h; return s; }
}
