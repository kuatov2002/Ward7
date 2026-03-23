using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Панель-справочник: бесплатно, без траты ходов.
/// Показывает брифинг дела, список фигурантов и сколько ходов осталось.
/// Открывается через папку дела на столе.
/// </summary>
public class CaseDossierUI : MonoBehaviour, IPanelController
{
    const string PanelName = "case-dossier-panel";

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
        var state = ServiceLocator.Get<GameStateService>();
        var actions = ServiceLocator.Get<ActionService>();
        var c = cases.ActiveCase;
        if (c == null) return;

        // Закрыть
        var closeRow = new VisualElement();
        closeRow.AddToClassList("close-row");
        var closeBtn = new Button(() => UIManager.Instance.HideAllPanels());
        closeBtn.text = "✕";
        closeBtn.AddToClassList("btn-close");
        closeRow.Add(closeBtn);
        panel.Add(closeRow);

        // Заголовок
        var title = new Label($"ДОСЬЕ: {c.displayName}");
        title.AddToClassList("title");
        panel.Add(title);

        // Ходы — крупно и сразу
        int moves = state.MovesRemaining;
        var movesBox = new VisualElement();
        movesBox.AddToClassList("box");
        movesBox.style.flexDirection = FlexDirection.Row;
        movesBox.style.justifyContent = Justify.SpaceBetween;
        movesBox.style.alignItems = Align.Center;

        var movesLabel = new Label($"Ходов осталось: {moves}");
        movesLabel.AddToClassList("text-bold");
        if (moves <= 2)
            movesLabel.style.color = new Color(0.9f, 0.2f, 0.2f);
        else if (moves <= 4)
            movesLabel.style.color = new Color(1f, 0.7f, 0f);
        movesBox.Add(movesLabel);

        // Стоимость действий — напоминание
        var costsLabel = new Label("Допрос: 2 хода  •  Осмотр: 1 ход  •  База: 1 ход  •  Ставка: 3 хода");
        costsLabel.AddToClassList("text-small");
        costsLabel.AddToClassList("text-dim");
        movesBox.Add(costsLabel);
        panel.Add(movesBox);

        panel.Add(Spacer(8));

        // Брифинг
        var briefBox = new VisualElement();
        briefBox.AddToClassList("box");
        briefBox.style.borderLeftWidth = 3;
        briefBox.style.borderLeftColor = new Color(1f, 0.7f, 0.2f);

        var briefLabel = new Label("ОБСТОЯТЕЛЬСТВА ДЕЛА");
        briefLabel.AddToClassList("text-small");
        briefLabel.AddToClassList("text-amber");
        briefLabel.style.letterSpacing = 2;
        briefBox.Add(briefLabel);

        panel.Add(Spacer(4));

        var briefText = new Label(c.briefingText ?? "");
        briefText.AddToClassList("text");
        briefBox.Add(briefText);
        panel.Add(briefBox);

        panel.Add(Spacer(10));

        // Фигуранты — с прогрессом допроса
        var persHeader = new Label("ФИГУРАНТЫ ДЕЛА");
        persHeader.AddToClassList("header");
        panel.Add(persHeader);

        panel.Add(Spacer(4));

        if (c.persons != null)
        {
            foreach (var p in c.persons)
            {
                var row = new VisualElement();
                row.AddToClassList("box");
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.paddingTop = 6;
                row.style.paddingBottom = 6;

                // Иконка роли (цветной квадрат — PSX стиль)
                var icon = new VisualElement();
                icon.style.width = 10;
                icon.style.height = 10;
                icon.style.marginRight = 10;
                icon.style.flexShrink = 0;
                icon.style.backgroundColor = p.role == PersonRole.Suspect
                    ? new Color(0.8f, 0.2f, 0.2f)
                    : new Color(0.2f, 0.6f, 0.8f);
                row.Add(icon);

                var info = new VisualElement();
                info.style.flexGrow = 1;

                var nameLabel = new Label(p.displayName);
                nameLabel.AddToClassList("text-bold");
                info.Add(nameLabel);

                var roleLabel = new Label(p.role == PersonRole.Suspect ? "подозреваемый" : "свидетель");
                roleLabel.AddToClassList("text-small");
                roleLabel.AddToClassList("text-dim");
                info.Add(roleLabel);

                if (!string.IsNullOrEmpty(p.description))
                {
                    var descLabel = new Label(p.description);
                    descLabel.AddToClassList("text-small");
                    info.Add(descLabel);
                }

                row.Add(info);

                // Статус допроса
                bool interrogated = actions.HasPerformed(ActionType.Interrogation, p.personId);
                var statusLabel = new Label(interrogated ? "✓ ДОПРОШЕН" : "не допрошен");
                statusLabel.AddToClassList("text-small");
                statusLabel.style.color = interrogated
                    ? new Color(0.3f, 0.9f, 0.3f)
                    : new Color(0.5f, 0.5f, 0.5f);
                statusLabel.style.width = 90;
                statusLabel.style.unityTextAlign = TextAnchor.MiddleRight;
                row.Add(statusLabel);

                panel.Add(row);
            }
        }

        panel.Add(Spacer(15));

        // Кнопка — перейти к действиям
        var goBtn = new Button(() => UIManager.Instance.ShowPanel("command-center-panel"));
        goBtn.text = "ПЕРЕЙТИ К РАССЛЕДОВАНИЮ →";
        goBtn.AddToClassList("btn-wide");
        goBtn.style.alignSelf = Align.Center;
        panel.Add(goBtn);
    }

    public void OnHide() { }

    static VisualElement Spacer(int h = 10)
    {
        var s = new VisualElement();
        s.style.height = h;
        return s;
    }
}
