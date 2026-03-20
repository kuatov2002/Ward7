using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class CaseResultUI : MonoBehaviour, IPanelController
{
    const string PanelName = "case-result-panel";

    void Start()
    {
        UIManager.Instance.RegisterController(PanelName, this);
    }

    public void OnShow()
    {
        BuildPanel();
    }

    void BuildPanel()
    {
        var root = UIManager.Instance.GetRoot();
        var panel = root.Q<VisualElement>(PanelName);
        panel.Clear();

        var save = ServiceLocator.Get<SaveService>();
        var cases = ServiceLocator.Get<CaseService>();
        var state = ServiceLocator.Get<GameStateService>();
        var c = cases.ActiveCase;

        var lastResult = save.Data.caseResults.LastOrDefault();
        if (lastResult == null) return;

        var title = new Label("РЕЗУЛЬТАТ ДЕЛА");
        title.AddToClassList("title");
        panel.Add(title);

        panel.Add(Spacer(15));

        string caseName = c != null ? c.displayName : lastResult.caseId;
        var caseLabel = new Label($"Дело: {caseName}");
        caseLabel.AddToClassList("header-center");
        panel.Add(caseLabel);

        panel.Add(Spacer(10));

        // Result display
        var resultBox = new VisualElement();
        resultBox.AddToClassList("box");

        string resultText;
        string resultClass;

        switch (lastResult.result)
        {
            case CaseResult.CorrectArrest:
                resultText = "ПРАВИЛЬНЫЙ АРЕСТ";
                resultClass = "result-correct";
                resultBox.AddToClassList(resultClass);
                break;
            case CaseResult.WrongArrest:
                resultText = "ОШИБОЧНЫЙ АРЕСТ";
                resultClass = "result-wrong";
                resultBox.AddToClassList(resultClass);
                break;
            case CaseResult.Unsolved:
                resultText = "ДЕЛО НЕРАСКРЫТО";
                resultClass = "result-unsolved";
                resultBox.AddToClassList(resultClass);
                break;
            case CaseResult.WeakCase:
                resultText = "СЛАБАЯ ДОКАЗАТЕЛЬНАЯ БАЗА";
                resultClass = "result-weak";
                resultBox.AddToClassList(resultClass);
                break;
            default:
                resultText = "???";
                resultClass = "";
                break;
        }

        var resultLabel = new Label(resultText);
        resultLabel.AddToClassList("title");
        resultLabel.style.fontSize = 22;
        resultBox.Add(resultLabel);

        panel.Add(resultBox);

        panel.Add(Spacer(10));

        // Consequence details
        if (c != null)
        {
            CaseConsequenceData consData = lastResult.result switch
            {
                CaseResult.CorrectArrest => c.consequenceCorrectArrest,
                CaseResult.WrongArrest => c.consequenceWrongArrest,
                CaseResult.Unsolved => c.consequenceUnsolved,
                CaseResult.WeakCase => c.consequenceWeakCase,
                _ => null
            };

            if (consData != null)
            {
                if (!string.IsNullOrEmpty(consData.headlineText))
                {
                    var headlineBox = new VisualElement();
                    headlineBox.AddToClassList("box");
                    var headlineLabel = new Label(consData.headlineText);
                    headlineLabel.AddToClassList("text-bold");
                    headlineBox.Add(headlineLabel);

                    if (!string.IsNullOrEmpty(consData.detailText))
                    {
                        var detailLabel = new Label(consData.detailText);
                        detailLabel.AddToClassList("text");
                        headlineBox.Add(detailLabel);
                    }

                    panel.Add(headlineBox);
                    UIAnimations.FadeIn(headlineBox, 500);
                }
            }

            // Show true culprit for wrong arrests
            if (lastResult.result == CaseResult.WrongArrest && !string.IsNullOrEmpty(c.trueCulpritId))
            {
                panel.Add(Spacer(5));
                string culpritName = GetPersonName(c, c.trueCulpritId);
                var culpritLabel = new Label($"Настоящий виновный — {culpritName} — остался на свободе.");
                culpritLabel.AddToClassList("text-bold");
                culpritLabel.AddToClassList("text-red");
                culpritLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                panel.Add(culpritLabel);
            }

            if (lastResult.result == CaseResult.WeakCase)
            {
                panel.Add(Spacer(5));
                var weakLabel = new Label("Адвокат разрушил обвинение в суде. Подозреваемый вышел на свободу.");
                weakLabel.AddToClassList("text-bold");
                weakLabel.AddToClassList("text-amber");
                weakLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                panel.Add(weakLabel);
            }

            if (lastResult.result == CaseResult.Unsolved)
            {
                panel.Add(Spacer(5));
                var pressLabel = new Label("Давление прессы усиливается. В следующем деле у вас будет на 1 ход меньше.");
                pressLabel.AddToClassList("text-bold");
                pressLabel.AddToClassList("text-red");
                pressLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                panel.Add(pressLabel);
            }
        }

        panel.Add(Spacer(20));

        var continueBtn = new Button(() => {
            state.ResetForNewCase();
            state.AdvanceCase();

            if (state.IsGameComplete)
            {
                UIManager.Instance.ShowPanel("ending-panel");
            }
            else
            {
                cases.LoadCase(state.CurrentCase);
                if (cases.ActiveCase != null)
                {
                    state.InitCase(cases.ActiveCase.totalMoves);
                    ServiceLocator.Get<DeductionService>().SetActiveCase(cases.ActiveCase);
                }
                UIManager.Instance.ShowPanel("case-briefing-panel");
            }
        });
        continueBtn.text = state.IsGameComplete ? "ФИНАЛЬНЫЙ ОТЧЁТ" : "СЛЕДУЮЩЕЕ ДЕЛО";
        continueBtn.AddToClassList("btn-wide");
        panel.Add(continueBtn);
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
