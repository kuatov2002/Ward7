using UnityEngine;
using UnityEngine.UIElements;

public class InterrogationUI : MonoBehaviour, IPanelController
{
    const string PanelName = "interrogation-panel";

    void Start()
    {
        UIManager.Instance.RegisterController(PanelName, this);
    }

    public void OnShow()
    {
        var root = UIManager.Instance.GetRoot();
        var panel = root.Q<VisualElement>(PanelName);
        panel.Clear();

        var cases = ServiceLocator.Get<CaseService>();
        var choices = ServiceLocator.Get<DailyChoiceService>();
        var state = ServiceLocator.Get<GameStateService>();
        var pressure = ServiceLocator.Get<PressureService>();
        var notes = ServiceLocator.Get<NoteService>();
        var s = cases.ActiveCase;
        if (s == null) return;
        int w = state.CurrentWeek;

        var closeRow = new VisualElement();
        closeRow.AddToClassList("close-row");
        var closeBtn = new Button(() => UIManager.Instance.HideAllPanels());
        closeBtn.text = "\u2715";
        closeBtn.AddToClassList("btn-close");
        closeRow.Add(closeBtn);
        panel.Add(closeRow);

        var title = new Label($"ДОПРОС: {s.displayName}");
        title.AddToClassList("header");
        panel.Add(title);

        // ─── PRESSURE BAR ───
        if (s.pressureThreshold > 0)
        {
            panel.Add(Spacer(5));
            var pressureRow = new VisualElement();
            pressureRow.AddToClassList("row");
            var pressLabel = new Label("Давление: ");
            pressLabel.AddToClassList("text");
            pressureRow.Add(pressLabel);

            var barBg = new VisualElement();
            barBg.style.width = 200;
            barBg.style.height = 12;
            barBg.style.backgroundColor = new Color(0.2f, 0.2f, 0.25f);
            barBg.style.borderTopLeftRadius = barBg.style.borderTopRightRadius =
                barBg.style.borderBottomLeftRadius = barBg.style.borderBottomRightRadius = 4;

            float pct = Mathf.Clamp01((float)pressure.CurrentPressure / s.pressureThreshold);
            var barFill = new VisualElement();
            barFill.style.width = Length.Percent(pct * 100f);
            barFill.style.height = 12;
            barFill.style.backgroundColor = pct < 0.6f
                ? new Color(0.3f, 0.7f, 0.3f)
                : pct < 0.85f
                    ? new Color(0.8f, 0.6f, 0.2f)
                    : new Color(0.8f, 0.2f, 0.2f);
            barFill.style.borderTopLeftRadius = barFill.style.borderTopRightRadius =
                barFill.style.borderBottomLeftRadius = barFill.style.borderBottomRightRadius = 4;
            barBg.Add(barFill);
            pressureRow.Add(barBg);
            panel.Add(pressureRow);

            var pressExplain = new Label("Чем выше давление — тем больше шанс что подозреваемый замкнётся и откажется отвечать.");
            pressExplain.AddToClassList("text-small");
            pressExplain.AddToClassList("text-dim");
            panel.Add(pressExplain);
        }

        bool shutdown = pressure.IsShutdown(s.pressureThreshold);
        bool bluffFailed = pressure.BluffFailed;

        panel.Add(Spacer());

        var scroll = new ScrollView(ScrollViewMode.Vertical);
        scroll.style.maxHeight = 500;
        scroll.style.flexGrow = 1;

        // ─── TONED QUESTIONS ───
        var stdHeader = new Label("ДОПРОС:");
        stdHeader.AddToClassList("text-bold");
        scroll.Add(stdHeader);
        scroll.Add(Spacer(5));

        var save = ServiceLocator.Get<SaveService>();

        if (s.tonedQuestions != null)
        {
            for (int i = 0; i < s.tonedQuestions.Length; i++)
            {
                var tq = s.tonedQuestions[i];
                string toneKey = $"{i}:";
                string chosenTone = null;
                foreach (var ct in save.Data.chosenTones)
                {
                    if (ct.StartsWith(toneKey))
                    {
                        chosenTone = ct.Substring(toneKey.Length);
                        break;
                    }
                }

                var box = new VisualElement();
                box.AddToClassList("box");

                if (shutdown && chosenTone == null)
                {
                    var topicLbl = new Label(tq.topic);
                    topicLbl.AddToClassList("text");
                    topicLbl.AddToClassList("text-gray");
                    box.Add(topicLbl);
                    var refused = new Label("(Подозреваемый отказывается отвечать)");
                    refused.AddToClassList("text");
                    refused.AddToClassList("text-red");
                    box.Add(refused);
                }
                else if (chosenTone != null)
                {
                    // Already answered — show result
                    string question, answer;
                    string toneClass;
                    string toneIcon;
                    switch (chosenTone)
                    {
                        case "press":
                            question = tq.questionPress; answer = tq.answerPress;
                            toneClass = "tone-press"; toneIcon = "!";
                            break;
                        case "empathy":
                            question = tq.questionEmpathy; answer = tq.answerEmpathy;
                            toneClass = "tone-empathy"; toneIcon = "\u2665";
                            break;
                        default:
                            question = tq.questionNeutral; answer = tq.answerNeutral;
                            toneClass = "tone-neutral"; toneIcon = "\u2014";
                            break;
                    }

                    box.AddToClassList(toneClass);

                    var topicLbl = new Label($"[{toneIcon}] {tq.topic}");
                    topicLbl.AddToClassList("text-small");
                    topicLbl.AddToClassList("text-dim");
                    box.Add(topicLbl);

                    var qLbl = new Label(question);
                    qLbl.AddToClassList("text-bold");
                    box.Add(qLbl);
                    box.Add(Spacer(3));

                    var aLbl = new Label(answer);
                    aLbl.AddToClassList("text");
                    MakeNoteable(aLbl, answer, "interrogation", w, notes);
                    box.Add(aLbl);
                }
                else
                {
                    // Not yet answered — show topic + 3 tone buttons
                    var topicLbl = new Label(tq.topic);
                    topicLbl.AddToClassList("text-bold");
                    topicLbl.style.fontSize = 16;
                    topicLbl.style.marginBottom = 6;
                    box.Add(topicLbl);

                    var btnRow = new VisualElement();
                    btnRow.style.flexDirection = FlexDirection.Row;
                    btnRow.style.justifyContent = Justify.SpaceBetween;

                    int idx = i;

                    var pressBtn = new Button(() => CommitTone(idx, "press", tq.pressurePress, save, pressure));
                    pressBtn.text = "! ДАВИТЬ";
                    pressBtn.AddToClassList("btn-tone");
                    pressBtn.AddToClassList("btn-tone-press");
                    btnRow.Add(pressBtn);

                    var neutralBtn = new Button(() => CommitTone(idx, "neutral", tq.pressureNeutral, save, pressure));
                    neutralBtn.text = "\u2014 НЕЙТРАЛЬНО";
                    neutralBtn.AddToClassList("btn-tone");
                    neutralBtn.AddToClassList("btn-tone-neutral");
                    btnRow.Add(neutralBtn);

                    var empathyBtn = new Button(() => CommitTone(idx, "empathy", tq.pressureEmpathy, save, pressure));
                    empathyBtn.text = "\u2665 СОЧУВСТВИЕ";
                    empathyBtn.AddToClassList("btn-tone");
                    empathyBtn.AddToClassList("btn-tone-empathy");
                    btnRow.Add(empathyBtn);

                    box.Add(btnRow);
                }

                scroll.Add(box);
            }
        }

        // ─── CONDITIONAL QUESTIONS ───
        if (s.conditionalQuestions != null && !shutdown)
        {
            bool hasAny = false;
            foreach (var cq in s.conditionalQuestions)
            {
                if (choices.GetSelected(w, cq.requiredChoiceType) == cq.requiredChoiceId)
                {
                    if (!hasAny)
                    {
                        scroll.Add(Spacer());
                        var condHeader = new Label("РАЗБЛОКИРОВАННЫЕ ВОПРОСЫ:");
                        condHeader.AddToClassList("text-bold");
                        condHeader.AddToClassList("text-yellow");
                        scroll.Add(condHeader);

                        var condExplain = new Label("Эти вопросы доступны благодаря вашим предыдущим выборам (контакт, улика, показания).");
                        condExplain.AddToClassList("text-small");
                        condExplain.AddToClassList("text-dim");
                        scroll.Add(condExplain);

                        scroll.Add(Spacer(5));
                        hasAny = true;
                    }

                    var box = new VisualElement();
                    box.AddToClassList("box");
                    var q = new Label($"\u2014 {cq.question}");
                    q.AddToClassList("text-bold");
                    box.Add(q);
                    box.Add(Spacer(3));
                    var a = new Label(cq.answer);
                    a.AddToClassList("text");
                    MakeNoteable(a, cq.answer, "interrogation_cond", w, notes);
                    box.Add(a);

                    if (cq.pressureChange != 0)
                        AddPressureTag(box, cq.pressureChange);

                    scroll.Add(box);
                }
            }
        }

        // ─── BLUFF QUESTIONS ───
        if (s.bluffQuestions != null && s.bluffQuestions.Length > 0 && !shutdown)
        {
            scroll.Add(Spacer());
            var bluffHeader = new Label("БЛЕФ (рискованный вопрос):");
            bluffHeader.AddToClassList("text-bold");
            bluffHeader.style.color = new Color(1f, 0.4f, 0.4f);
            scroll.Add(bluffHeader);

            var bluffExplain = new Label("Блеф сработает если у вас есть подтверждающая улика. Провал закроет дополнительные вопросы.");
            bluffExplain.AddToClassList("text-small");
            bluffExplain.AddToClassList("text-dim");
            scroll.Add(bluffExplain);

            scroll.Add(Spacer(5));

            foreach (var bq in s.bluffQuestions)
            {
                string bluffId = bq.question;
                bool used = choices.IsChosen(w, ChoiceType.Bluff) &&
                            choices.GetSelected(w, ChoiceType.Bluff) == bluffId;
                var box = new VisualElement();
                box.AddToClassList("box");
                box.style.borderLeftWidth = 3;
                box.style.borderLeftColor = new Color(1f, 0.3f, 0.3f);

                if (used)
                {
                    bool success = choices.GetSelected(w, bq.requiredChoiceType) == bq.requiredChoiceId;
                    var q = new Label($"\u2014 {bq.question}");
                    q.AddToClassList("text-bold");
                    q.AddToClassList(success ? "text-green" : "text-red");
                    box.Add(q);
                    box.Add(Spacer(3));
                    var a = new Label(success ? bq.answerSuccess : bq.answerFail);
                    a.AddToClassList("text");
                    box.Add(a);
                    if (!success)
                    {
                        var warn = new Label("Блеф провалился. Подозреваемый насторожился.");
                        warn.AddToClassList("text");
                        warn.AddToClassList("text-red");
                        box.Add(warn);
                    }
                }
                else if (!choices.IsChosen(w, ChoiceType.Bluff))
                {
                    var btn = new Button(() => {
                        choices.Commit(w, ChoiceType.Bluff, bluffId);
                        bool success = choices.GetSelected(w, bq.requiredChoiceType) == bq.requiredChoiceId;
                        pressure.AddPressure(bq.pressureChange);
                        if (!success) pressure.SetBluffFailed();
                        OnShow();
                    });
                    btn.text = bq.question;
                    btn.AddToClassList("btn-small");
                    btn.style.color = new Color(1f, 0.6f, 0.6f);
                    box.Add(btn);
                }
                else
                {
                    var q = new Label($"\u2014 {bq.question}");
                    q.AddToClassList("text");
                    q.AddToClassList("text-gray");
                    box.Add(q);
                }

                scroll.Add(box);
            }
        }

        // ─── FOLLOW-UP QUESTIONS ───
        if (!bluffFailed && !shutdown)
        {
            scroll.Add(Spacer(15));
            var fuHeader = new Label("ВОПРОС ВНЕ ПРОТОКОЛА (выберите один):");
            fuHeader.AddToClassList("text-bold");
            fuHeader.AddToClassList("text-accent");
            scroll.Add(fuHeader);
            scroll.Add(Spacer(5));

            bool fuDone = choices.IsChosen(w, ChoiceType.FollowUp);
            string fuSel = choices.GetSelected(w, ChoiceType.FollowUp);

            if (s.followUps != null)
            {
                foreach (var fu in s.followUps)
                {
                    bool mine = fuSel == fu.followUpId;
                    var box = new VisualElement();
                    box.AddToClassList("box");

                    if (mine)
                    {
                        var q = new Label($"\u2014 {fu.question}");
                        q.AddToClassList("text-bold");
                        q.AddToClassList("text-green");
                        box.Add(q);
                        box.Add(Spacer(3));
                        var a = new Label(fu.answer);
                        a.AddToClassList("text");
                        MakeNoteable(a, fu.answer, "followup", w, notes);
                        box.Add(a);
                    }
                    else if (!fuDone)
                    {
                        var btn = new Button(() => {
                            choices.Commit(w, ChoiceType.FollowUp, fu.followUpId);
                            OnShow();
                        });
                        btn.text = fu.question;
                        btn.AddToClassList("btn-small");
                        box.Add(btn);
                    }
                    else
                    {
                        var q = new Label($"\u2014 {fu.question}");
                        q.AddToClassList("text");
                        q.AddToClassList("text-gray");
                        box.Add(q);
                    }

                    scroll.Add(box);
                }
            }
        }
        else if (bluffFailed)
        {
            scroll.Add(Spacer(15));
            var blockedLabel = new Label("Подозреваемый замкнулся после провалившегося блефа. Дополнительные вопросы недоступны.");
            blockedLabel.AddToClassList("text");
            blockedLabel.AddToClassList("text-red");
            scroll.Add(blockedLabel);
        }

        panel.Add(scroll);
    }

    void CommitTone(int index, string tone, int pressureChange, SaveService save, PressureService pressure)
    {
        save.Data.chosenTones.Add($"{index}:{tone}");
        save.Save();
        pressure.AddPressure(pressureChange);
        if (ProceduralAudio.Instance != null)
            ProceduralAudio.Instance.PlayPaperFlip();
        OnShow();
    }

    public void OnHide() { }

    static void AddPressureTag(VisualElement box, int change)
    {
        var tag = new Label(change > 0 ? $"[+{change} давление]" : $"[{change} давление]");
        tag.AddToClassList("text-small");
        tag.style.color = change > 0 ? new Color(1f, 0.5f, 0.3f) : new Color(0.5f, 0.8f, 0.5f);
        box.Add(tag);
    }

    static void MakeNoteable(Label label, string text, string source, int week, NoteService notes)
    {
        if (notes.HasNote(week, text))
            label.AddToClassList("text-noted");

        label.RegisterCallback<ClickEvent>(evt => {
            if (notes.HasNote(week, text))
            {
                notes.RemoveNote(week, text);
                label.RemoveFromClassList("text-noted");
            }
            else
            {
                notes.AddNote(week, text, source);
                label.AddToClassList("text-noted");
                if (ProceduralAudio.Instance != null)
                    ProceduralAudio.Instance.PlayPaperFlip();
            }
            if (EvidenceBoard.Instance != null)
                EvidenceBoard.Instance.RefreshFromChoices();
        });
    }

    static VisualElement Spacer(int h = 10)
    {
        var s = new VisualElement();
        s.style.height = h;
        return s;
    }
}
