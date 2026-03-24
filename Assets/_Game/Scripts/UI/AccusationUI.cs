using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class AccusationUI : MonoBehaviour, IPanelController
{
    const string PanelName = "accusation-panel";
 
    void Start() => UIManager.Instance.RegisterController(PanelName, this);
 
    public void OnShow() => BuildPanel();
 
    void BuildPanel()
    {
        var root  = UIManager.Instance.GetRoot();
        var panel = root.Q<VisualElement>(PanelName);
        panel.Clear();
 
        var cases     = ServiceLocator.Get<CaseService>();
        var deduction = ServiceLocator.Get<DeductionService>();
        var c = cases.ActiveCase;
        if (c == null) return;
 
        var title = new Label("ПРЕДЪЯВЛЕНИЕ ОБВИНЕНИЯ");
        title.AddToClassList("title"); panel.Add(title);
 
        panel.Add(Spacer(10));
 
        var warningLabel = new Label("Убедитесь, что все три факта действительно доказывают вину обвиняемого.");
        warningLabel.AddToClassList("text"); warningLabel.AddToClassList("text-amber");
        warningLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        warningLabel.style.whiteSpace = WhiteSpace.Normal;
        panel.Add(warningLabel);
 
        panel.Add(Spacer(15));
 
        // ── Accused person ──
        string accusedId   = deduction.GetAccused();
        string accusedName = !string.IsNullOrEmpty(accusedId) ? GetPersonName(c, accusedId) : "не выбран";
 
        var accusedBox = new VisualElement();
        accusedBox.AddToClassList("box");
        accusedBox.style.borderLeftWidth = 3;
        accusedBox.style.borderLeftColor = new Color(0.8f, 0.2f, 0.2f);
 
        var accusedHeader = new Label("ОБВИНЯЕМЫЙ:");
        accusedHeader.AddToClassList("text-small"); accusedHeader.AddToClassList("text-dim");
        accusedBox.Add(accusedHeader);
 
        var accusedLabel = new Label(accusedName);
        accusedLabel.AddToClassList("header");
        accusedLabel.style.color = new Color(1f, 0.5f, 0.5f);
        accusedBox.Add(accusedLabel);
        panel.Add(accusedBox);
 
        panel.Add(Spacer(10));
 
        // ── Selected supporting fragments ──
        var selectedFrags = deduction.GetAccusationFragments();
 
        var factsHeader = new Label("ДОКАЗАТЕЛЬСТВА:");
        factsHeader.AddToClassList("text-small"); factsHeader.AddToClassList("text-dim");
        panel.Add(factsHeader);
        panel.Add(Spacer(4));
 
        var factsBox = new VisualElement();
        factsBox.AddToClassList("box");
 
        if (selectedFrags.Count == 0)
        {
            var empty = new Label("[ не выбраны ]");
            empty.AddToClassList("text-dim"); factsBox.Add(empty);
        }
        else
        {
            foreach (var fid in selectedFrags)
            {
                var frag = c.fragments?.FirstOrDefault(f => f.fragmentId == fid);
                if (frag == null) continue;
 
                bool isPhys = ServiceLocator.Get<SaveService>().Data.physicalFragments.Contains(fid);
 
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems    = Align.Center;
                row.style.marginBottom  = 4;
 
                var typeLabel = new Label(isPhys ? "●" : "○");
                typeLabel.style.marginRight = 8;
                typeLabel.style.color = isPhys ? new Color(0.3f, 0.9f, 0.3f) : new Color(0.8f, 0.6f, 0.2f);
                typeLabel.style.fontSize = 16;
                row.Add(typeLabel);
 
                var fragLabel = new Label(frag.displayText);
                fragLabel.AddToClassList("text-bold"); fragLabel.style.flexGrow = 1;
                fragLabel.style.whiteSpace = WhiteSpace.Normal;
                row.Add(fragLabel);
 
                factsBox.Add(row);
            }
        }
 
        // Legend
        var legend = new Label("● физическая улика  ○ подтверждённые показания");
        legend.AddToClassList("text-small"); legend.AddToClassList("text-dim");
        legend.style.marginTop = 6;
        factsBox.Add(legend);
 
        panel.Add(factsBox);
 
        panel.Add(Spacer(20));
 
        bool ready = deduction.IsAccusationReady();
 
        var row2 = new VisualElement();
        row2.AddToClassList("row-center");
 
        var confirmBtn = new Button(() => {
            if (ProceduralAudio.Instance != null) ProceduralAudio.Instance.PlayStamp();
            if (ProceduralMusic.Instance != null) ProceduralMusic.Instance.SetIntensity(1f);
 
            var result   = deduction.ValidateAccusation(c);
            var verdicts = ServiceLocator.Get<VerdictService>();
            verdicts.CommitAccusation(c, result, accusedId);
 
            UIManager.Instance.PlayDayTransition("ОБВИНЕНИЕ ПРЕДЪЯВЛЕНО", () =>
                UIManager.Instance.ShowPanel("case-result-panel"));
        });
        confirmBtn.text = "ПОДТВЕРДИТЬ ОБВИНЕНИЕ";
        confirmBtn.AddToClassList("btn-sign");
        confirmBtn.SetEnabled(ready);
        row2.Add(confirmBtn);
        panel.Add(row2);
 
        panel.Add(Spacer(10));
 
        var backBtn = new Button(() => UIManager.Instance.ShowPanel("command-center-panel"));
        backBtn.text = "Назад"; backBtn.AddToClassList("btn-small");
        panel.Add(backBtn);
    }
 
    string GetPersonName(CaseSO c, string personId)
    {
        var p = c.persons?.FirstOrDefault(x => x.personId == personId);
        return p != null ? p.displayName : personId;
    }
 
    public void OnHide() { }
 
    static VisualElement Spacer(int h = 10)
    { var s = new VisualElement(); s.style.height = h; return s; }
}
