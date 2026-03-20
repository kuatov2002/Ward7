using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EvidenceBoard : MonoBehaviour
{
    public static EvidenceBoard Instance { get; private set; }

    GameObject _boardGo;
    Material _baseMat;
    readonly List<GameObject> _cards = new();

    const float BoardX = -1.8f;
    const float BoardY = 1.5f;
    const float BoardZ = 2.44f;
    const float BoardW = 1.2f;
    const float BoardH = 0.8f;

    void Awake() => Instance = this;

    public void Init(Material baseMaterial)
    {
        _baseMat = baseMaterial;
        CreateBoard();
    }

    void CreateBoard()
    {
        _boardGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _boardGo.name = "EvidenceBoard";
        _boardGo.transform.position = new Vector3(BoardX, BoardY, BoardZ);
        _boardGo.transform.localScale = new Vector3(BoardW, BoardH, 0.03f);
        SetMat(_boardGo, new Color(0.6f, 0.45f, 0.25f));
        CreateFrame();
    }

    void CreateFrame()
    {
        float halfW = BoardW / 2f;
        float halfH = BoardH / 2f;
        float frameThickness = 0.03f;

        CreateFramePiece("Top", new Vector3(BoardX, BoardY + halfH, BoardZ - 0.01f),
            new Vector3(BoardW + frameThickness * 2, frameThickness, 0.04f));
        CreateFramePiece("Bottom", new Vector3(BoardX, BoardY - halfH, BoardZ - 0.01f),
            new Vector3(BoardW + frameThickness * 2, frameThickness, 0.04f));
        CreateFramePiece("Left", new Vector3(BoardX - halfW, BoardY, BoardZ - 0.01f),
            new Vector3(frameThickness, BoardH, 0.04f));
        CreateFramePiece("Right", new Vector3(BoardX + halfW, BoardY, BoardZ - 0.01f),
            new Vector3(frameThickness, BoardH, 0.04f));
    }

    void CreateFramePiece(string name, Vector3 pos, Vector3 scale)
    {
        var piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
        piece.name = $"Frame_{name}";
        piece.transform.position = pos;
        piece.transform.localScale = scale;
        piece.transform.SetParent(_boardGo.transform);
        SetMat(piece, new Color(0.3f, 0.2f, 0.1f));
    }

    public void AddCard(string text, Color cardColor)
    {
        int idx = _cards.Count;
        int col = idx % 3;
        int row = idx / 3;

        float startX = BoardX - 0.4f;
        float startY = BoardY + 0.25f;
        float cardW = 0.22f;
        float cardH = 0.12f;
        float gap = 0.06f;

        float x = startX + col * (cardW + gap);
        float y = startY - row * (cardH + gap);

        var card = GameObject.CreatePrimitive(PrimitiveType.Cube);
        card.name = $"Card_{idx}";
        card.transform.position = new Vector3(x, y, BoardZ - 0.025f);
        card.transform.localScale = new Vector3(cardW, cardH, 0.005f);
        card.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(-3f, 3f));
        SetMat(card, cardColor);
        card.transform.SetParent(_boardGo.transform);

        var pin = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pin.name = "Pin";
        pin.transform.position = new Vector3(x, y + cardH * 0.35f, BoardZ - 0.035f);
        pin.transform.localScale = Vector3.one * 0.02f;
        pin.transform.SetParent(card.transform);
        SetMat(pin, Color.red);

        _cards.Add(card);
    }

    public void ClearCards()
    {
        foreach (var card in _cards)
            if (card != null) Destroy(card);
        _cards.Clear();
    }

    /// <summary>
    /// Refresh board from revealed deduction fragments.
    /// Colors by fragment type: red=motive, blue=opportunity, yellow=evidence, purple=suspect.
    /// </summary>
    public void RefreshFromFragments()
    {
        ClearCards();

        var deduction = ServiceLocator.Get<DeductionService>();
        var cases = ServiceLocator.Get<CaseService>();
        if (deduction == null || cases == null) return;

        var c = cases.ActiveCase;
        if (c == null || c.fragments == null) return;

        var revealed = deduction.GetRevealedFragments();

        foreach (var frag in c.fragments)
        {
            if (!revealed.Contains(frag.fragmentId)) continue;

            Color cardColor = frag.fragmentType switch
            {
                FragmentType.Motive => new Color(0.7f, 0.3f, 0.3f),
                FragmentType.Opportunity => new Color(0.3f, 0.5f, 0.7f),
                FragmentType.Evidence => new Color(0.7f, 0.6f, 0.2f),
                FragmentType.Suspect => new Color(0.6f, 0.3f, 0.7f),
                _ => new Color(0.5f, 0.5f, 0.5f)
            };

            AddCard(frag.displayText, cardColor);
        }
    }

    void SetMat(GameObject go, Color color)
    {
        var r = go.GetComponent<Renderer>();
        if (r == null) return;
        if (_baseMat != null)
        {
            var mat = new Material(_baseMat);
            mat.color = color;
            r.material = mat;
        }
        else
        {
            r.material.color = color;
        }
    }
}
