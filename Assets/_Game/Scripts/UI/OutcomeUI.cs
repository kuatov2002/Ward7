using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class CaseBriefingUI : MonoBehaviour, IPanelController
{
    const string PanelName = "case-briefing-panel";

    void Start()
    {
        UIManager.Instance.RegisterController(PanelName, this);
    }

    public void OnShow()
    {
        var root = UIManager.Instance.GetRoot();
        var panel = root.Q<VisualElement>(PanelName);
        panel.Clear();

        var state = ServiceLocator.Get<GameStateService>();
        var conseq = ServiceLocator.Get<ConsequenceService>();
        var cases = ServiceLocator.Get<CaseService>();

        var title = new Label($"ДЕЛО #{state.CurrentCase}");
        title.AddToClassList("title");
        panel.Add(title);

        panel.Add(Spacer());

        // Show consequences from previous case
        var consequences = conseq.ResolveForCase(state.CurrentCase);
        if (consequences.Count > 0)
        {
            var hdr = new Label("ПОСЛЕДСТВИЯ ПРЕДЫДУЩЕГО ДЕЛА:");
            hdr.AddToClassList("header");
            panel.Add(hdr);

            panel.Add(Spacer(5));

            foreach (var c in consequences)
            {
                var box = new VisualElement();
                box.AddToClassList("box");
                var lbl = new Label(c.headlineText);
                lbl.AddToClassList("text");
                box.Add(lbl);
                if (!string.IsNullOrEmpty(c.detailText))
                {
                    var detail = new Label(c.detailText);
                    detail.AddToClassList("text-small");
                    box.Add(detail);
                }
                panel.Add(box);
            }

            panel.Add(Spacer(10));
        }

        // Show escaped criminals warning
        var escaped = conseq.GetEscapedCriminals();
        if (escaped.Count > 0)
        {
            var escLabel = new Label($"Преступников на свободе: {escaped.Count}");
            escLabel.AddToClassList("text-bold");
            escLabel.AddToClassList("text-red");
            escLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            panel.Add(escLabel);
            panel.Add(Spacer(5));
        }

        // Press penalty warning
        if (state.PressPenalty > 0)
        {
            var pressLabel = new Label($"Давление прессы: -{state.PressPenalty} ходов к бюджету");
            pressLabel.AddToClassList("text-bold");
            pressLabel.AddToClassList("text-red");
            pressLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            panel.Add(pressLabel);
            panel.Add(Spacer(10));
        }

        // New case intro
        var suspect = cases.ActiveCase;
        if (suspect != null)
        {
            var caseCard = new VisualElement();
            caseCard.AddToClassList("box");
            caseCard.style.borderLeftWidth = 4;
            caseCard.style.borderLeftColor = new Color(1f, 0.7f, 0.2f);
            caseCard.style.paddingTop = 12;
            caseCard.style.paddingBottom = 12;

            var caseLabel = new Label("НОВОЕ ДЕЛО");
            caseLabel.AddToClassList("text-small");
            caseLabel.AddToClassList("text-amber");
            caseLabel.style.letterSpacing = 3;
            caseCard.Add(caseLabel);

            var caseName = new Label(suspect.displayName);
            caseName.AddToClassList("title");
            caseName.style.fontSize = 24;
            caseName.style.marginTop = 4;
            caseName.style.marginBottom = 4;
            caseCard.Add(caseName);

            if (!string.IsNullOrEmpty(suspect.briefingText))
            {
                var briefing = new Label(suspect.briefingText);
                briefing.AddToClassList("text");
                caseCard.Add(briefing);
            }

            // List suspects
            if (suspect.persons != null && suspect.persons.Length > 0)
            {
                caseCard.Add(Spacer(8));
                var personsLabel = new Label("ФИГУРАНТЫ:");
                personsLabel.AddToClassList("text-bold");
                personsLabel.AddToClassList("text-amber");
                caseCard.Add(personsLabel);

                foreach (var p in suspect.persons)
                {
                    string roleStr = p.role == PersonRole.Suspect ? "подозреваемый" : "свидетель";
                    var personLabel = new Label($"\u2022 {p.displayName} ({roleStr})");
                    personLabel.AddToClassList("text");
                    caseCard.Add(personLabel);
                }
            }

            // Budget info
            caseCard.Add(Spacer(8));
            int effectiveMoves = suspect.totalMoves - state.PressPenalty;
            if (effectiveMoves < 3) effectiveMoves = 3;
            var budgetLabel = new Label($"Бюджет расследования: {effectiveMoves} ходов");
            budgetLabel.AddToClassList("text-bold");
            budgetLabel.AddToClassList("text-cyan");
            caseCard.Add(budgetLabel);

            panel.Add(caseCard);
        }

        panel.Add(Spacer(20));

        var btn = new Button(() => {
            UIManager.Instance.PlayDayTransition("РАССЛЕДОВАНИЕ", () => {
                UIManager.Instance.HideAllPanels();
                OfficeController.Instance.OnGameStarted();
            });
        });
        btn.text = "НАЧАТЬ РАССЛЕДОВАНИЕ";
        btn.AddToClassList("btn-wide");
        panel.Add(btn);
    }

    public void OnHide() { }

    static VisualElement Spacer(int h = 15)
    {
        var s = new VisualElement();
        s.style.height = h;
        return s;
    }
}
