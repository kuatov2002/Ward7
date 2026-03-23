using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class CommandCenterUI : MonoBehaviour, IPanelController
{
    const string PanelName = "command-center-panel";
    static int _activeTab = 0;

    void Start()
    {
        UIManager.Instance.RegisterController(PanelName, this);
    }

    public void OnShow() => BuildPanel();

    void BuildPanel()
    {
        var root = UIManager.Instance.GetRoot();
        var panel = root.Q<VisualElement>(PanelName);
        panel.Clear();

        var cases     = ServiceLocator.Get<CaseService>();
        var state     = ServiceLocator.Get<GameStateService>();
        var actions   = ServiceLocator.Get<ActionService>();
        var deduction = ServiceLocator.Get<DeductionService>();
        var c = cases.ActiveCase;
        if (c == null) return;

        // ── Закрыть ──
        var closeRow = new VisualElement();
        closeRow.AddToClassList("close-row");
        var closeBtn = new Button(() => UIManager.Instance.HideAllPanels());
        closeBtn.text = "✕";
        closeBtn.AddToClassList("btn-close");
        closeRow.Add(closeBtn);
        panel.Add(closeRow);

        // ── Заголовок + ходы ──
        var topRow = new VisualElement();
        topRow.style.flexDirection  = FlexDirection.Row;
        topRow.style.justifyContent = Justify.SpaceBetween;
        topRow.style.alignItems     = Align.Center;
        topRow.style.marginBottom   = 8;

        var title = new Label("РАССЛЕДОВАНИЕ");
        title.AddToClassList("header");
        topRow.Add(title);

        int moves = state.MovesRemaining;
        var movesLabel = new Label($"Ходов: {moves}");
        movesLabel.AddToClassList("text-bold");
        movesLabel.style.color = moves <= 2
            ? new Color(0.9f, 0.2f, 0.2f)
            : moves <= 4 ? new Color(1f, 0.7f, 0f) : new Color(0.5f, 0.9f, 0.5f);
        topRow.Add(movesLabel);
        panel.Add(topRow);

        // ── Прогресс цепочки ──
        bool chainComplete = deduction.IsChainComplete();
        int revealed    = deduction.GetRevealedFragments().Count;
        int totalFrags  = c.fragments?.Length ?? 0;

        var progressRow = new VisualElement();
        progressRow.AddToClassList("box");
        progressRow.style.flexDirection  = FlexDirection.Row;
        progressRow.style.justifyContent = Justify.SpaceBetween;
        progressRow.style.alignItems     = Align.Center;
        progressRow.style.paddingTop     = 6;
        progressRow.style.paddingBottom  = 6;

        var fragLabel = new Label($"Фрагментов: {revealed}/{totalFrags}");
        fragLabel.AddToClassList("text-small");
        fragLabel.AddToClassList("text-cyan");
        progressRow.Add(fragLabel);

        var chainLabel = new Label(chainComplete ? "● ЦЕПОЧКА ГОТОВА" : "○ цепочка не собрана");
        chainLabel.AddToClassList("text-small");
        chainLabel.style.color = chainComplete ? new Color(0.3f, 0.9f, 0.3f) : new Color(0.5f, 0.5f, 0.5f);
        progressRow.Add(chainLabel);
        panel.Add(progressRow);

        panel.Add(Spacer(8));

        // ── Вкладки ──
        var tabRow = new VisualElement();
        tabRow.style.flexDirection = FlexDirection.Row;
        tabRow.style.marginBottom  = 4;

        string[] tabNames = { "ЛЮДИ", "МЕСТА", "БАЗА", "СТАВКИ" };
        for (int i = 0; i < tabNames.Length; i++)
        {
            int idx = i;
            int done  = CountDone(c, actions, idx);
            int total = idx == 0 ? (c.interrogations?.Length ?? 0)
                      : idx == 1 ? (c.locations?.Length ?? 0)
                      : idx == 2 ? (c.databaseQueries?.Length ?? 0)
                                 : (c.confrontations?.Length ?? 0);

            bool isActive = _activeTab == idx;

            var tab = new Button(() => { _activeTab = idx; BuildPanel(); });
            tab.text = total > 0 ? $"{tabNames[i]}\n{done}/{total}" : tabNames[i];
            tab.style.flexGrow  = 1;
            tab.style.height    = 38;
            tab.style.fontSize  = 11;
            tab.style.unityFontStyleAndWeight = FontStyle.Bold;
            tab.style.whiteSpace = WhiteSpace.Normal;

            // border radius — стороны по отдельности
            tab.style.borderTopLeftRadius     = 0;
            tab.style.borderTopRightRadius    = 0;
            tab.style.borderBottomLeftRadius  = 0;
            tab.style.borderBottomRightRadius = 0;

            tab.style.backgroundColor = isActive
                ? new Color(0.15f, 0.35f, 0.15f)
                : new Color(0.05f, 0.1f, 0.05f);

            // border width — стороны по отдельности
            tab.style.borderTopWidth    = 1;
            tab.style.borderBottomWidth = isActive ? 2 : 1;
            tab.style.borderLeftWidth   = 1;
            tab.style.borderRightWidth  = 1;

            // border color — стороны по отдельности
            var activeColor  = new Color(0.3f, 1f, 0.3f);
            var inactiveColor = new Color(0.15f, 0.3f, 0.15f);
            tab.style.borderTopColor    = inactiveColor;
            tab.style.borderBottomColor = isActive ? activeColor : inactiveColor;
            tab.style.borderLeftColor   = inactiveColor;
            tab.style.borderRightColor  = inactiveColor;

            tab.style.color      = isActive ? new Color(0.8f, 1f, 0.8f) : new Color(0.3f, 0.6f, 0.3f);
            tab.style.marginRight = 2;
            tabRow.Add(tab);
        }
        panel.Add(tabRow);

        // ── Контент ──
        var scroll = new ScrollView(ScrollViewMode.Vertical);
        scroll.style.flexGrow  = 1;
        scroll.style.flexShrink = 1;
        scroll.style.marginTop  = 4;

        switch (_activeTab)
        {
            case 0: BuildPeopleTab(scroll, c, actions); break;
            case 1: BuildLocationsTab(scroll, c, actions); break;
            case 2: BuildDatabaseTab(scroll, c, actions); break;
            case 3: BuildConfrontationsTab(scroll, c, actions); break;
        }
        panel.Add(scroll);

        panel.Add(Spacer(10));

        // ── Нижние кнопки ──
        var bottomRow = new VisualElement();
        bottomRow.style.flexDirection  = FlexDirection.Row;
        bottomRow.style.justifyContent = Justify.Center;
        bottomRow.style.flexWrap = Wrap.Wrap;

        var deductBtn = new Button(() => UIManager.Instance.ShowPanel("deduction-panel"));
        deductBtn.text = "ДОСКА ДЕДУКЦИИ";
        deductBtn.AddToClassList("btn-wide");
        deductBtn.style.marginRight  = 8;
        deductBtn.style.marginBottom = 6;
        bottomRow.Add(deductBtn);

        var accuseBtn = new Button(() => UIManager.Instance.ShowPanel("accusation-panel"));
        accuseBtn.text = chainComplete ? "★ ОБВИНИТЬ" : "ОБВИНИТЬ";
        accuseBtn.AddToClassList("btn-sign");
        accuseBtn.SetEnabled(chainComplete);
        accuseBtn.style.marginBottom = 6;
        bottomRow.Add(accuseBtn);

        panel.Add(bottomRow);

        // ── Кнопка "Нераскрыто" — мелкая, без рамки ──
        var unsolvedRow = new VisualElement();
        unsolvedRow.style.alignItems = Align.Center;
        unsolvedRow.style.marginTop  = 4;

        var unsolvedBtn = new Button(() => ShowUnsolvedConfirm(panel, c));
        unsolvedBtn.text = "закрыть дело как нераскрытое";
        unsolvedBtn.style.fontSize        = 11;
        unsolvedBtn.style.color           = new Color(0.4f, 0.4f, 0.4f);
        unsolvedBtn.style.backgroundColor = new StyleColor(Color.clear);
        unsolvedBtn.style.borderTopWidth    = 0;
        unsolvedBtn.style.borderBottomWidth = 0;
        unsolvedBtn.style.borderLeftWidth   = 0;
        unsolvedBtn.style.borderRightWidth  = 0;
        unsolvedBtn.style.marginTop = 2;
        unsolvedRow.Add(unsolvedBtn);
        panel.Add(unsolvedRow);
    }

    // ─────────────────────────────────────────────
    // ВКЛАДКИ
    // ─────────────────────────────────────────────

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
                ? new Color(0.7f, 0.2f, 0.2f)
                : new Color(0.2f, 0.5f, 0.7f);

            container.Add(BuildCard(
                topText:      GetPersonName(c, interr.targetPersonId),
                subText:      person?.role == PersonRole.Suspect ? "подозреваемый" : "свидетель",
                doneText:     "✓ допрос завершён",
                stripColor:   strip,
                btnTextNew:   $"ДОПРОСИТЬ\n{cost} хода",
                btnTextDone:  "ПЕРЕЧИТАТЬ",
                done:         done,
                canAfford:    actions.CanPerform(ActionType.Interrogation),
                btnBgNew:     new Color(0.1f, 0.25f, 0.1f),
                btnBorderNew: new Color(0.3f, 0.7f, 0.3f),
                onClick: () => {
                    InterrogationUI.PendingTargetPersonId = interr.targetPersonId;
                    UIManager.Instance.ShowPanel("interrogation-panel");
                }
            ));
        }
    }

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
                topText:      loc.displayName,
                subText:      loc.description,
                doneText:     $"✓ зон: {zDone}/{zTotal}",
                stripColor:   new Color(0.7f, 0.6f, 0.2f),
                btnTextNew:   $"ОСМОТРЕТЬ\n{cost} ход",
                btnTextDone:  "ЕЩЁ РАЗ",
                done:         done,
                canAfford:    actions.CanPerform(ActionType.LocationInspect),
                btnBgNew:     new Color(0.15f, 0.12f, 0.05f),
                btnBorderNew: new Color(0.6f, 0.5f, 0.1f),
                onClick: () => {
                    LocationInspectUI.PendingLocationId = loc.locationId;
                    UIManager.Instance.ShowPanel("location-panel");
                }
            ));
        }
    }

    void BuildDatabaseTab(VisualElement container, CaseSO c, ActionService actions)
    {
        if (c.databaseQueries == null || c.databaseQueries.Length == 0)
        { container.Add(EmptyLabel("Запросов нет")); return; }

        int cost = actions.GetCost(ActionType.DatabaseQuery);
        foreach (var q in c.databaseQueries)
        {
            bool done = actions.IsQueryMade(q.queryId);
            container.Add(BuildCard(
                topText:      q.displayName,
                subText:      null,
                doneText:     "✓ запрос выполнен",
                stripColor:   new Color(0.2f, 0.6f, 0.7f),
                btnTextNew:   $"ПРОБИТЬ\n{cost} ход",
                btnTextDone:  "ПЕРЕЧИТАТЬ",
                done:         done,
                canAfford:    actions.CanPerform(ActionType.DatabaseQuery),
                btnBgNew:     new Color(0.05f, 0.15f, 0.2f),
                btnBorderNew: new Color(0.2f, 0.6f, 0.7f),
                onClick: () => {
                    DatabaseUI.PendingQueryId = q.queryId;
                    UIManager.Instance.ShowPanel("database-panel");
                }
            ));
        }
    }

    void BuildConfrontationsTab(VisualElement container, CaseSO c, ActionService actions)
    {
        if (c.confrontations == null || c.confrontations.Length == 0)
        { container.Add(EmptyLabel("Очных ставок нет")); return; }

        int cost = actions.GetCost(ActionType.Confrontation);

        var hint = new Label($"⚠ Стоит {cost} хода — используйте когда есть весомая улика");
        hint.AddToClassList("text-small");
        hint.AddToClassList("text-amber");
        hint.style.whiteSpace  = WhiteSpace.Normal;
        hint.style.marginBottom = 8;
        container.Add(hint);

        foreach (var conf in c.confrontations)
        {
            bool done = actions.IsConfrontationDone(conf.personA, conf.personB);
            container.Add(BuildCard(
                topText:      $"{GetPersonName(c, conf.personA)}  vs  {GetPersonName(c, conf.personB)}",
                subText:      null,
                doneText:     "✓ ставка проведена",
                stripColor:   new Color(0.7f, 0.2f, 0.5f),
                btnTextNew:   $"УСТРОИТЬ\n{cost} хода",
                btnTextDone:  "РЕЗУЛЬТАТ",
                done:         done,
                canAfford:    actions.CanPerform(ActionType.Confrontation),
                btnBgNew:     new Color(0.18f, 0.05f, 0.12f),
                btnBorderNew: new Color(0.6f, 0.2f, 0.5f),
                onClick: () => {
                    ConfrontationUI.PendingPersonA = conf.personA;
                    ConfrontationUI.PendingPersonB = conf.personB;
                    UIManager.Instance.ShowPanel("confrontation-panel");
                }
            ));
        }
    }

    // ─────────────────────────────────────────────
    // УНИВЕРСАЛЬНАЯ КАРТОЧКА
    // ─────────────────────────────────────────────

    VisualElement BuildCard(
        string topText, string subText, string doneText,
        Color stripColor,
        string btnTextNew, string btnTextDone,
        bool done, bool canAfford,
        Color btnBgNew, Color btnBorderNew,
        System.Action onClick)
    {
        var card = new VisualElement();
        card.AddToClassList("box");
        card.style.marginBottom  = 6;
        card.style.flexDirection = FlexDirection.Row;
        card.style.alignItems    = Align.Center;

        var strip = new VisualElement();
        strip.style.width           = 4;
        strip.style.alignSelf       = Align.Stretch;
        strip.style.marginRight     = 10;
        strip.style.backgroundColor = stripColor;
        card.Add(strip);

        var info = new VisualElement();
        info.style.flexGrow = 1;

        var nameLabel = new Label(topText);
        nameLabel.AddToClassList("text-bold");
        info.Add(nameLabel);

        if (!string.IsNullOrEmpty(subText))
        {
            var sub = new Label(subText);
            sub.AddToClassList("text-small");
            sub.AddToClassList("text-dim");
            sub.style.whiteSpace = WhiteSpace.Normal;
            info.Add(sub);
        }

        if (done)
        {
            var dl = new Label(doneText);
            dl.AddToClassList("text-small");
            dl.style.color = new Color(0.3f, 0.7f, 0.3f);
            info.Add(dl);
        }
        card.Add(info);

        // Кнопка
        var btn = new Button(onClick);
        btn.text = done ? btnTextDone : btnTextNew;
        btn.style.width    = 90;
        btn.style.height   = 50;
        btn.style.fontSize = 12;
        btn.style.unityFontStyleAndWeight = FontStyle.Bold;
        btn.style.whiteSpace = WhiteSpace.Normal;

        btn.style.borderTopLeftRadius     = 0;
        btn.style.borderTopRightRadius    = 0;
        btn.style.borderBottomLeftRadius  = 0;
        btn.style.borderBottomRightRadius = 0;

        btn.style.borderTopWidth    = 1;
        btn.style.borderBottomWidth = 1;
        btn.style.borderLeftWidth   = 1;
        btn.style.borderRightWidth  = 1;

        if (done)
        {
            btn.style.backgroundColor  = new Color(0.05f, 0.1f, 0.05f);
            var dc = new Color(0.15f, 0.3f, 0.15f);
            btn.style.borderTopColor    = dc;
            btn.style.borderBottomColor = dc;
            btn.style.borderLeftColor   = dc;
            btn.style.borderRightColor  = dc;
        }
        else
        {
            btn.style.backgroundColor  = btnBgNew;
            btn.style.borderTopColor    = btnBorderNew;
            btn.style.borderBottomColor = btnBorderNew;
            btn.style.borderLeftColor   = btnBorderNew;
            btn.style.borderRightColor  = btnBorderNew;
        }

        if (!done && !canAfford)
        {
            btn.SetEnabled(false);
            btn.style.opacity = 0.4f;
        }
        card.Add(btn);

        return card;
    }

    // ─────────────────────────────────────────────
    // ДИАЛОГ "НЕРАСКРЫТО"
    // ─────────────────────────────────────────────

    void ShowUnsolvedConfirm(VisualElement panel, CaseSO c)
    {
        panel.Clear();

        var box = new VisualElement();
        box.AddToClassList("box");
        box.style.alignItems    = Align.Center;
        box.style.paddingTop    = 30;
        box.style.paddingBottom = 30;
        box.style.borderLeftWidth   = 3;
        box.style.borderRightWidth  = 0;
        box.style.borderTopWidth    = 0;
        box.style.borderBottomWidth = 0;
        box.style.borderLeftColor   = new Color(0.8f, 0.2f, 0.2f);

        var warn = new Label("ЗАКРЫТЬ ДЕЛО КАК НЕРАСКРЫТОЕ?");
        warn.AddToClassList("title");
        warn.style.fontSize = 20;
        warn.style.color    = new Color(0.9f, 0.3f, 0.3f);
        box.Add(warn);
        box.Add(Spacer(10));

        var detail = new Label("Преступник останется на свободе.\nДавление прессы вырастет — в следующем деле на 1 ход меньше.");
        detail.AddToClassList("text");
        detail.style.whiteSpace      = WhiteSpace.Normal;
        detail.style.unityTextAlign  = TextAnchor.MiddleCenter;
        box.Add(detail);
        box.Add(Spacer(20));

        var row = new VisualElement();
        row.style.flexDirection  = FlexDirection.Row;
        row.style.justifyContent = Justify.Center;

        var confirmBtn = new Button(() => {
            ServiceLocator.Get<VerdictService>().CommitUnsolved(c);
            if (ProceduralAudio.Instance != null) ProceduralAudio.Instance.PlayStamp();
            UIManager.Instance.ShowPanel("case-result-panel");
        });
        confirmBtn.text = "ДА, ЗАКРЫТЬ";
        confirmBtn.style.width   = 160;
        confirmBtn.style.height  = 44;
        confirmBtn.style.fontSize = 14;
        confirmBtn.style.unityFontStyleAndWeight = FontStyle.Bold;
        confirmBtn.style.backgroundColor = new Color(0.3f, 0.05f, 0.05f);
        var redBorder = new Color(0.8f, 0.2f, 0.2f);
        confirmBtn.style.borderTopColor    = redBorder;
        confirmBtn.style.borderBottomColor = redBorder;
        confirmBtn.style.borderLeftColor   = redBorder;
        confirmBtn.style.borderRightColor  = redBorder;
        confirmBtn.style.borderTopWidth    = 2;
        confirmBtn.style.borderBottomWidth = 2;
        confirmBtn.style.borderLeftWidth   = 2;
        confirmBtn.style.borderRightWidth  = 2;
        confirmBtn.style.borderTopLeftRadius     = 0;
        confirmBtn.style.borderTopRightRadius    = 0;
        confirmBtn.style.borderBottomLeftRadius  = 0;
        confirmBtn.style.borderBottomRightRadius = 0;
        confirmBtn.style.color       = new Color(0.9f, 0.4f, 0.4f);
        confirmBtn.style.marginRight = 12;
        row.Add(confirmBtn);

        var cancelBtn = new Button(() => BuildPanel());
        cancelBtn.text = "ОТМЕНА";
        cancelBtn.AddToClassList("btn-wide");
        cancelBtn.style.width = 140;
        row.Add(cancelBtn);

        box.Add(row);
        panel.Add(box);
        UIAnimations.ScaleIn(box, 200);
    }

    // ─────────────────────────────────────────────
    // ХЕЛПЕРЫ
    // ─────────────────────────────────────────────

    int CountDone(CaseSO c, ActionService actions, int tabIdx) => tabIdx switch
    {
        0 => c.interrogations?.Count(i => actions.HasPerformed(ActionType.Interrogation, i.targetPersonId)) ?? 0,
        1 => c.locations?.Count(l => actions.HasPerformed(ActionType.LocationInspect, l.locationId)) ?? 0,
        2 => c.databaseQueries?.Count(q => actions.IsQueryMade(q.queryId)) ?? 0,
        3 => c.confrontations?.Count(f => actions.IsConfrontationDone(f.personA, f.personB)) ?? 0,
        _ => 0
    };

    string GetPersonName(CaseSO c, string personId)
    {
        var p = c.persons?.FirstOrDefault(x => x.personId == personId);
        return p != null ? p.displayName : personId;
    }

    static VisualElement EmptyLabel(string text)
    {
        var l = new Label(text);
        l.AddToClassList("text-dim");
        l.style.unityTextAlign = TextAnchor.MiddleCenter;
        l.style.marginTop = 20;
        return l;
    }

    public void OnHide() { }

    static VisualElement Spacer(int h = 10)
    {
        var s = new VisualElement(); s.style.height = h; return s;
    }
}