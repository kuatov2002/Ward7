using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class EndingUI : MonoBehaviour, IPanelController
{
    const string PanelName = "ending-panel";

    void Start()
    {
        UIManager.Instance.RegisterController(PanelName, this);
    }

    public void OnShow()
    {
        var root = UIManager.Instance.GetRoot();
        var panel = root.Q<VisualElement>(PanelName);
        panel.Clear();

        var save = ServiceLocator.Get<SaveService>();
        var cases = ServiceLocator.Get<CaseService>();

        var title = new Label("ФИНАЛЬНЫЙ ОТЧЁТ");
        title.AddToClassList("title");
        panel.Add(title);

        panel.Add(Spacer(20));

        // Career summary
        int totalCases = save.Data.caseResults.Count;
        int correct = save.Data.caseResults.Count(r => r.result == CaseResult.CorrectArrest);
        int wrong = save.Data.caseResults.Count(r => r.result == CaseResult.WrongArrest);
        int unsolved = save.Data.caseResults.Count(r => r.result == CaseResult.Unsolved);
        int weak = save.Data.caseResults.Count(r => r.result == CaseResult.WeakCase);
        int escaped = save.Data.escapedCriminals.Count;

        var summary = new Label($"Дел расследовано: {totalCases}");
        summary.AddToClassList("header-center");
        panel.Add(summary);

        panel.Add(Spacer(10));

        var statsBox = new VisualElement();
        statsBox.AddToClassList("box");

        AddStatRow(statsBox, "Правильных арестов:", correct, "text-green");
        AddStatRow(statsBox, "Ошибочных арестов:", wrong, "text-red");
        AddStatRow(statsBox, "Нераскрытых дел:", unsolved, "text-gray");
        AddStatRow(statsBox, "Слабых обвинений:", weak, "text-yellow");
        AddStatRow(statsBox, "Преступников на свободе:", escaped, "text-red");

        panel.Add(statsBox);

        panel.Add(Spacer(15));

        // Per-case breakdown
        var sub = new Label("Подробности расследований:");
        sub.AddToClassList("header");
        panel.Add(sub);

        panel.Add(Spacer());

        var scroll = new ScrollView(ScrollViewMode.Vertical);
        scroll.style.flexGrow = 1;
        scroll.style.flexShrink = 1;

        foreach (var r in save.Data.caseResults)
        {
            var caseData = cases.GetCase(r.caseNumber);
            string name = caseData != null ? caseData.displayName : r.caseId;

            var box = new VisualElement();
            box.AddToClassList("box");

            string resultClass = r.result switch
            {
                CaseResult.CorrectArrest => "result-correct",
                CaseResult.WrongArrest => "result-wrong",
                CaseResult.Unsolved => "result-unsolved",
                CaseResult.WeakCase => "result-weak",
                _ => ""
            };
            box.AddToClassList(resultClass);

            var caseLabel = new Label($"Дело #{r.caseNumber}: {name}");
            caseLabel.AddToClassList("text-bold");
            box.Add(caseLabel);

            string resultStr = r.result switch
            {
                CaseResult.CorrectArrest => "ПРАВИЛЬНЫЙ АРЕСТ",
                CaseResult.WrongArrest => "ОШИБОЧНЫЙ АРЕСТ",
                CaseResult.Unsolved => "НЕРАСКРЫТО",
                CaseResult.WeakCase => "СЛАБОЕ ОБВИНЕНИЕ",
                _ => "???"
            };

            var resultLabel = new Label(resultStr);
            resultLabel.AddToClassList("text-bold");
            resultLabel.AddToClassList(r.result == CaseResult.CorrectArrest ? "text-green" : "text-red");
            box.Add(resultLabel);

            if (!string.IsNullOrEmpty(r.accusedPersonId) && caseData != null)
            {
                var person = caseData.persons?.FirstOrDefault(p => p.personId == r.accusedPersonId);
                if (person != null)
                {
                    var accusedLabel = new Label($"Обвинён: {person.displayName}");
                    accusedLabel.AddToClassList("text");
                    box.Add(accusedLabel);
                }

                if (r.result == CaseResult.WrongArrest && !string.IsNullOrEmpty(caseData.trueCulpritId))
                {
                    var truePerson = caseData.persons?.FirstOrDefault(p => p.personId == caseData.trueCulpritId);
                    if (truePerson != null)
                    {
                        var trueLabel = new Label($"Настоящий виновный: {truePerson.displayName}");
                        trueLabel.AddToClassList("text");
                        trueLabel.AddToClassList("text-red");
                        box.Add(trueLabel);
                    }
                }
            }

            scroll.Add(box);
        }

        panel.Add(scroll);

        panel.Add(Spacer(20));

        // Career rating
        float ratio = totalCases > 0 ? (float)correct / totalCases : 0;
        string rating;
        string ratingClass;
        if (ratio >= 0.8f)
        {
            rating = "ВЫДАЮЩИЙСЯ СЛЕДОВАТЕЛЬ";
            ratingClass = "text-green";
        }
        else if (ratio >= 0.5f)
        {
            rating = "КОМПЕТЕНТНЫЙ ДЕТЕКТИВ";
            ratingClass = "text-yellow";
        }
        else
        {
            rating = "ПРОВАЛ КАРЬЕРЫ";
            ratingClass = "text-red";
        }

        var ratingLabel = new Label(rating);
        ratingLabel.AddToClassList("title");
        ratingLabel.AddToClassList(ratingClass);
        panel.Add(ratingLabel);

        panel.Add(Spacer(20));

        var menuBtn = new Button(() => {
            UIManager.Instance.HideAllPanels();
            UIManager.Instance.ShowPanel("main-menu-panel");
        });
        menuBtn.text = "ГЛАВНОЕ МЕНЮ";
        menuBtn.AddToClassList("btn-wide");
        panel.Add(menuBtn);
    }

    void AddStatRow(VisualElement parent, string label, int value, string valueClass)
    {
        var row = new VisualElement();
        row.AddToClassList("row");
        row.style.marginBottom = 2;

        var labelEl = new Label(label);
        labelEl.AddToClassList("text");
        labelEl.style.width = 200;
        row.Add(labelEl);

        var valueEl = new Label(value.ToString());
        valueEl.AddToClassList("text-bold");
        valueEl.AddToClassList(valueClass);
        row.Add(valueEl);

        parent.Add(row);
    }

    public void OnHide() { }

    static VisualElement Spacer(int h = 10)
    {
        var s = new VisualElement();
        s.style.height = h;
        return s;
    }
}
