using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class ConnectionBoardUI : MonoBehaviour, IPanelController
{
    const string PanelName = "connection-panel";

    string _selectedCard;
    float _savedScroll;
    readonly List<string> _foundPairs = new();
    int _attemptsUsed;

    void Start()
    {
        UIManager.Instance.RegisterController(PanelName, this);
    }

    public void OnShow()
    {
        var save = ServiceLocator.Get<SaveService>();
        _foundPairs.Clear();
        _foundPairs.AddRange(save.Data.foundConnections);
        _attemptsUsed = save.Data.connectionAttemptsUsed;
        _selectedCard = null;
        BuildPanel();
    }

    void BuildPanel()
    {
        var root = UIManager.Instance.GetRoot();
        var panel = root.Q<VisualElement>(PanelName);
        var oldScroll = panel.Q<ScrollView>();
        if (oldScroll != null) _savedScroll = oldScroll.scrollOffset.y;
        panel.Clear();

        var cases = ServiceLocator.Get<CaseService>();
        var choices = ServiceLocator.Get<DailyChoiceService>();
        var state = ServiceLocator.Get<GameStateService>();
        var s = cases.ActiveCase;
        if (s == null || s.connectionCards == null || s.connectionCards.Length == 0) return;
        int w = state.CurrentWeek;

        // Close row
        var closeRow = new VisualElement();
        closeRow.AddToClassList("close-row");
        var closeBtn = new Button(() => UIManager.Instance.HideAllPanels());
        closeBtn.text = "\u2715";
        closeBtn.AddToClassList("btn-close");
        closeRow.Add(closeBtn);
        panel.Add(closeRow);

        var title = new Label("ДОСКА СВЯЗЕЙ");
        title.AddToClassList("header");
        panel.Add(title);

        int remaining = s.maxConnectionAttempts - _attemptsUsed;
        int found = _foundPairs.Count;
        int total = s.connections != null ? s.connections.Length : 0;

        var tutorial = new Label("Кликните на одну карточку, затем на другую чтобы проверить связь. Правильная пара раскроет скрытую информацию. Количество попыток ограничено.");
        tutorial.AddToClassList("text-small");
        tutorial.AddToClassList("text-dim");
        panel.Add(tutorial);
        panel.Add(Spacer(5));

        var info = new Label($"Связей найдено: {found}/{total}  |  Попыток осталось: {remaining}");
        info.AddToClassList("text");
        info.AddToClassList("text-amber");
        panel.Add(info);

        panel.Add(Spacer(10));

        // Cards grid
        var grid = new VisualElement();
        grid.style.flexDirection = FlexDirection.Row;
        grid.style.flexWrap = Wrap.Wrap;
        grid.style.justifyContent = Justify.Center;

        foreach (var card in s.connectionCards)
        {
            // Only show discovered cards
            if (!DiscoveryHelper.IsDiscovered(card.alwaysVisible, card.requiredChoiceType,
                card.requiredChoiceId, w, choices))
                continue;

            var cardEl = new Button(() => OnCardClicked(card.cardId, s));
            cardEl.text = card.label;
            cardEl.AddToClassList("conn-card");

            // Color by type
            switch (card.type)
            {
                case ConnectionCardData.CardType.Person:
                    cardEl.AddToClassList("conn-card-person");
                    break;
                case ConnectionCardData.CardType.Item:
                    cardEl.AddToClassList("conn-card-item");
                    break;
                case ConnectionCardData.CardType.Event:
                    cardEl.AddToClassList("conn-card-event");
                    break;
            }

            if (_selectedCard == card.cardId)
                cardEl.AddToClassList("conn-card-selected");

            // Check if this card has all its connections found
            bool fullyConnected = IsFullyConnected(card.cardId, s);
            if (fullyConnected)
                cardEl.AddToClassList("conn-card-done");

            cardEl.SetEnabled(remaining > 0 || _selectedCard != null);
            grid.Add(cardEl);
        }

        panel.Add(grid);

        panel.Add(Spacer(15));

        // Found connections list
        if (_foundPairs.Count > 0)
        {
            var foundHeader = new Label("ОБНАРУЖЕННЫЕ СВЯЗИ:");
            foundHeader.AddToClassList("text-bold");
            foundHeader.AddToClassList("text-green");
            panel.Add(foundHeader);
            panel.Add(Spacer(5));

            var scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.style.maxHeight = 200;

            foreach (var pair in _foundPairs)
            {
                var parts = pair.Split('|');
                if (parts.Length != 2) continue;
                var conn = FindConnection(parts[0], parts[1], s);
                if (conn == null) continue;

                string labelA = GetCardLabel(parts[0], s);
                string labelB = GetCardLabel(parts[1], s);

                var box = new VisualElement();
                box.AddToClassList("box");
                box.style.borderLeftColor = new Color(0.3f, 0.8f, 0.3f);

                var link = new Label($"{labelA} \u2194 {labelB}");
                link.AddToClassList("text-bold");
                link.AddToClassList("text-green");
                box.Add(link);

                var reveal = new Label(conn.revealText);
                reveal.AddToClassList("text");
                box.Add(reveal);

                scroll.Add(box);
            }

            panel.Add(scroll);
            scroll.schedule.Execute(() => scroll.scrollOffset = new Vector2(0, _savedScroll));
        }
    }

    void OnCardClicked(string cardId, SuspectSO s)
    {
        if (_selectedCard == null)
        {
            _selectedCard = cardId;
            BuildPanel();
            return;
        }

        if (_selectedCard == cardId)
        {
            _selectedCard = null;
            BuildPanel();
            return;
        }

        // Try connection
        string pairKey = MakePairKey(_selectedCard, cardId);
        _selectedCard = null;

        if (_foundPairs.Contains(pairKey))
        {
            BuildPanel();
            return;
        }

        _attemptsUsed++;
        var save = ServiceLocator.Get<SaveService>();
        save.Data.connectionAttemptsUsed = _attemptsUsed;

        var conn = FindConnection(pairKey.Split('|')[0], pairKey.Split('|')[1], s);
        if (conn != null)
        {
            _foundPairs.Add(pairKey);
            save.Data.foundConnections.Add(pairKey);
            if (ProceduralAudio.Instance != null)
                ProceduralAudio.Instance.PlayPaperFlip();
        }
        else
        {
            if (ProceduralAudio.Instance != null)
                ProceduralAudio.Instance.PlayStamp();
        }

        save.Save();
        BuildPanel();
    }

    bool IsFullyConnected(string cardId, SuspectSO s)
    {
        if (s.connections == null) return false;
        foreach (var c in s.connections)
        {
            if (c.cardA == cardId || c.cardB == cardId)
            {
                string key = MakePairKey(c.cardA, c.cardB);
                if (!_foundPairs.Contains(key)) return false;
            }
        }
        return true;
    }

    static string MakePairKey(string a, string b)
    {
        return string.Compare(a, b) < 0 ? $"{a}|{b}" : $"{b}|{a}";
    }

    static ConnectionData FindConnection(string a, string b, SuspectSO s)
    {
        if (s.connections == null) return null;
        return s.connections.FirstOrDefault(c =>
            (c.cardA == a && c.cardB == b) || (c.cardA == b && c.cardB == a));
    }

    static string GetCardLabel(string cardId, SuspectSO s)
    {
        if (s.connectionCards == null) return cardId;
        var card = s.connectionCards.FirstOrDefault(c => c.cardId == cardId);
        return card != null ? card.label : cardId;
    }

    public void OnHide() { _selectedCard = null; }

    public int GetFoundCount()
    {
        return _foundPairs.Count;
    }

    static VisualElement Spacer(int h = 10)
    {
        var el = new VisualElement();
        el.style.height = h;
        return el;
    }
}
