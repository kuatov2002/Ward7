using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class CommandCenterUI : MonoBehaviour, IPanelController
{
    const string PanelName = "command-center-panel";
    static int _activeTab = 0;

    void Start() => UIManager.Instance.RegisterController(PanelName, this);
    public void OnShow() => BuildPanel();

    void BuildPanel()
    {
        var root  = UIManager.Instance.GetRoot();
        var panel = root.Q<VisualElement>(PanelName);
        panel.Clear();

        var cases         = ServiceLocator.Get<CaseService>();
        var state         = ServiceLocator.Get<GameStateService>();
        var actions       = ServiceLocator.Get<ActionService>();
        var deduction     = ServiceLocator.Get<DeductionService>();
        var contradictions = ServiceLocator.Get<ContradictionService>();
        var c = cases.ActiveCase;
        if (c == null) return;

        // ── Close ──
        var closeRow = new VisualElement(); closeRow.AddToClassList("close-row");
        var closeBtn = new Button(() => UIManager.Instance.HideAllPanels());
        closeBtn.text = "✕"; closeBtn.AddToClassList("btn-close");
        closeRow.Add(closeBtn); panel.Add(closeRow);

        // ── Header ──
        var topRow = new VisualElement();
        topRow.style.flexDirection  = FlexDirection.Row;
        topRow.style.justifyContent = Justify.SpaceBetween;
        topRow.style.alignItems     = Align.Center;
        topRow.style.marginBottom   = 8;

        var title = new Label("РАССЛЕДОВАНИЕ"); title.AddToClassList("header"); topRow.Add(title);

        int moves = state.MovesRemaining;
        var movesLabel = new Label($"Ходов: {moves}");
        movesLabel.AddToClassList("text-bold");
        movesLabel.style.color = moves <= 2 ? new Color(0.9f, 0.2f, 0.2f)
                                : moves <= 4 ? new Color(1f, 0.7f, 0f)
                                : new Color(0.5f, 0.9f, 0.5f);
        topRow.Add(movesLabel);
        panel.Add(topRow);

        // ── Status bar ──
        var activeCts  = contradictions.GetActive(c, actions);
        var physCount  = ServiceLocator.Get<SaveService>().Data.physicalFragments.Count;
        bool accusReady = deduction.IsAccusationReady();

        var statusRow = new VisualElement();
        statusRow.AddToClassList("box");
        statusRow.style.flexDirection  = FlexDirection.Row;
        statusRow.style.justifyContent = Justify.SpaceBetween;
        statusRow.style.alignItems     = Align.Center;
        statusRow.style.paddingTop     = 5;
        statusRow.style.paddingBottom  = 5;

        var physLabel = new Label($"Физ. улик: {physCount}");
        physLabel.AddToClassList("text-small"); physLabel.style.color = new Color(0.3f, 0.8f, 0.3f);
        statusRow.Add(physLabel);

        if (activeCts.Count > 0)
        {
            var ctLabel = new Label($"⚠ {activeCts.Count} расхождение(й) в показаниях");
            ctLabel.AddToClassList("text-small"); ctLabel.style.color = new Color(1f, 0.6f, 0.1f);
            statusRow.Add(ctLabel);
        }

        var accusLabel = new Label(accusReady ? "★ ОБВИНЕНИЕ ГОТОВО" : "○ обвинение не сформировано");
        accusLabel.AddToClassList("text-small");
        accusLabel.style.color = accusReady ? new Color(0.9f, 0.8f, 0.1f) : new Color(0.4f, 0.4f, 0.4f);
        statusRow.Add(accusLabel);
        panel.Add(statusRow);
        panel.Add(Spacer(8));

        // ── Tabs ──
        // Tabs: ЛЮДИ | МЕСТА | БАЗА | ОЧНЫЕ СТАВКИ | РАСХОЖДЕНИЯ
        var tabDefs = new (string name, int idx)[]
        {
            ("ЛЮДИ",     0),
            ("МЕСТА",    1),
            ("БАЗА",     2),
            ("СТАВКИ",   3),
            ("⚠ РАСХ.",  4),
        };

        var tabRow = new VisualElement();
        tabRow.style.flexDirection = FlexDirection.Row;
        tabRow.style.marginBottom  = 4;

        foreach (var (tabName, tabIdx) in tabDefs)
        {
            bool isActive   = _activeTab == tabIdx;
            bool hasAlert   = tabIdx == 4 && activeCts.Count > 0;

            var tab = new Button(() => { _activeTab = tabIdx; BuildPanel(); });
            tab.text = hasAlert ? $"{tabName} ({activeCts.Count})" : tabName;

            tab.style.flexGrow  = 1; tab.style.height = 34; tab.style.fontSize = 11;
            tab.style.unityFontStyleAndWeight = FontStyle.Bold;
            tab.style.borderTopLeftRadius     = 0; tab.style.borderTopRightRadius    = 0;
            tab.style.borderBottomLeftRadius  = 0; tab.style.borderBottomRightRadius = 0;
            tab.style.borderTopWidth    = 1; tab.style.borderLeftWidth  = 1;
            tab.style.borderRightWidth  = 1;
            tab.style.borderBottomWidth = isActive ? 2 : 1;

            Color activeColor  = hasAlert ? new Color(1f, 0.6f, 0.1f) : new Color(0.3f, 1f, 0.3f);
            Color inactiveColor = new Color(0.15f, 0.3f, 0.15f);

            tab.style.backgroundColor = isActive ? new Color(0.12f, 0.28f, 0.12f) : new Color(0.04f, 0.08f, 0.04f);
            tab.style.borderTopColor    = inactiveColor; tab.style.borderLeftColor  = inactiveColor;
            tab.style.borderRightColor  = inactiveColor;
            tab.style.borderBottomColor = isActive ? activeColor : inactiveColor;
            tab.style.color = isActive ? (hasAlert ? new Color(1f, 0.8f, 0.4f) : new Color(0.8f, 1f, 0.8f))
                                       : (hasAlert ? new Color(0.8f, 0.5f, 0.1f) : new Color(0.3f, 0.6f, 0.3f));
            tab.style.marginRight = 2;
            tabRow.Add(tab);
        }
        panel.Add(tabRow);

        // ── Tab content ──
        var scroll = new ScrollView(ScrollViewMode.Vertical);
        scroll.style.flexGrow = 1; scroll.style.flexShrink = 1; scroll.style.marginTop = 4;

        switch (_activeTab)
        {
            case 0: BuildPeopleTab(scroll, c, actions); break;
            case 1: BuildLocationsTab(scroll, c, actions); break;
            case 2: BuildDatabaseTab(scroll, c, actions); break;
            case 3: BuildConfrontationsTab(scroll, c, actions, contradictions); break;
            case 4: BuildContradictionsTab(scroll, c, actions, contradictions); break;
        }
        panel.Add(scroll);

        panel.Add(Spacer(10));

        // ── Bottom row ──
        var bottomRow = new VisualElement();
        bottomRow.style.flexDirection  = FlexDirection.Row;
        bottomRow.style.justifyContent = Justify.Center;
        bottomRow.style.flexWrap       = Wrap.Wrap;

        var deductBtn = new Button(() => UIManager.Instance.ShowPanel("deduction-panel"));
        deductBtn.text = "ДОСКА ДЕДУКЦИИ";
        deductBtn.AddToClassList("btn-wide");
        deductBtn.style.marginRight = 8; deductBtn.style.marginBottom = 6;
        bottomRow.Add(deductBtn);

        var accuseBtn = new Button(() => UIManager.Instance.ShowPanel("accusation-panel"));
        accuseBtn.text = accusReady ? "★ ОБВИНИТЬ" : "ОБВИНИТЬ";
        accuseBtn.AddToClassList("btn-sign");
        accuseBtn.SetEnabled(accusReady);
        accuseBtn.style.marginBottom = 6;
        bottomRow.Add(accuseBtn);
        panel.Add(bottomRow);

        // ── Unsolved ──
        var unsolvedRow = new VisualElement(); unsolvedRow.style.alignItems = Align.Center;
        var unsolvedBtn = new Button(() => ShowUnsolvedConfirm(panel, c));
        unsolvedBtn.text = "закрыть дело как нераскрытое";
        unsolvedBtn.style.fontSize = 11; unsolvedBtn.style.color = new Color(0.4f, 0.4f, 0.4f);
        unsolvedBtn.style.backgroundColor = new StyleColor(Color.clear);
        unsolvedBtn.style.borderTopWidth = 0; unsolvedBtn.style.borderBottomWidth = 0;
        unsolvedBtn.style.borderLeftWidth = 0; unsolvedBtn.style.borderRightWidth = 0;
        unsolvedRow.Add(unsolvedBtn);
        panel.Add(unsolvedRow);
    }

    // ─── TAB: Contradictions (the new core tab) ───────────────────────

    void BuildContradictionsTab(VisualElement container, CaseSO c, ActionService actions,
                                 ContradictionService contradictions)
    {
        var active = contradictions.GetActive(c, actions);

        var intro = new Label(
            "Здесь отображаются расхождения между показаниями фигурантов и физическими уликами. " +
            "Используйте их для очных ставок.");
        intro.AddToClassList("text-small"); intro.AddToClassList("text-dim");
        intro.style.whiteSpace = WhiteSpace.Normal; intro.style.marginBottom = 10;
        container.Add(intro);

        if (active.Count == 0)
        {
            var none = new Label("Нет выявленных расхождений.\n\nОпросите подозреваемых и найдите физические улики — тогда система обнаружит противоречия.");
            none.AddToClassList("text-dim"); none.style.whiteSpace = WhiteSpace.Normal;
            none.style.unityTextAlign = TextAnchor.MiddleCenter; none.style.marginTop = 20;
            container.Add(none);
            return;
        }

        foreach (var ct in active)
        {
            var box = new VisualElement();
            box.AddToClassList("box");
            box.style.borderLeftWidth = 3;
            box.style.borderLeftColor = new Color(1f, 0.5f, 0.1f);
            box.style.marginBottom    = 8;

            var person = c.persons?.FirstOrDefault(p => p.personId == ct.personId);
            var nameLabel = new Label(person?.displayName ?? ct.personId);
            nameLabel.AddToClassList("text-bold"); box.Add(nameLabel);

            var claimLabel = new Label($"Утверждает: «{ct.claimText}»");
            claimLabel.AddToClassList("text");
            claimLabel.style.whiteSpace = WhiteSpace.Normal; box.Add(claimLabel);

            box.Add(Spacer(4));
            var ctHeader = new Label("Противоречит:");
            ctHeader.AddToClassList("text-small"); ctHeader.AddToClassList("text-dim");
            box.Add(ctHeader);

            foreach (var fid in ct.contradictingFragmentIds)
            {
                var frag = c.fragments?.FirstOrDefault(f => f.fragmentId == fid);
                if (frag == null) continue;
                var fl = new Label($"  • {frag.displayText}");
                fl.AddToClassList("text-small"); fl.style.color = new Color(0.3f, 0.8f, 0.3f);
                box.Add(fl);
            }

            box.Add(Spacer(6));

            // Find confrontations involving this person
            var relatedConfs = c.confrontations?
                .Where(cf => cf.personA == ct.personId || cf.personB == ct.personId).ToList();

            if (relatedConfs != null && relatedConfs.Count > 0)
            {
                var confLabel = new Label("Можно устроить очную ставку:");
                confLabel.AddToClassList("text-small"); confLabel.AddToClassList("text-amber");
                box.Add(confLabel);

                foreach (var conf in relatedConfs)
                {
                    bool done = actions.IsConfrontationDone(conf.personA, conf.personB);
                    string nameA = GetPersonName(c, conf.personA);
                    string nameB = GetPersonName(c, conf.personB);

                    var confBtn = new Button(() => {
                        ConfrontationUI.PendingPersonA = conf.personA;
                        ConfrontationUI.PendingPersonB = conf.personB;
                        UIManager.Instance.ShowPanel("confrontation-panel");
                    });
                    confBtn.text = done ? $"✓ {nameA} × {nameB}" : $"⟶ {nameA} × {nameB}";
                    confBtn.AddToClassList("btn-small");
                    confBtn.style.marginTop = 4;
                    if (done) confBtn.style.opacity = 0.5f;
                    box.Add(confBtn);
                }
            }

            container.Add(box);
        }
    }

    // ─── TAB: People ─────────────────────────────────────────────────

    void BuildPeopleTab(VisualElement container, CaseSO c, ActionService actions)
    {
        if (c.interrogations == null || c.interrogations.Length == 0)
        { container.Add(EmptyLabel("Фигурантов нет")); return; }

        int cost = actions.GetCost(ActionType.Interrogation);
        foreach (var interr in c.interrogations)
        {
            var person = c.persons?.FirstOrDefault(p => p.personId == interr.targetPersonId);
            bool done  = actions.HasPerformed(ActionType.Interrogation, interr.targetPersonId);
            Color strip = person?.role == PersonRole.Suspect
                ? new Color(0.7f, 0.2f, 0.2f) : new Color(0.2f, 0.5f, 0.7f);

            container.Add(BuildCard(
                GetPersonName(c, interr.targetPersonId),
                person?.role == PersonRole.Suspect ? "подозреваемый" : "свидетель",
                "✓ опрошен", strip,
                $"ДОПРОСИТЬ\n{cost} хода", "ПОВТОРИТЬ", done,
                actions.CanPerform(ActionType.Interrogation),
                new Color(0.1f, 0.25f, 0.1f), new Color(0.3f, 0.7f, 0.3f),
                () => {
                    InterrogationUI.PendingTargetPersonId = interr.targetPersonId;
                    UIManager.Instance.ShowPanel("interrogation-panel");
                }
            ));
        }
    }

    // ─── TAB: Locations ──────────────────────────────────────────────

    void BuildLocationsTab(VisualElement container, CaseSO c, ActionService actions)
    {
        if (c.locations == null || c.locations.Length == 0)
        { container.Add(EmptyLabel("Мест нет")); return; }

        int cost = actions.GetCost(ActionType.LocationInspect);
        foreach (var loc in c.locations)
        {
            bool done   = actions.HasPerformed(ActionType.LocationInspect, loc.locationId);
            int  zTotal = loc.zones?.Length ?? 0;
            int  zDone  = 0;
            for (int zi = 0; zi < zTotal; zi++)
                if (actions.IsZoneInspected(loc.locationId, zi)) zDone++;

            container.Add(BuildCard(
                loc.displayName, loc.description,
                $"✓ зон: {zDone}/{zTotal}", new Color(0.7f, 0.6f, 0.2f),
                $"ОСМОТРЕТЬ\n{cost} ход", "ЕЩЁ РАЗ", done,
                actions.CanPerform(ActionType.LocationInspect),
                new Color(0.15f, 0.12f, 0.05f), new Color(0.6f, 0.5f, 0.1f),
                () => {
                    LocationInspectUI.PendingLocationId = loc.locationId;
                    UIManager.Instance.ShowPanel("location-panel");
                }
            ));
        }
    }

    // ─── TAB: Database ───────────────────────────────────────────────

    void BuildDatabaseTab(VisualElement container, CaseSO c, ActionService actions)
    {
        if (c.databaseQueries == null || c.databaseQueries.Length == 0)
        { container.Add(EmptyLabel("Запросов нет")); return; }

        var hint = new Label("Запросы по базе дают физические улики — они всегда достоверны.");
        hint.AddToClassList("text-small"); hint.style.color = new Color(0.3f, 0.8f, 0.3f);
        hint.style.whiteSpace = WhiteSpace.Normal; hint.style.marginBottom = 8;
        container.Add(hint);

        int cost = actions.GetCost(ActionType.DatabaseQuery);
        foreach (var q in c.databaseQueries)
        {
            bool done = actions.IsQueryMade(q.queryId);
            container.Add(BuildCard(
                q.displayName, null, "✓ запрос выполнен", new Color(0.2f, 0.6f, 0.7f),
                $"ПРОБИТЬ\n{cost} ход", "ПЕРЕЧИТАТЬ", done,
                actions.CanPerform(ActionType.DatabaseQuery),
                new Color(0.05f, 0.15f, 0.2f), new Color(0.2f, 0.6f, 0.7f),
                () => {
                    DatabaseUI.PendingQueryId = q.queryId;
                    UIManager.Instance.ShowPanel("database-panel");
                }
            ));
        }
    }

    // ─── TAB: Confrontations ─────────────────────────────────────────

    void BuildConfrontationsTab(VisualElement container, CaseSO c, ActionService actions,
                                 ContradictionService contradictions)
    {
        if (c.confrontations == null || c.confrontations.Length == 0)
        { container.Add(EmptyLabel("Очных ставок нет")); return; }

        var hint = new Label("Очная ставка требует расхождений в показаниях. Ищите противоречия между показаниями и физическими уликами.");
        hint.AddToClassList("text-small"); hint.AddToClassList("text-amber");
        hint.style.whiteSpace = WhiteSpace.Normal; hint.style.marginBottom = 8;
        container.Add(hint);

        int cost = actions.GetCost(ActionType.Confrontation);
        var activeCts = contradictions.GetActive(c, actions);

        foreach (var conf in c.confrontations)
        {
            bool done     = actions.IsConfrontationDone(conf.personA, conf.personB);
            bool unlocked = done || activeCts.Any(ct =>
                ct.personId == conf.personA || ct.personId == conf.personB);

            string nameA = GetPersonName(c, conf.personA);
            string nameB = GetPersonName(c, conf.personB);

            var card = new VisualElement();
            card.AddToClassList("box");
            card.style.marginBottom  = 6;
            card.style.flexDirection = FlexDirection.Row;
            card.style.alignItems    = Align.Center;

            var strip = new VisualElement();
            strip.style.width           = 4;
            strip.style.alignSelf       = Align.Stretch;
            strip.style.marginRight     = 10;
            strip.style.backgroundColor = unlocked ? new Color(0.7f, 0.2f, 0.5f) : new Color(0.3f, 0.3f, 0.3f);
            card.Add(strip);

            var info = new VisualElement(); info.style.flexGrow = 1;

            var nameLabel = new Label($"{nameA}  ×  {nameB}");
            nameLabel.AddToClassList("text-bold"); info.Add(nameLabel);

            if (!unlocked)
            {
                var lockLabel = new Label("🔒 Нет оснований — найдите противоречия в показаниях");
                lockLabel.AddToClassList("text-small"); lockLabel.style.color = new Color(0.5f, 0.4f, 0.1f);
                info.Add(lockLabel);
            }
            else if (!done)
            {
                var ctCount = activeCts.Count(ct => ct.personId == conf.personA || ct.personId == conf.personB);
                var readyLabel = new Label($"✓ Найдено расхождений: {ctCount}");
                readyLabel.AddToClassList("text-small"); readyLabel.style.color = new Color(0.8f, 0.6f, 0.1f);
                info.Add(readyLabel);
            }
            else
            {
                var doneLabel = new Label("✓ ставка проведена");
                doneLabel.AddToClassList("text-small"); doneLabel.style.color = new Color(0.3f, 0.7f, 0.3f);
                info.Add(doneLabel);
            }
            card.Add(info);

            var btn = new Button(() => {
                ConfrontationUI.PendingPersonA = conf.personA;
                ConfrontationUI.PendingPersonB = conf.personB;
                UIManager.Instance.ShowPanel("confrontation-panel");
            });
            btn.text    = done ? "РЕЗУЛЬТАТ" : (unlocked ? $"УСТРОИТЬ\n{cost} хода" : "ЗАБЛОКИРОВАНО");
            btn.style.width   = 90; btn.style.height = 50; btn.style.fontSize = 12;
            btn.style.unityFontStyleAndWeight = FontStyle.Bold;
            btn.style.whiteSpace = WhiteSpace.Normal;
            btn.style.borderTopLeftRadius = 0; btn.style.borderTopRightRadius = 0;
            btn.style.borderBottomLeftRadius = 0; btn.style.borderBottomRightRadius = 0;
            btn.style.borderTopWidth = 1; btn.style.borderBottomWidth = 1;
            btn.style.borderLeftWidth = 1; btn.style.borderRightWidth = 1;

            if (unlocked && !done)
            {
                btn.style.backgroundColor = new Color(0.18f, 0.05f, 0.12f);
                var pc = new Color(0.6f, 0.2f, 0.5f);
                btn.style.borderTopColor = pc; btn.style.borderBottomColor = pc;
                btn.style.borderLeftColor = pc; btn.style.borderRightColor = pc;
                if (!actions.CanPerform(ActionType.Confrontation))
                { btn.SetEnabled(false); btn.style.opacity = 0.4f; }
            }
            else
            {
                var dc = new Color(0.15f, 0.15f, 0.15f);
                btn.style.backgroundColor = new Color(0.05f, 0.05f, 0.05f);
                btn.style.borderTopColor = dc; btn.style.borderBottomColor = dc;
                btn.style.borderLeftColor = dc; btn.style.borderRightColor = dc;
                if (!done) btn.SetEnabled(false);
            }
            card.Add(btn);
            container.Add(card);
        }
    }

    // ─── UNSOLVED confirm ─────────────────────────────────────────────

    void ShowUnsolvedConfirm(VisualElement panel, CaseSO c)
    {
        panel.Clear();
        var box = new VisualElement();
        box.AddToClassList("box"); box.style.alignItems = Align.Center;
        box.style.paddingTop = 30; box.style.paddingBottom = 30;
        box.style.borderLeftWidth = 3; box.style.borderLeftColor = new Color(0.8f, 0.2f, 0.2f);

        var warn = new Label("ЗАКРЫТЬ ДЕЛО КАК НЕРАСКРЫТОЕ?");
        warn.AddToClassList("title"); warn.style.fontSize = 20;
        warn.style.color = new Color(0.9f, 0.3f, 0.3f); box.Add(warn); box.Add(Spacer(10));

        var detail = new Label("Преступник останется на свободе.\nДавление прессы вырастет — в следующем деле на 1 ход меньше.");
        detail.AddToClassList("text"); detail.style.whiteSpace = WhiteSpace.Normal;
        detail.style.unityTextAlign = TextAnchor.MiddleCenter; box.Add(detail); box.Add(Spacer(20));

        var row = new VisualElement(); row.style.flexDirection = FlexDirection.Row;
        row.style.justifyContent = Justify.Center;

        var confirmBtn = new Button(() => {
            ServiceLocator.Get<VerdictService>().CommitUnsolved(c);
            if (ProceduralAudio.Instance != null) ProceduralAudio.Instance.PlayStamp();
            UIManager.Instance.ShowPanel("case-result-panel");
        });
        confirmBtn.text = "ДА, ЗАКРЫТЬ"; confirmBtn.style.width = 160; confirmBtn.style.height = 44;
        confirmBtn.style.backgroundColor = new Color(0.3f, 0.05f, 0.05f);
        var rb = new Color(0.8f, 0.2f, 0.2f);
        confirmBtn.style.borderTopColor = rb; confirmBtn.style.borderBottomColor = rb;
        confirmBtn.style.borderLeftColor = rb; confirmBtn.style.borderRightColor = rb;
        confirmBtn.style.borderTopWidth = 2; confirmBtn.style.borderBottomWidth = 2;
        confirmBtn.style.borderLeftWidth = 2; confirmBtn.style.borderRightWidth = 2;
        confirmBtn.style.borderTopLeftRadius = 0; confirmBtn.style.borderTopRightRadius = 0;
        confirmBtn.style.borderBottomLeftRadius = 0; confirmBtn.style.borderBottomRightRadius = 0;
        confirmBtn.style.color = new Color(0.9f, 0.4f, 0.4f); confirmBtn.style.marginRight = 12;
        row.Add(confirmBtn);

        var cancelBtn = new Button(() => BuildPanel());
        cancelBtn.text = "ОТМЕНА"; cancelBtn.AddToClassList("btn-wide"); cancelBtn.style.width = 140;
        row.Add(cancelBtn);
        box.Add(row);
        panel.Add(box);
        UIAnimations.ScaleIn(box, 200);
    }

    // ─── Helpers ──────────────────────────────────────────────────────

    VisualElement BuildCard(string topText, string subText, string doneText, Color stripColor,
        string btnTextNew, string btnTextDone, bool done, bool canAfford,
        Color btnBgNew, Color btnBorderNew, System.Action onClick)
    {
        var card = new VisualElement();
        card.AddToClassList("box"); card.style.marginBottom = 6;
        card.style.flexDirection = FlexDirection.Row; card.style.alignItems = Align.Center;

        var strip = new VisualElement();
        strip.style.width = 4; strip.style.alignSelf = Align.Stretch;
        strip.style.marginRight = 10; strip.style.backgroundColor = stripColor;
        card.Add(strip);

        var info = new VisualElement(); info.style.flexGrow = 1;
        var nameLabel = new Label(topText); nameLabel.AddToClassList("text-bold"); info.Add(nameLabel);

        if (!string.IsNullOrEmpty(subText))
        {
            var sub = new Label(subText); sub.AddToClassList("text-small"); sub.AddToClassList("text-dim");
            sub.style.whiteSpace = WhiteSpace.Normal; info.Add(sub);
        }
        if (done) { var dl = new Label(doneText); dl.AddToClassList("text-small"); dl.style.color = new Color(0.3f, 0.7f, 0.3f); info.Add(dl); }
        card.Add(info);

        var btn = new Button(onClick); btn.text = done ? btnTextDone : btnTextNew;
        btn.style.width = 90; btn.style.height = 50; btn.style.fontSize = 12;
        btn.style.unityFontStyleAndWeight = FontStyle.Bold;
        btn.style.whiteSpace = WhiteSpace.Normal;
        btn.style.borderTopLeftRadius = 0; btn.style.borderTopRightRadius = 0;
        btn.style.borderBottomLeftRadius = 0; btn.style.borderBottomRightRadius = 0;
        btn.style.borderTopWidth = 1; btn.style.borderBottomWidth = 1;
        btn.style.borderLeftWidth = 1; btn.style.borderRightWidth = 1;

        if (done) {
            btn.style.backgroundColor = new Color(0.05f, 0.1f, 0.05f);
            var dc = new Color(0.15f, 0.3f, 0.15f);
            btn.style.borderTopColor = dc; btn.style.borderBottomColor = dc;
            btn.style.borderLeftColor = dc; btn.style.borderRightColor = dc;
        } else {
            btn.style.backgroundColor = btnBgNew;
            btn.style.borderTopColor = btnBorderNew; btn.style.borderBottomColor = btnBorderNew;
            btn.style.borderLeftColor = btnBorderNew; btn.style.borderRightColor = btnBorderNew;
            if (!canAfford) { btn.SetEnabled(false); btn.style.opacity = 0.4f; }
        }
        card.Add(btn);
        return card;
    }

    string GetPersonName(CaseSO c, string id)
    { var p = c.persons?.FirstOrDefault(x => x.personId == id); return p?.displayName ?? id; }

    static VisualElement EmptyLabel(string text)
    { var l = new Label(text); l.AddToClassList("text-dim"); l.style.unityTextAlign = TextAnchor.MiddleCenter; l.style.marginTop = 20; return l; }

    static VisualElement Spacer(int h = 10) { var s = new VisualElement(); s.style.height = h; return s; }

    public void OnHide() { }
}