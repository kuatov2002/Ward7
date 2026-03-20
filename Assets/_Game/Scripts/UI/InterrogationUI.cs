using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class InterrogationUI : MonoBehaviour, IPanelController
{
    const string PanelName = "interrogation-panel";
    public static string PendingTargetPersonId;

    string _targetPersonId;
    bool _actionCommitted;

    void Start()
    {
        UIManager.Instance.RegisterController(PanelName, this);
    }

    public void OnShow()
    {
        _targetPersonId = PendingTargetPersonId;
        PendingTargetPersonId = null;
        _actionCommitted = false;

        var actions = ServiceLocator.Get<ActionService>();
        if (!string.IsNullOrEmpty(_targetPersonId) && !actions.HasPerformed(ActionType.Interrogation, _targetPersonId))
        {
            actions.CommitAction(ActionType.Interrogation, _targetPersonId);
            _actionCommitted = true;

            // Update HUD
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
        var actions = ServiceLocator.Get<ActionService>();
        var deduction = ServiceLocator.Get<DeductionService>();
        var c = cases.ActiveCase;
        if (c == null || string.IsNullOrEmpty(_targetPersonId))
        {
            UIManager.Instance.ShowPanel("command-center-panel");
            return;
        }

        var interrData = c.interrogations?.FirstOrDefault(i => i.targetPersonId == _targetPersonId);
        if (interrData == null)
        {
            UIManager.Instance.ShowPanel("command-center-panel");
            return;
        }

        string personName = GetPersonName(c, _targetPersonId);

        // Close row
        var closeRow = new VisualElement();
        closeRow.AddToClassList("close-row");
        var closeBtn = new Button(() => UIManager.Instance.ShowPanel("command-center-panel"));
        closeBtn.text = "\u2715";
        closeBtn.AddToClassList("btn-close");
        closeRow.Add(closeBtn);
        panel.Add(closeRow);

        var title = new Label($"ДОПРОС: {personName}");
        title.AddToClassList("header");
        panel.Add(title);

        // Person description
        var person = c.persons?.FirstOrDefault(p => p.personId == _targetPersonId);
        if (person != null && !string.IsNullOrEmpty(person.description))
        {
            var desc = new Label(person.description);
            desc.AddToClassList("text");
            desc.style.unityFontStyleAndWeight = FontStyle.Italic;
            panel.Add(desc);
        }

        panel.Add(Spacer(10));

        var scroll = new ScrollView(ScrollViewMode.Vertical);
        scroll.style.flexGrow = 1;
        scroll.style.flexShrink = 1;

        if (interrData.questions != null)
        {
            for (int i = 0; i < interrData.questions.Length; i++)
            {
                var q = interrData.questions[i];
                bool asked = actions.IsQuestionAsked(_targetPersonId, i);

                var box = new VisualElement();
                box.AddToClassList("box");

                if (asked)
                {
                    // Show question and answer
                    var qLabel = new Label($"\u2014 {q.questionText}");
                    qLabel.AddToClassList("text-bold");
                    box.Add(qLabel);
                    box.Add(Spacer(3));

                    // Show the answer (player doesn't know if it's a lie)
                    var aLabel = new Label(q.answerText);
                    aLabel.AddToClassList("text");
                    box.Add(aLabel);

                    // If this revealed a fragment, show subtle indicator
                    if (!string.IsNullOrEmpty(q.revealedFragmentId) && deduction.IsRevealed(q.revealedFragmentId))
                    {
                        var fragLabel = new Label("[+ Фрагмент добавлен на доску]");
                        fragLabel.AddToClassList("text-small");
                        fragLabel.AddToClassList("text-cyan");
                        box.Add(fragLabel);
                    }
                }
                else
                {
                    int idx = i;
                    var btn = new Button(() => {
                        actions.MarkQuestionAsked(_targetPersonId, idx);

                        // Reveal fragment
                        if (!string.IsNullOrEmpty(q.revealedFragmentId))
                            deduction.RevealFragment(q.revealedFragmentId);

                        if (ProceduralAudio.Instance != null)
                            ProceduralAudio.Instance.PlayPaperFlip();

                        BuildPanel();
                    });
                    btn.text = q.questionText;
                    btn.AddToClassList("btn-small");
                    btn.style.whiteSpace = WhiteSpace.Normal;
                    btn.style.height = StyleKeyword.Auto;
                    btn.style.paddingTop = 6;
                    btn.style.paddingBottom = 6;
                    box.Add(btn);
                }

                scroll.Add(box);
            }
        }

        panel.Add(scroll);

        panel.Add(Spacer(10));

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
