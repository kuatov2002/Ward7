using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Confrontations now require at least one detected contradiction involving the pair.
/// On confrontation, contradictions are resolved → truth fragments revealed.
/// </summary>
public class ConfrontationUI : MonoBehaviour, IPanelController
{
    const string PanelName = "confrontation-panel";
    public static string PendingPersonA;
    public static string PendingPersonB;

    string _personA;
    string _personB;

    void Start() => UIManager.Instance.RegisterController(PanelName, this);

    public void OnShow()
    {
        _personA = PendingPersonA;
        _personB = PendingPersonB;
        PendingPersonA = PendingPersonB = null;
        BuildPanel();
    }

    void BuildPanel()
    {
        var root  = UIManager.Instance.GetRoot();
        var panel = root.Q<VisualElement>(PanelName);
        panel.Clear();

        var cases         = ServiceLocator.Get<CaseService>();
        var actions       = ServiceLocator.Get<ActionService>();
        var deduction     = ServiceLocator.Get<DeductionService>();
        var contradictions = ServiceLocator.Get<ContradictionService>();
        var c = cases.ActiveCase;

        if (c == null || string.IsNullOrEmpty(_personA))
        { UIManager.Instance.ShowPanel("command-center-panel"); return; }

        string nameA = GetPersonName(c, _personA);
        string nameB = GetPersonName(c, _personB);
        bool alreadyDone = actions.IsConfrontationDone(_personA, _personB);

        // ── Close ──
        var closeRow = new VisualElement(); closeRow.AddToClassList("close-row");
        var closeBtn = new Button(() => UIManager.Instance.ShowPanel("command-center-panel"));
        closeBtn.text = "✕"; closeBtn.AddToClassList("btn-close");
        closeRow.Add(closeBtn); panel.Add(closeRow);

        var title = new Label("ОЧНАЯ СТАВКА");
        title.AddToClassList("title"); panel.Add(title);

        var vsLabel = new Label($"{nameA}  ×  {nameB}");
        vsLabel.AddToClassList("header-center"); panel.Add(vsLabel);

        panel.Add(Spacer(12));

        // ── Find active contradictions for this pair ──
        var activeCts = contradictions
            .GetActive(c, actions)
            .Where(ct => ct.personId == _personA || ct.personId == _personB)
            .ToList();

        bool canProceed = activeCts.Count > 0 || alreadyDone;

        if (!canProceed)
        {
            // ── Blocked: no evidence yet ──
            var blockBox = new VisualElement();
            blockBox.AddToClassList("box");
            blockBox.style.borderLeftWidth = 3;
            blockBox.style.borderLeftColor = new Color(0.6f, 0.4f, 0f);

            var blockLabel = new Label("Нет оснований для очной ставки.");
            blockLabel.AddToClassList("text-bold"); blockLabel.AddToClassList("text-amber");
            blockBox.Add(blockLabel);

            var hintLabel = new Label(
                "Чтобы устроить ставку — найдите физические улики, которые противоречат показаниям кого-либо из этих людей. " +
                "Осмотрите места, сделайте запросы по базе.");
            hintLabel.AddToClassList("text"); hintLabel.style.whiteSpace = WhiteSpace.Normal;
            blockBox.Add(hintLabel);
            panel.Add(blockBox);

            var backBtn2 = new Button(() => UIManager.Instance.ShowPanel("command-center-panel"));
            backBtn2.text = "НАЗАД"; backBtn2.AddToClassList("btn-wide");
            panel.Add(backBtn2);
            return;
        }

        if (!alreadyDone)
        {
            // ── Show what contradictions will be confronted ──
            var ctHeader = new Label("РАСХОЖДЕНИЯ В ПОКАЗАНИЯХ:");
            ctHeader.AddToClassList("text-bold"); ctHeader.AddToClassList("text-amber");
            ctHeader.style.marginBottom = 6;
            panel.Add(ctHeader);

            foreach (var ct in activeCts)
            {
                var ctBox = new VisualElement();
                ctBox.AddToClassList("box");
                ctBox.style.borderLeftWidth = 3;
                ctBox.style.borderLeftColor = new Color(0.9f, 0.4f, 0.1f);

                string personName = GetPersonName(c, ct.personId);
                var ctLabel = new Label($"{personName} утверждает:");
                ctLabel.AddToClassList("text-bold"); ctBox.Add(ctLabel);

                var claimLabel = new Label($"«{ct.claimText}»");
                claimLabel.AddToClassList("text");
                ctBox.Add(claimLabel);

                // Show what evidence contradicts it
                if (ct.contradictingFragmentIds?.Count > 0)
                {
                    var evidLabel = new Label("Противоречит уликам:");
                    evidLabel.AddToClassList("text-small"); evidLabel.AddToClassList("text-dim");
                    evidLabel.style.marginTop = 4;
                    ctBox.Add(evidLabel);

                    foreach (var fid in ct.contradictingFragmentIds)
                    {
                        var frag = c.fragments?.FirstOrDefault(f => f.fragmentId == fid);
                        if (frag == null) continue;
                        var fidLabel = new Label($"  • {frag.displayText}");
                        fidLabel.AddToClassList("text-small"); fidLabel.AddToClassList("text-cyan");
                        ctBox.Add(fidLabel);
                    }
                }

                panel.Add(ctBox);
            }

            panel.Add(Spacer(12));
        }

        // ── Scripted confrontation result ──
        var conf = c.confrontations?.FirstOrDefault(x =>
            (x.personA == _personA && x.personB == _personB) ||
            (x.personA == _personB && x.personB == _personA));

        if (alreadyDone && conf != null)
        {
            // Just show result
            ShowResult(panel, c, conf, deduction, null);
        }
        else if (conf != null && activeCts.Count > 0)
        {
            // Confirm button
            var confirmBtn = new Button(() => {
                // Pay the cost
                actions.CommitAction(ActionType.Confrontation, $"{_personA}|{_personB}");
                actions.MarkConfrontationDone(_personA, _personB);
                UIManager.Instance.UpdateMovesCounter(
                    ServiceLocator.Get<GameStateService>().MovesRemaining,
                    ServiceLocator.Get<GameStateService>().PressPenalty);

                // Resolve contradictions → reveal truth fragments
                var resolved = contradictions.ResolveForConfrontation(_personA, _personB, c, actions);

                // Reveal truth text fragments (from the interrogation questions)
                foreach (var ct in resolved)
                {
                    var interr = c.interrogations?.FirstOrDefault(i => i.targetPersonId == ct.personId);
                    if (interr == null || ct.questionIndex >= interr.questions.Length) continue;
                    var q = interr.questions[ct.questionIndex];
                    // The lie's revealedFragmentId is the "false alibi" — we DON'T reveal it
                    // Instead, the confrontation's revealedFragmentId holds the TRUTH
                }

                // Reveal the confrontation fragment (the real truth)
                if (!string.IsNullOrEmpty(conf.revealedFragmentId))
                    deduction.RevealFragment(conf.revealedFragmentId);

                if (ProceduralAudio.Instance != null) ProceduralAudio.Instance.PlayStamp();
                BuildPanel();
            });

            int cost = actions.GetCost(ActionType.Confrontation);
            confirmBtn.text = $"УСТРОИТЬ СТАВКУ ({cost} хода)";
            confirmBtn.AddToClassList("btn-sign");
            confirmBtn.SetEnabled(actions.CanPerform(ActionType.Confrontation));
            panel.Add(confirmBtn);
        }
        else if (alreadyDone)
        {
            var doneLabel = new Label("Ставка уже проведена.");
            doneLabel.AddToClassList("text-dim"); panel.Add(doneLabel);
        }

        // Show result if already done
        if (alreadyDone && conf != null)
        {
            panel.Add(Spacer(10));
            ShowResult(panel, c, conf, deduction, activeCts.Count > 0 ? null : (System.Action)null);
        }

        panel.Add(Spacer(10));
        var backBtn = new Button(() => UIManager.Instance.ShowPanel("command-center-panel"));
        backBtn.text = "ВЕРНУТЬСЯ"; backBtn.AddToClassList("btn-wide");
        panel.Add(backBtn);
    }

    void ShowResult(VisualElement panel, CaseSO c, ConfrontationData conf,
                    DeductionService deduction, System.Action onReveal)
    {
        var resultBox = new VisualElement();
        resultBox.AddToClassList("box");
        resultBox.style.borderLeftWidth = 3;
        resultBox.style.borderLeftColor = new Color(1f, 0.4f, 0.2f);

        var resultLabel = new Label(conf.resultText);
        resultLabel.AddToClassList("text");
        resultLabel.style.whiteSpace = WhiteSpace.Normal;
        resultBox.Add(resultLabel);

        if (!string.IsNullOrEmpty(conf.whoBreaks))
        {
            var breakLabel = new Label($"{GetPersonName(c, conf.whoBreaks)} не выдержал.");
            breakLabel.AddToClassList("text-bold"); breakLabel.AddToClassList("text-red");
            breakLabel.style.marginTop = 6;
            resultBox.Add(breakLabel);
        }

        if (!string.IsNullOrEmpty(conf.revealedFragmentId) && deduction.IsRevealed(conf.revealedFragmentId))
        {
            var fragLabel = new Label("[+ улика получена]");
            fragLabel.AddToClassList("text-small");
            fragLabel.style.color = new Color(0.3f, 0.8f, 0.8f);
            fragLabel.style.marginTop = 6;
            resultBox.Add(fragLabel);
        }

        panel.Add(resultBox);
        UIAnimations.FadeIn(resultBox, 400);
    }

    string GetPersonName(CaseSO c, string id)
    {
        var p = c.persons?.FirstOrDefault(x => x.personId == id);
        return p != null ? p.displayName : id;
    }

    public void OnHide() { }

    static VisualElement Spacer(int h = 10)
    { var s = new VisualElement(); s.style.height = h; return s; }
}