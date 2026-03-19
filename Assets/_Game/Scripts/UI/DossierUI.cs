using UnityEngine;
using UnityEngine.UIElements;

public class DossierUI : MonoBehaviour, IPanelController
{
    const string PanelName = "dossier-panel";

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

        panel.Add(Spacer());

        var scroll = new ScrollView(ScrollViewMode.Vertical);
        scroll.style.maxHeight = 500;
        scroll.style.flexGrow = 1;

        // Split dossier into paragraphs for individual noting
        var paragraphs = s.dossierText.Split('\n');
        foreach (var p in paragraphs)
        {
            string trimmed = p.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;

            var box = new VisualElement();
            box.AddToClassList("box");
            var label = new Label(trimmed);
            label.AddToClassList("text");
            MakeNoteable(label, trimmed, "dossier", w, notes);
            box.Add(label);
            scroll.Add(box);
        }

        panel.Add(scroll);
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
