using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class CommandCenterUI : MonoBehaviour, IPanelController
{
    const string PanelName = "command-center-panel";

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

        var cases = ServiceLocator.Get<CaseService>();
        var state = ServiceLocator.Get<GameStateService>();
        var actions = ServiceLocator.Get<ActionService>();
        var deduction = ServiceLocator.Get<DeductionService>();
        var c = cases.ActiveCase;
        if (c == null) return;

        // Close row
        var closeRow = new VisualElement();
        closeRow.AddToClassList("close-row");
        var closeBtn = new Button(() => UIManager.Instance.HideAllPanels());
        closeBtn.text = "\u2715";
        closeBtn.AddToClassList("btn-close");
        closeRow.Add(closeBtn);
        panel.Add(closeRow);

        // Title
        var title = new Label($"ДЕЛО: {c.displayName}");
        title.AddToClassList("title");
        panel.Add(title);

        // Moves remaining
        var movesLabel = new Label($"ХОДОВ ОСТАЛОСЬ: {state.MovesRemaining}");
        movesLabel.AddToClassList("header-center");
        if (state.MovesRemaining <= 2)
            movesLabel.AddToClassList("text-red");
        else if (state.MovesRemaining <= 4)
            movesLabel.AddToClassList("text-yellow");
        panel.Add(movesLabel);

        if (state.PressPenalty > 0)
        {
            var penaltyLabel = new Label($"Давление прессы: -{state.PressPenalty} хода");
            penaltyLabel.AddToClassList("text-small");
            penaltyLabel.AddToClassList("text-red");
            penaltyLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            panel.Add(penaltyLabel);
        }

        panel.Add(Spacer(10));

        // Deduction progress
        int revealed = deduction.GetRevealedFragments().Count;
        int total = c.fragments != null ? c.fragments.Length : 0;
        bool chainComplete = deduction.IsChainComplete();

        var deductionInfo = new VisualElement();
        deductionInfo.AddToClassList("box");
        var deductionLabel = new Label($"ДОСКА ДЕДУКЦИИ: {revealed}/{total} фрагментов");
        deductionLabel.AddToClassList("text-bold");
        deductionLabel.AddToClassList("text-cyan");
        deductionInfo.Add(deductionLabel);

        string chainStatus = chainComplete ? "ЦЕПОЧКА ЗАМКНУТА" : "Цепочка не завершена";
        var chainLabel = new Label(chainStatus);
        chainLabel.AddToClassList("text-small");
        chainLabel.AddToClassList(chainComplete ? "text-green" : "text-dim");
        deductionInfo.Add(chainLabel);
        panel.Add(deductionInfo);

        panel.Add(Spacer(10));

        var scroll = new ScrollView(ScrollViewMode.Vertical);
        scroll.style.flexGrow = 1;
        scroll.style.flexShrink = 1;

        // Actions header
        var actHeader = new Label("ДЕЙСТВИЯ РАССЛЕДОВАНИЯ:");
        actHeader.AddToClassList("header");
        scroll.Add(actHeader);
        scroll.Add(Spacer(5));

        // Interrogation actions
        if (c.interrogations != null)
        {
            foreach (var interr in c.interrogations)
            {
                string personName = GetPersonName(c, interr.targetPersonId);
                bool done = actions.HasPerformed(ActionType.Interrogation, interr.targetPersonId);
                int cost = actions.GetCost(ActionType.Interrogation);
                bool canDo = !done && actions.CanPerform(ActionType.Interrogation);

                var btn = new Button(() => {
                    UIManager.Instance.ShowPanel("interrogation-panel");
                    // InterrogationUI will read the target from the action
                    InterrogationUI.PendingTargetPersonId = interr.targetPersonId;
                });
                btn.text = done
                    ? $"[ВЫПОЛНЕНО] Допросить: {personName}"
                    : $"Допросить: {personName}  [{cost} хода]";
                btn.AddToClassList("action-btn");
                btn.SetEnabled(canDo);
                if (done)
                {
                    btn.style.opacity = 0.5f;
                    btn.SetEnabled(false);
                }
                scroll.Add(btn);
            }
        }

        // Location actions
        if (c.locations != null)
        {
            foreach (var loc in c.locations)
            {
                bool done = actions.HasPerformed(ActionType.LocationInspect, loc.locationId);
                int cost = actions.GetCost(ActionType.LocationInspect);
                bool canDo = !done && actions.CanPerform(ActionType.LocationInspect);

                var btn = new Button(() => {
                    LocationInspectUI.PendingLocationId = loc.locationId;
                    UIManager.Instance.ShowPanel("location-panel");
                });
                btn.text = done
                    ? $"[ВЫПОЛНЕНО] Осмотреть: {loc.displayName}"
                    : $"Осмотреть: {loc.displayName}  [{cost} ход]";
                btn.AddToClassList("action-btn");
                btn.SetEnabled(canDo);
                if (done)
                {
                    btn.style.opacity = 0.5f;
                    btn.SetEnabled(false);
                }
                scroll.Add(btn);
            }
        }

        // Database actions
        if (c.databaseQueries != null)
        {
            foreach (var q in c.databaseQueries)
            {
                bool done = actions.IsQueryMade(q.queryId);
                int cost = actions.GetCost(ActionType.DatabaseQuery);
                bool canDo = !done && actions.CanPerform(ActionType.DatabaseQuery);

                var btn = new Button(() => {
                    DatabaseUI.PendingQueryId = q.queryId;
                    UIManager.Instance.ShowPanel("database-panel");
                });
                btn.text = done
                    ? $"[ВЫПОЛНЕНО] Пробить: {q.displayName}"
                    : $"Пробить по базе: {q.displayName}  [{cost} ход]";
                btn.AddToClassList("action-btn");
                btn.SetEnabled(canDo);
                if (done)
                {
                    btn.style.opacity = 0.5f;
                    btn.SetEnabled(false);
                }
                scroll.Add(btn);
            }
        }

        // Confrontation actions
        if (c.confrontations != null)
        {
            foreach (var conf in c.confrontations)
            {
                string nameA = GetPersonName(c, conf.personA);
                string nameB = GetPersonName(c, conf.personB);
                bool done = actions.IsConfrontationDone(conf.personA, conf.personB);
                int cost = actions.GetCost(ActionType.Confrontation);
                bool canDo = !done && actions.CanPerform(ActionType.Confrontation);

                var btn = new Button(() => {
                    ConfrontationUI.PendingPersonA = conf.personA;
                    ConfrontationUI.PendingPersonB = conf.personB;
                    UIManager.Instance.ShowPanel("confrontation-panel");
                });
                btn.text = done
                    ? $"[ВЫПОЛНЕНО] Очная ставка: {nameA} vs {nameB}"
                    : $"Очная ставка: {nameA} vs {nameB}  [{cost} хода]";
                btn.AddToClassList("action-btn");
                btn.SetEnabled(canDo);
                if (done)
                {
                    btn.style.opacity = 0.5f;
                    btn.SetEnabled(false);
                }
                scroll.Add(btn);
            }
        }

        panel.Add(scroll);
        panel.Add(Spacer(15));

        // Bottom action buttons
        var bottomRow = new VisualElement();
        bottomRow.AddToClassList("row-center");

        var deductionBtn = new Button(() => UIManager.Instance.ShowPanel("deduction-panel"));
        deductionBtn.text = "ДОСКА ДЕДУКЦИИ";
        deductionBtn.AddToClassList("btn-wide");
        deductionBtn.style.marginRight = 8;
        bottomRow.Add(deductionBtn);

        panel.Add(bottomRow);

        panel.Add(Spacer(8));

        var bottomRow2 = new VisualElement();
        bottomRow2.AddToClassList("row-center");

        var accuseBtn = new Button(() => UIManager.Instance.ShowPanel("accusation-panel"));
        accuseBtn.text = "ОБВИНИТЬ";
        accuseBtn.AddToClassList("btn-sign");
        accuseBtn.SetEnabled(chainComplete);
        accuseBtn.style.marginRight = 8;
        bottomRow2.Add(accuseBtn);

        var unsolvedBtn = new Button(() => {
            var verdicts = ServiceLocator.Get<VerdictService>();
            verdicts.CommitUnsolved(c);
            if (ProceduralAudio.Instance != null)
                ProceduralAudio.Instance.PlayStamp();
            UIManager.Instance.ShowPanel("case-result-panel");
        });
        unsolvedBtn.text = "ДЕЛО НЕРАСКРЫТО";
        unsolvedBtn.AddToClassList("btn-small");
        unsolvedBtn.style.color = new Color(0.6f, 0.6f, 0.6f);
        bottomRow2.Add(unsolvedBtn);

        panel.Add(bottomRow2);
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
