using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class DeductionBoardUI : MonoBehaviour, IPanelController
{
    const string PanelName = "deduction-panel";

    string _selectedFragment;
    FragmentType? _selectedSlot;

    void Start()
    {
        UIManager.Instance.RegisterController(PanelName, this);
    }

    public void OnShow()
    {
        _selectedFragment = null;
        _selectedSlot = null;
        BuildPanel();
    }

    void BuildPanel()
    {
        var root = UIManager.Instance.GetRoot();
        var panel = root.Q<VisualElement>(PanelName);
        panel.Clear();

        var cases = ServiceLocator.Get<CaseService>();
        var deduction = ServiceLocator.Get<DeductionService>();
        var c = cases.ActiveCase;
        if (c == null) return;

        var closeRow = new VisualElement();
        closeRow.AddToClassList("close-row");
        var closeBtn = new Button(() => UIManager.Instance.ShowPanel("command-center-panel"));
        closeBtn.text = "\u2715";
        closeBtn.AddToClassList("btn-close");
        closeRow.Add(closeBtn);
        panel.Add(closeRow);

        var title = new Label("ДОСКА ДЕДУКЦИИ");
        title.AddToClassList("title");
        panel.Add(title);

        var instrLabel = new Label("Постройте цепочку: выберите фрагмент снизу, затем кликните слот чтобы поместить.");
        instrLabel.AddToClassList("text-small");
        instrLabel.AddToClassList("text-dim");
        instrLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        panel.Add(instrLabel);

        panel.Add(Spacer(10));

        // ─── DEDUCTION CHAIN (4 slots) ───
        var chain = new VisualElement();
        chain.AddToClassList("deduction-chain");

        AddSlot(chain, c, deduction, FragmentType.Motive, "МОТИВ");
        AddArrow(chain);
        AddSlot(chain, c, deduction, FragmentType.Opportunity, "ВОЗМОЖНОСТЬ");
        AddArrow(chain);
        AddSlot(chain, c, deduction, FragmentType.Evidence, "УЛИКА");
        AddArrow(chain);
        AddSlot(chain, c, deduction, FragmentType.Suspect, "ИСПОЛНИТЕЛЬ");

        panel.Add(chain);

        panel.Add(Spacer(10));

        // Chain status
        bool chainComplete = deduction.IsChainComplete();
        var statusLabel = new Label(chainComplete ? "ЦЕПОЧКА ЗАМКНУТА — МОЖНО ОБВИНЯТЬ" : "Заполните все 4 слота чтобы обвинить");
        statusLabel.AddToClassList("text-bold");
        statusLabel.AddToClassList(chainComplete ? "text-green" : "text-dim");
        statusLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        panel.Add(statusLabel);

        panel.Add(Spacer(15));

        // ─── FRAGMENT INVENTORY ───
        var fragHeader = new Label("НАЙДЕННЫЕ ФРАГМЕНТЫ:");
        fragHeader.AddToClassList("header");
        panel.Add(fragHeader);
        panel.Add(Spacer(5));

        var revealed = deduction.GetRevealedFragments();
        // Group by type
        var byType = new Dictionary<FragmentType, List<DeductionFragmentData>>();
        foreach (FragmentType ft in System.Enum.GetValues(typeof(FragmentType)))
            byType[ft] = new List<DeductionFragmentData>();

        if (c.fragments != null)
        {
            foreach (var frag in c.fragments)
            {
                if (revealed.Contains(frag.fragmentId))
                    byType[frag.fragmentType].Add(frag);
            }
        }

        // Get placed fragment IDs
        var placedIds = new HashSet<string>();
        foreach (FragmentType ft in System.Enum.GetValues(typeof(FragmentType)))
        {
            string placed = deduction.GetChainSlot(ft);
            if (!string.IsNullOrEmpty(placed)) placedIds.Add(placed);
        }

        var fragScroll = new ScrollView(ScrollViewMode.Vertical);
        fragScroll.style.flexGrow = 1;
        fragScroll.style.flexShrink = 1;

        string[] typeNames = { "МОТИВЫ", "ВОЗМОЖНОСТИ", "УЛИКИ", "ПОДОЗРЕВАЕМЫЕ" };
        FragmentType[] types = { FragmentType.Motive, FragmentType.Opportunity, FragmentType.Evidence, FragmentType.Suspect };

        for (int t = 0; t < types.Length; t++)
        {
            var frags = byType[types[t]];
            if (frags.Count == 0) continue;

            var typeLabel = new Label(typeNames[t]);
            typeLabel.AddToClassList("text-bold");
            typeLabel.AddToClassList("text-amber");
            typeLabel.style.marginTop = 8;
            fragScroll.Add(typeLabel);

            var grid = new VisualElement();
            grid.style.flexDirection = FlexDirection.Row;
            grid.style.flexWrap = Wrap.Wrap;

            foreach (var frag in frags)
            {
                bool isPlaced = placedIds.Contains(frag.fragmentId);
                bool isSelected = _selectedFragment == frag.fragmentId;

                var card = new Button(() => {
                    if (isPlaced) return;
                    _selectedFragment = frag.fragmentId;
                    _selectedSlot = null;
                    BuildPanel();
                });
                card.text = frag.displayText;
                card.AddToClassList("fragment-card");

                switch (frag.fragmentType)
                {
                    case FragmentType.Motive: card.AddToClassList("fragment-motive"); break;
                    case FragmentType.Opportunity: card.AddToClassList("fragment-opportunity"); break;
                    case FragmentType.Evidence: card.AddToClassList("fragment-evidence"); break;
                    case FragmentType.Suspect: card.AddToClassList("fragment-suspect"); break;
                }

                if (isSelected) card.AddToClassList("fragment-card-selected");
                if (isPlaced) card.AddToClassList("fragment-card-placed");

                grid.Add(card);
            }

            fragScroll.Add(grid);
        }

        if (revealed.Count == 0)
        {
            var emptyLabel = new Label("Пока нет найденных фрагментов. Проводите расследование чтобы находить улики.");
            emptyLabel.AddToClassList("text");
            emptyLabel.AddToClassList("text-dim");
            fragScroll.Add(emptyLabel);
        }

        panel.Add(fragScroll);

        panel.Add(Spacer(10));

        var backBtn = new Button(() => UIManager.Instance.ShowPanel("command-center-panel"));
        backBtn.text = "ВЕРНУТЬСЯ В КОМАНДНЫЙ ЦЕНТР";
        backBtn.AddToClassList("btn-wide");
        panel.Add(backBtn);
    }

    void AddSlot(VisualElement chain, CaseSO c, DeductionService deduction, FragmentType slotType, string label)
    {
        string placedId = deduction.GetChainSlot(slotType);
        bool filled = !string.IsNullOrEmpty(placedId);

        var slot = new VisualElement();
        slot.AddToClassList("deduction-slot");
        if (filled) slot.AddToClassList("deduction-slot-filled");

        var slotLabel = new Label(label);
        slotLabel.AddToClassList("deduction-slot-label");
        slot.Add(slotLabel);

        if (filled)
        {
            // Show placed fragment
            var frag = c.fragments?.FirstOrDefault(f => f.fragmentId == placedId);
            string text = frag != null ? frag.displayText : placedId;
            var contentLabel = new Label(text);
            contentLabel.AddToClassList("deduction-slot-content");
            slot.Add(contentLabel);

            // Click to remove
            slot.RegisterCallback<ClickEvent>(evt => {
                deduction.RemoveFromChain(slotType);
                if (ProceduralAudio.Instance != null)
                    ProceduralAudio.Instance.PlayPaperFlip();
                BuildPanel();
            });
        }
        else
        {
            var emptyLabel = new Label("[ пусто ]");
            emptyLabel.AddToClassList("text-small");
            emptyLabel.AddToClassList("text-dim");
            emptyLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            slot.Add(emptyLabel);

            // Click to place selected fragment
            slot.RegisterCallback<ClickEvent>(evt => {
                if (string.IsNullOrEmpty(_selectedFragment)) return;

                // Verify fragment type matches slot
                var cases = ServiceLocator.Get<CaseService>();
                var frag = cases.ActiveCase?.fragments?.FirstOrDefault(f => f.fragmentId == _selectedFragment);
                if (frag == null || frag.fragmentType != slotType)
                {
                    UIManager.Instance.ShowHint("Тип фрагмента не подходит для этого слота");
                    return;
                }

                deduction.PlaceOnChain(slotType, _selectedFragment);
                _selectedFragment = null;
                if (ProceduralAudio.Instance != null)
                    ProceduralAudio.Instance.PlayPaperFlip();
                BuildPanel();
            });
        }

        // Highlight if this slot type matches selected fragment
        if (!string.IsNullOrEmpty(_selectedFragment) && !filled)
        {
            var cases = ServiceLocator.Get<CaseService>();
            var frag = cases.ActiveCase?.fragments?.FirstOrDefault(f => f.fragmentId == _selectedFragment);
            if (frag != null && frag.fragmentType == slotType)
            {
                var green = new Color(0.3f, 1f, 0.3f);
                slot.style.borderTopColor = green;
                slot.style.borderBottomColor = green;
                slot.style.borderLeftColor = green;
                slot.style.borderRightColor = green;
                UIAnimations.Pulse(slot, 800);
            }
        }

        chain.Add(slot);
    }

    void AddArrow(VisualElement chain)
    {
        var arrow = new Label("\u2192");
        arrow.AddToClassList("deduction-arrow");
        chain.Add(arrow);
    }

    public void OnHide()
    {
        _selectedFragment = null;
        _selectedSlot = null;
    }

    static VisualElement Spacer(int h = 10)
    {
        var s = new VisualElement();
        s.style.height = h;
        return s;
    }
}
