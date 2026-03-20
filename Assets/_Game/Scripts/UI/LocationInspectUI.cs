using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class LocationInspectUI : MonoBehaviour, IPanelController
{
    const string PanelName = "location-panel";
    public static string PendingLocationId;

    string _locationId;

    void Start()
    {
        UIManager.Instance.RegisterController(PanelName, this);
    }

    public void OnShow()
    {
        _locationId = PendingLocationId;
        PendingLocationId = null;

        var actions = ServiceLocator.Get<ActionService>();
        if (!string.IsNullOrEmpty(_locationId) && !actions.HasPerformed(ActionType.LocationInspect, _locationId))
        {
            actions.CommitAction(ActionType.LocationInspect, _locationId);
            var state = ServiceLocator.Get<GameStateService>();
            UIManager.Instance.UpdateMovesCounter(state.MovesRemaining, state.PressPenalty);
        }

        BuildPanel();
    }

    void BuildPanel()
    {
        var root = UIManager.Instance.GetRoot();
        var panel = root.Q<VisualElement>(PanelName);
        panel.Clear();

        var cases = ServiceLocator.Get<CaseService>();
        var actions = ServiceLocator.Get<ActionService>();
        var deduction = ServiceLocator.Get<DeductionService>();
        var c = cases.ActiveCase;
        if (c == null || string.IsNullOrEmpty(_locationId))
        {
            UIManager.Instance.ShowPanel("command-center-panel");
            return;
        }

        var loc = c.locations?.FirstOrDefault(l => l.locationId == _locationId);
        if (loc == null)
        {
            UIManager.Instance.ShowPanel("command-center-panel");
            return;
        }

        var closeRow = new VisualElement();
        closeRow.AddToClassList("close-row");
        var closeBtn = new Button(() => UIManager.Instance.ShowPanel("command-center-panel"));
        closeBtn.text = "\u2715";
        closeBtn.AddToClassList("btn-close");
        closeRow.Add(closeBtn);
        panel.Add(closeRow);

        var title = new Label($"ОСМОТР: {loc.displayName}");
        title.AddToClassList("header");
        panel.Add(title);

        if (!string.IsNullOrEmpty(loc.description))
        {
            var desc = new Label(loc.description);
            desc.AddToClassList("text");
            desc.style.unityFontStyleAndWeight = FontStyle.Italic;
            panel.Add(desc);
        }

        panel.Add(Spacer(10));

        var instrLabel = new Label("Выберите зону для осмотра:");
        instrLabel.AddToClassList("text-bold");
        panel.Add(instrLabel);

        panel.Add(Spacer(5));

        var grid = new VisualElement();
        grid.style.flexDirection = FlexDirection.Row;
        grid.style.flexWrap = Wrap.Wrap;
        grid.style.justifyContent = Justify.Center;

        if (loc.zones != null)
        {
            for (int i = 0; i < loc.zones.Length; i++)
            {
                var zone = loc.zones[i];
                bool inspected = actions.IsZoneInspected(_locationId, i);

                var zoneEl = new VisualElement();
                zoneEl.AddToClassList("inspect-zone");
                if (inspected) zoneEl.AddToClassList("inspect-zone-revealed");

                if (inspected)
                {
                    var nameLabel = new Label(zone.zoneName);
                    nameLabel.AddToClassList("text-bold");
                    zoneEl.Add(nameLabel);

                    var detailLabel = new Label(zone.description);
                    detailLabel.AddToClassList("text");
                    zoneEl.Add(detailLabel);

                    if (!string.IsNullOrEmpty(zone.revealedFragmentId) && deduction.IsRevealed(zone.revealedFragmentId))
                    {
                        var fragLabel = new Label("[+ Фрагмент]");
                        fragLabel.AddToClassList("text-small");
                        fragLabel.AddToClassList("text-cyan");
                        zoneEl.Add(fragLabel);
                    }
                }
                else
                {
                    int idx = i;
                    var btn = new Button(() => {
                        actions.MarkZoneInspected(_locationId, idx);
                        if (!string.IsNullOrEmpty(zone.revealedFragmentId))
                            deduction.RevealFragment(zone.revealedFragmentId);
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
        backBtn.text = "ВЕРНУТЬСЯ В КОМАНДНЫЙ ЦЕНТР";
        backBtn.AddToClassList("btn-wide");
        panel.Add(backBtn);
    }

    public void OnHide() { }

    static VisualElement Spacer(int h = 10)
    {
        var s = new VisualElement();
        s.style.height = h;
        return s;
    }
}
