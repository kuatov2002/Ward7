using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class LocationInspectUI : MonoBehaviour, IPanelController
{
    const string PanelName = "location-panel";
    public static string PendingLocationId;
 
    string _locationId;
 
    void Start() => UIManager.Instance.RegisterController(PanelName, this);
 
    public void OnShow()
    {
        _locationId = PendingLocationId;
        PendingLocationId = null;
 
        var actions = ServiceLocator.Get<ActionService>();
        if (!string.IsNullOrEmpty(_locationId)
            && !actions.HasPerformed(ActionType.LocationInspect, _locationId))
        {
            actions.CommitAction(ActionType.LocationInspect, _locationId);
            UIManager.Instance.UpdateMovesCounter(
                ServiceLocator.Get<GameStateService>().MovesRemaining,
                ServiceLocator.Get<GameStateService>().PressPenalty);
        }
        BuildPanel();
    }
 
    void BuildPanel()
    {
        var root  = UIManager.Instance.GetRoot();
        var panel = root.Q<VisualElement>(PanelName);
        panel.Clear();
 
        var cases         = ServiceLocator.Get<CaseService>();
        var actions       = ServiceLocator.Get<ActionService>();
        var deduction     = ServiceLocator.Get<DeductionService>();
        var contradictions = ServiceLocator.Get<ContradictionService>();
        var c = cases.ActiveCase;
 
        if (c == null || string.IsNullOrEmpty(_locationId))
        { UIManager.Instance.ShowPanel("command-center-panel"); return; }
 
        var loc = c.locations?.FirstOrDefault(l => l.locationId == _locationId);
        if (loc == null)
        { UIManager.Instance.ShowPanel("command-center-panel"); return; }
 
        var closeRow = new VisualElement(); closeRow.AddToClassList("close-row");
        var closeBtn = new Button(() => UIManager.Instance.ShowPanel("command-center-panel"));
        closeBtn.text = "✕"; closeBtn.AddToClassList("btn-close");
        closeRow.Add(closeBtn); panel.Add(closeRow);
 
        var title = new Label($"ОСМОТР: {loc.displayName}");
        title.AddToClassList("header"); panel.Add(title);
 
        if (!string.IsNullOrEmpty(loc.description))
        {
            var desc = new Label(loc.description);
            desc.AddToClassList("text");
            panel.Add(desc);
        }
 
        panel.Add(Spacer(10));
        panel.Add(new Label("Выберите зону для осмотра:") { name = "_instrL" });
 
        var grid = new VisualElement();
        grid.style.flexDirection  = FlexDirection.Row;
        grid.style.flexWrap       = Wrap.Wrap;
        grid.style.justifyContent = Justify.Center;
 
        if (loc.zones != null)
        {
            for (int i = 0; i < loc.zones.Length; i++)
            {
                var zone      = loc.zones[i];
                bool inspected = actions.IsZoneInspected(_locationId, i);
 
                var zoneEl = new VisualElement();
                zoneEl.AddToClassList("inspect-zone");
                if (inspected) zoneEl.AddToClassList("inspect-zone-revealed");
 
                if (inspected)
                {
                    var nameLabel = new Label(zone.zoneName);
                    nameLabel.AddToClassList("text-bold"); zoneEl.Add(nameLabel);
 
                    var detailLabel = new Label(zone.description);
                    detailLabel.AddToClassList("text"); zoneEl.Add(detailLabel);
 
                    if (!string.IsNullOrEmpty(zone.revealedFragmentId)
                        && deduction.IsRevealed(zone.revealedFragmentId))
                    {
                        var fragLabel = new Label("[физическая улика занесена]");
                        fragLabel.AddToClassList("text-small");
                        fragLabel.style.color = new Color(0.3f, 0.8f, 0.3f);
                        zoneEl.Add(fragLabel);
                    }
                }
                else
                {
                    int idx = i;
                    var btn = new Button(() => {
                        actions.MarkZoneInspected(_locationId, idx);
 
                        if (!string.IsNullOrEmpty(zone.revealedFragmentId))
                        {
                            // Physical evidence — always trustworthy
                            deduction.RevealFragment(zone.revealedFragmentId);
                            contradictions.RegisterPhysicalFragment(zone.revealedFragmentId);
                        }
                        if (ProceduralAudio.Instance != null)
                            ProceduralAudio.Instance.PlayPaperFlip();
                        BuildPanel();
                    });
                    btn.text = zone.zoneName;
                    btn.AddToClassList("inspect-zone-btn");
                    zoneEl.Add(btn);
                }
 
                grid.Add(zoneEl);
            }
        }
 
        panel.Add(grid);
        panel.Add(Spacer(15));
 
        var backBtn = new Button(() => UIManager.Instance.ShowPanel("command-center-panel"));
        backBtn.text = "ВЕРНУТЬСЯ"; backBtn.AddToClassList("btn-wide");
        panel.Add(backBtn);
    }
 
    public void OnHide() { }
    static VisualElement Spacer(int h = 10) { var s = new VisualElement(); s.style.height = h; return s; }
}
