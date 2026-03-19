using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class OutcomeUI : MonoBehaviour, IPanelController
{
    const string PanelName = "outcome-panel";

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

        var title = new Label($"НЕДЕЛЯ {state.CurrentWeek} — ПОНЕДЕЛЬНИК");
        title.AddToClassList("title");
        panel.Add(title);

        panel.Add(Spacer());

        List<string> headlines = conseq.ResolveWeek(state.CurrentWeek)
            .Where(h => !string.IsNullOrEmpty(h)).ToList();

        if (state.CurrentWeek == 1 && headlines.Count == 0)
        {
            var intro = new Label("На ваш стол легло новое дело.");
            intro.AddToClassList("header-center");
            panel.Add(intro);
        }
        else if (headlines.Count == 0)
        {
            var noNews = new Label("— Новых последствий нет —");
            noNews.AddToClassList("header-center");
            panel.Add(noNews);
        }
        else
        {
            var hdr = new Label("ПОСЛЕДСТВИЯ ВАШИХ РЕШЕНИЙ:");
            hdr.AddToClassList("header");
            panel.Add(hdr);

            var consExplain = new Label("Ваши вердикты предыдущих недель привели к этим событиям:");
            consExplain.AddToClassList("text-small");
            consExplain.AddToClassList("text-dim");
            panel.Add(consExplain);

            panel.Add(Spacer(10));

            foreach (var h in headlines)
            {
                var box = new VisualElement();
                box.AddToClassList("box");
                var lbl = new Label(h);
                lbl.AddToClassList("text");
                box.Add(lbl);
                panel.Add(box);
            }
        }

        var suspect = cases.ActiveCase;
        if (suspect != null)
        {
            panel.Add(Spacer(20));

            // Dramatic case intro card
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

            // First sentence of dossier as "accusation"
            string accusation = "";
            if (!string.IsNullOrEmpty(suspect.dossierText))
            {
                int dotIdx = suspect.dossierText.IndexOf('.');
                if (dotIdx > 0) accusation = suspect.dossierText.Substring(0, dotIdx + 1);
                else accusation = suspect.dossierText.Length > 100
                    ? suspect.dossierText.Substring(0, 100) + "..."
                    : suspect.dossierText;
            }
            if (!string.IsNullOrEmpty(accusation))
            {
                var accLabel = new Label(accusation);
                accLabel.AddToClassList("text");
                accLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
                caseCard.Add(accLabel);
            }

            panel.Add(caseCard);
        }

        panel.Add(Spacer(30));

        var btn = new Button(() => {
            UIManager.Instance.PlayDayTransition("Понедельник", () => {
                state.AdvanceDay();
                UIManager.Instance.HideAllPanels();
                OfficeController.Instance.RefreshDesk();
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
