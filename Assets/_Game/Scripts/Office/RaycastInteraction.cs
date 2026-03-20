using UnityEngine;

public class RaycastInteraction : MonoBehaviour
{
    [SerializeField] float maxDistance = 10f;
    [SerializeField] LayerMask interactLayer = ~0;

    DeskObject _current;
    bool _locked;

    public void SetLocked(bool locked)
    {
        _locked = locked;
        if (locked && _current != null)
        {
            _current.SetHighlight(false);
            _current = null;
            if (UIManager.Instance != null)
                UIManager.Instance.HideInteractHint();
        }
    }

    void Update()
    {
        if (_locked || Camera.main == null) return;

        // Raycast from center of screen (crosshair)
        var ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        DeskObject hit = null;

        if (Physics.Raycast(ray, out var info, maxDistance, interactLayer))
            hit = info.collider.GetComponentInParent<DeskObject>();

        if (hit != _current)
        {
            if (_current != null) _current.SetHighlight(false);
            _current = hit;
            if (_current != null)
            {
                _current.SetHighlight(true);
                if (UIManager.Instance != null)
                {
                    string hint = !string.IsNullOrEmpty(_current.displayName)
                        ? _current.displayName : _current.gameObject.name;
                    UIManager.Instance.ShowInteractHint(hint);
                }
            }
            else
            {
                if (UIManager.Instance != null)
                    UIManager.Instance.HideInteractHint();
            }
        }

        if (_current != null && (Input.GetMouseButtonUp(0) || Input.GetKeyDown(KeyCode.E)))
            _current.Interact();
    }
}
