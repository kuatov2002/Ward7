using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class AccusationUI : MonoBehaviour, IPanelController
{
    const string PanelName = "accusation-panel";

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
        var deduction = ServiceLocator.Get<DeductionService>();
        var c = cases.ActiveCase;
        if (c == null) return;

        var title = new Label("ПРЕДЪЯВЛЕНИЕ ОБВИНЕНИЯ");
        title.AddToClassList("title");
        panel.Add(title);

        panel.Add(Spacer(10));

        var warningLabel = new Label("Вы собираетесь закрыть дело. Проверьте цепочку дедукции.");
        warningLabel.AddToClassList("text");
        warningLabel.AddToClassList("text-amber");
        warningLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        panel.Add(warningLabel);

        panel.Add(Spacer(15));

        // Show the chain
        var chainBox = new VisualElement();
        chainBox.AddToClassList("box");

        AddChainRow(chainBox, "МОТИВ", deduction.GetChainSlot(FragmentType.Motive), c);
        AddChainRow(chainBox, "ВОЗМОЖНОСТЬ", deduction.GetChainSlot(FragmentType.Opportunity), c);
        AddChainRow(chainBox, "УЛИКА", deduction.GetChainSlot(FragmentType.Evidence), c);
        AddChainRow(chainBox, "ИСПОЛНИТЕЛЬ", deduction.GetChainSlot(FragmentType.Suspect), c);

        panel.Add(chainBox);

        // Show who is being accused
        string accusedId = deduction.GetAccusedPersonId(c);
        string accusedName = accusedId != null ? GetPersonName(c, accusedId) : "???";

        panel.Add(Spacer(10));

        var accusedLabel = new Label($"Обвиняемый: {accusedName}");
        accusedLabel.AddToClassList("header-center");
        accusedLabel.AddToClassList("text-red");
        panel.Add(accusedLabel);

        panel.Add(Spacer(20));

        var row = new VisualElement();
        row.AddToClassList("row-center");

        var confirmBtn = new Button(() => {
            if (ProceduralAudio.Instance != null)
                ProceduralAudio.Instance.PlayStamp();
            if (ProceduralMusic.Instance != null)
                ProceduralMusic.Instance.SetIntensity(1f);

            var result = deduction.ValidateChain(c);
            var verdicts = ServiceLocator.Get<VerdictService>();
            verdicts.CommitAccusation(c, result, accusedId);

            UIManager.Instance.PlayDayTransition("ОБВИНЕНИЕ ПРЕДЪЯВЛЕНО", () => {
                UIManager.Instance.ShowPanel("case-result-panel");
            });
        });
        confirmBtn.text = "ПОДТВЕРДИТЬ ОБВИНЕНИЕ";
        confirmBtn.AddToClassList("btn-sign");
        confirmBtn.SetEnabled(deduction.IsChainComplete());
        row.Add(confirmBtn);

        panel.Add(row);

        panel.Add(Spacer(10));

        var backBtn = new Button(() => UIManager.Instance.ShowPanel("command-center-panel"));
        backBtn.text = "Назад";
        backBtn.AddToClassList("btn-small");
        panel.Add(backBtn);
    }

    void AddChainRow(VisualElement parent, string label, string fragmentId, CaseSO c)
    {
        var row = new VisualElement();
        row.AddToClassList("row");
        row.style.marginBottom = 4;

        var labelEl = new Label($"{label}: ");
        labelEl.AddToClassList("text-small");
        labelEl.AddToClassList("text-dim");
        labelEl.style.width = 120;
        row.Add(labelEl);

        string text = "[ пусто ]";
        if (!string.IsNullOrEmpty(fragmentId) && c.fragments != null)
        {
            var frag = c.fragments.FirstOrDefault(f => f.fragmentId == fragmentId);
            if (frag != null) text = frag.displayText;
        }

        var valueEl = new Label(text);
        valueEl.AddToClassList("text-bold");
        if (string.IsNullOrEmpty(fragmentId))
            valueEl.AddToClassList("text-dim");
        row.Add(valueEl);

        parent.Add(row);
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
