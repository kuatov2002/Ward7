using UnityEngine;
using UnityEngine.UIElements;

public class DossierUI : MonoBehaviour, IPanelController
{
    const string PanelName = "dossier-panel";
    int _activeTab;

    void Start()
    {
        UIManager.Instance.RegisterController(PanelName, this);
    }

    public void OnShow()
    {
        _activeTab = 0;
        Build();
    }

    void Build()
    {
        var root = UIManager.Instance.GetRoot();
        var panel = root.Q<VisualElement>(PanelName);
        panel.Clear();

        var cases = ServiceLocator.Get<CaseService>();
        var notes = ServiceLocator.Get<NoteService>();
        var state = ServiceLocator.Get<GameStateService>();
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

        var title = new Label($"ДОСЬЕ: {s.displayName}");
        title.AddToClassList("header");
        panel.Add(title);

        var hint = new Label("(Кликните по тексту чтобы сделать заметку)");
        hint.AddToClassList("text-small");
        hint.AddToClassList("text-gray");
        panel.Add(hint);

        panel.Add(Spacer(5));

        // ─── TAB BAR ───
        var paragraphs = s.dossierText.Split('\n');
        // Split into sections: first paragraph = summary, rest = details
        // Group by empty lines or by fixed sections
        var sections = SplitIntoSections(paragraphs);

        string[] tabNames = GetTabNames(sections.Length);

        var tabBar = new VisualElement();
        tabBar.style.flexDirection = FlexDirection.Row;
        tabBar.style.marginBottom = 8;

        for (int i = 0; i < tabNames.Length; i++)
        {
            int tabIdx = i;
            var tabBtn = new Button(() => { _activeTab = tabIdx; Build(); });
            tabBtn.text = tabNames[i];
            tabBtn.AddToClassList("btn-small");

            if (i == _activeTab)
            {
                tabBtn.style.backgroundColor = new Color(0.15f, 0.3f, 0.15f);
                tabBtn.style.borderBottomWidth = 2;
                tabBtn.style.borderBottomColor = new Color(1f, 0.7f, 0f);
                tabBtn.style.color = new Color(1f, 0.7f, 0f);
            }
            else
            {
                tabBtn.style.opacity = 0.6f;
            }

            tabBtn.style.marginRight = 4;
            tabBar.Add(tabBtn);
        }
        panel.Add(tabBar);

        // ─── TAB CONTENT ───
        var scroll = new ScrollView(ScrollViewMode.Vertical);
        scroll.style.maxHeight = 500;
        scroll.style.flexGrow = 1;

        if (_activeTab < sections.Length)
        {
            var section = sections[_activeTab];
            for (int i = 0; i < section.Length; i++)
            {
                string trimmed = section[i].Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                var box = new VisualElement();
                box.AddToClassList("box");
                var label = new Label(trimmed);
                label.AddToClassList("text");
                MakeNoteable(label, trimmed, "dossier", w, notes);
                box.Add(label);
                scroll.Add(box);

                // Staggered fade-in for each paragraph
                UIAnimations.SlideInLeft(box, 150 + i * 60);
            }
        }

        panel.Add(scroll);
    }

    /// <summary>
    /// Split paragraphs into sections. First 2 paragraphs = summary.
    /// Next group = background. Rest = details.
    /// </summary>
    string[][] SplitIntoSections(string[] paragraphs)
    {
        // Filter out empty lines
        var nonEmpty = new System.Collections.Generic.List<string>();
        foreach (var p in paragraphs)
        {
            string t = p.Trim();
            if (!string.IsNullOrEmpty(t)) nonEmpty.Add(t);
        }

        if (nonEmpty.Count <= 3)
            return new[] { nonEmpty.ToArray() };

        // Split into roughly equal sections (2-3 tabs max)
        int perSection = Mathf.CeilToInt(nonEmpty.Count / 3f);
        var sections = new System.Collections.Generic.List<string[]>();

        for (int i = 0; i < nonEmpty.Count; i += perSection)
        {
            int count = Mathf.Min(perSection, nonEmpty.Count - i);
            var section = new string[count];
            nonEmpty.CopyTo(i, section, 0, count);
            sections.Add(section);
        }

        return sections.ToArray();
    }

    string[] GetTabNames(int count)
    {
        switch (count)
        {
            case 1: return new[] { "ПОЛНОЕ ДОСЬЕ" };
            case 2: return new[] { "БИОГРАФИЯ", "ДЕТАЛИ ДЕЛА" };
            default: return new[] { "БИОГРАФИЯ", "СВЯЗИ", "ДЕТАЛИ ДЕЛА" };
        }
    }

    public void OnHide() { }

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
