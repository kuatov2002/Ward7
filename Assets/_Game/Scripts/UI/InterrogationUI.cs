using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Interrogation now distinguishes TESTIMONY (may be lies) from FACTS.
/// Lie answers are shown but fragments are NOT auto-revealed.
/// Only honest answers reveal fragments immediately.
/// Lie fragments are unlocked through contradictions + confrontation.
/// </summary>
public class InterrogationUI : MonoBehaviour, IPanelController
{
    const string PanelName = "interrogation-panel";
    public static string PendingTargetPersonId;

    string _targetPersonId;

    void Start() => UIManager.Instance.RegisterController(PanelName, this);

    public void OnShow()
    {
        _targetPersonId = PendingTargetPersonId;
        PendingTargetPersonId = null;

        var actions = ServiceLocator.Get<ActionService>();
        if (!string.IsNullOrEmpty(_targetPersonId)
            && !actions.HasPerformed(ActionType.Interrogation, _targetPersonId))
        {
            actions.CommitAction(ActionType.Interrogation, _targetPersonId);
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

        var cases     = ServiceLocator.Get<CaseService>();
        var actions   = ServiceLocator.Get<ActionService>();
        var deduction = ServiceLocator.Get<DeductionService>();
        var c = cases.ActiveCase;

        if (c == null || string.IsNullOrEmpty(_targetPersonId))
        { UIManager.Instance.ShowPanel("command-center-panel"); return; }

        var interrData = c.interrogations?.FirstOrDefault(i => i.targetPersonId == _targetPersonId);
        if (interrData == null)
        { UIManager.Instance.ShowPanel("command-center-panel"); return; }

        var person = c.persons?.FirstOrDefault(p => p.personId == _targetPersonId);

        // ── Close ──
        var closeRow = new VisualElement(); closeRow.AddToClassList("close-row");
        var closeBtn = new Button(() => UIManager.Instance.ShowPanel("command-center-panel"));
        closeBtn.text = "✕"; closeBtn.AddToClassList("btn-close");
        closeRow.Add(closeBtn); panel.Add(closeRow);

        // ── Header ──
        var title = new Label($"ДОПРОС: {GetPersonName(c, _targetPersonId)}");
        title.AddToClassList("header"); panel.Add(title);

        if (person != null && !string.IsNullOrEmpty(person.description))
        {
            var desc = new Label(person.description);
            desc.AddToClassList("text");
            panel.Add(desc);
        }

        // ── Testimony note ──
        var note = new Label("Показания фигурантов требуют проверки. Ложь можно разоблачить через очную ставку.");
        note.AddToClassList("text-small"); note.AddToClassList("text-dim");
        note.style.whiteSpace = WhiteSpace.Normal; note.style.marginTop = 6; note.style.marginBottom = 10;
        panel.Add(note);

        // ── Questions ──
        var scroll = new ScrollView(ScrollViewMode.Vertical);
        scroll.style.flexGrow = 1; scroll.style.flexShrink = 1;

        if (interrData.questions != null)
        {
            for (int i = 0; i < interrData.questions.Length; i++)
            {
                var q     = interrData.questions[i];
                bool asked = actions.IsQuestionAsked(_targetPersonId, i);

                var box = new VisualElement();
                box.AddToClassList("box");
                box.style.marginBottom = 8;

                if (asked)
                {
                    // ── Question text ──
                    var qLabel = new Label($"— {q.questionText}");
                    qLabel.AddToClassList("text-bold"); box.Add(qLabel);
                    box.Add(Spacer(3));

                    // ── Answer text ──
                    var aLabel = new Label(q.answerText);
                    aLabel.AddToClassList("text"); box.Add(aLabel);

                    // ── Verification indicator ──
                    if (q.isLie)
                    {
                        // Check if this lie is already resolved
                        var save = ServiceLocator.Get<SaveService>();
                        bool resolved = save.Data.resolvedContradictions.Contains($"{_targetPersonId}:{i}");

                        if (resolved)
                        {
                            // Show truth
                            var truthBox = new VisualElement();
                            truthBox.style.flexDirection = FlexDirection.Row;
                            truthBox.style.alignItems    = Align.Center;
                            truthBox.style.marginTop     = 4;

                            var truthIcon = new Label("✓");
                            truthIcon.style.color = new Color(0.3f, 0.9f, 0.3f);
                            truthIcon.style.marginRight = 6;
                            truthBox.Add(truthIcon);

                            var truthLabel = new Label($"УСТАНОВЛЕНО: {q.truthText}");
                            truthLabel.AddToClassList("text-small");
                            truthLabel.style.color = new Color(0.3f, 0.9f, 0.3f);
                            truthLabel.style.whiteSpace = WhiteSpace.Normal;
                            truthBox.Add(truthLabel);
                            box.Add(truthBox);
                        }
                        else
                        {
                            // Suspicious marker — gives player a hint without solving for them
                            var suspRow = new VisualElement();
                            suspRow.style.flexDirection = FlexDirection.Row;
                            suspRow.style.alignItems    = Align.Center;
                            suspRow.style.marginTop     = 4;

                            var icon = new Label("⚠");
                            icon.style.color      = new Color(1f, 0.6f, 0f);
                            icon.style.marginRight = 6;
                            suspRow.Add(icon);

                            var suspect = new Label("показания не подтверждены");
                            suspect.AddToClassList("text-small");
                            suspect.style.color = new Color(0.7f, 0.4f, 0f);
                            suspRow.Add(suspect);
                            box.Add(suspRow);
                        }
                    }
                    else
                    {
                        // Honest answer → auto-reveal fragment
                        if (!string.IsNullOrEmpty(q.revealedFragmentId))
                            deduction.RevealFragment(q.revealedFragmentId);

                        if (!string.IsNullOrEmpty(q.revealedFragmentId)
                            && deduction.IsRevealed(q.revealedFragmentId))
                        {
                            var fragLabel = new Label("[+ показания занесены в базу улик]");
                            fragLabel.AddToClassList("text-small");
                            fragLabel.style.color = new Color(0.3f, 0.8f, 0.8f);
                            fragLabel.style.marginTop = 4;
                            box.Add(fragLabel);
                        }
                    }
                }
                else
                {
                    // ── Ask button ──
                    int idx = i;
                    var btn = new Button(() => {
                        actions.MarkQuestionAsked(_targetPersonId, idx);
                        if (ProceduralAudio.Instance != null) ProceduralAudio.Instance.PlayPaperFlip();
                        BuildPanel();
                    });
                    btn.text = q.questionText;
                    btn.AddToClassList("btn-small");
                    btn.style.whiteSpace   = WhiteSpace.Normal;
                    btn.style.height       = StyleKeyword.Auto;
                    btn.style.paddingTop   = 6;
                    btn.style.paddingBottom = 6;
                    box.Add(btn);
                }

                scroll.Add(box);
            }
        }

        panel.Add(scroll);
        panel.Add(Spacer(10));

        var backBtn = new Button(() => UIManager.Instance.ShowPanel("command-center-panel"));
        backBtn.text = "ВЕРНУТЬСЯ"; backBtn.AddToClassList("btn-wide");
        panel.Add(backBtn);
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