using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerSelection : NetworkBehaviour
{
    private Player _player;
    private Camera _playerCamera;
    private InputAction _leftClickAction;

    private Vector2 _selectionStart;
    private Vector2 _selectionEnd;
    private RectTransform _selectionBox;
    private bool _isDragging = false;
    private bool _isClickProcessed = false;

    private void Start()
    {
        _leftClickAction = InputSystem.actions.FindAction("LeftClick");
        _player = GetComponent<Player>();
        if (IsOwner)
        {
            _playerCamera = GetComponentInChildren<Camera>();

            if (_playerCamera == null)
            {
                Debug.LogError("Player camera is missing!");
            }

            // Image's RectTransform
            _selectionBox = GetComponentInChildren<Canvas>().GetComponentInChildren<Image>().GetComponent<RectTransform>();
            if (_selectionBox != null)
            {
                _selectionBox.gameObject.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        HandleInput();
    }

    private void HandleInput()
    {
        if (_leftClickAction.WasPressedThisFrame())
        {
            _selectionStart = Mouse.current.position.ReadValue();
            _isDragging = false;
            _isClickProcessed = false;
        }

        if (_leftClickAction.IsPressed())
        {
            Vector2 curMousePos = Mouse.current.position.ReadValue();

            if (Vector2.Distance(_selectionStart, curMousePos) > 2f)
            {
                _isDragging = true;
                _selectionEnd = curMousePos;
                UpdateSelectionBox();
                if (_selectionBox != null)
                {
                    _selectionBox.gameObject.SetActive(true);
                }
            }
        }

        if (_leftClickAction.WasReleasedThisFrame())
        {
            _selectionEnd = Mouse.current.position.ReadValue();
            _isDragging = false;

            if (_isDragging)
            {
                _selectionEnd = Mouse.current.position.ReadValue();
                PerformSelectionBox();
            }
            else if (!_isClickProcessed)
            {
                SelectEntityUnderCursor();
            }

            _isDragging = false;
            _isClickProcessed = true;

            if (_selectionBox != null)
            {
                _selectionBox.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateSelectionBox()
    {
        if (_selectionBox == null) return;

        Vector2 boxStart = _selectionStart;
        Vector2 boxEnd = _selectionEnd;

        Vector2 size = new Vector2(Mathf.Abs(boxEnd.x - boxStart.x), Mathf.Abs(boxEnd.y - boxStart.y));
        Vector2 center = (boxStart + boxEnd) / 2;

        _selectionBox.position = center;
        _selectionBox.sizeDelta = size;
    }

    private void PerformSelectionBox()
    {
        Rect selectionRect = new Rect(
            Mathf.Min(_selectionStart.x, _selectionEnd.x),
            Mathf.Min(_selectionStart.y, _selectionEnd.y),
            Mathf.Abs(_selectionEnd.x - _selectionStart.x),
            Mathf.Abs(_selectionEnd.y - _selectionStart.y)
        );

        List<Entity> entities = new();
        foreach (Entity entity in FindObjectsByType<Entity>(FindObjectsSortMode.None))
        {
            Vector3 screenPosition = _playerCamera.WorldToScreenPoint(entity.transform.position);

            if (selectionRect.Contains(screenPosition))
            {
                Debug.Log("HIT");
                entities.Add(entity);
                SelectEntityServerRpc(entity.NetworkObject);
            }
        }

        if (entities.Count == 0)
        {
            ClearSelectionServerRpc();
        }
    }

    private void SelectEntityUnderCursor()
    {
        Ray ray = _playerCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            var entity = hit.collider.GetComponent<Entity>();
            if (entity != null)
            {
                Debug.Log("SelectEntityUnderCursor HIT");
                SelectEntityServerRpc(entity.NetworkObject);
            }
        }
        else
        {
            ClearSelectionServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SelectEntityServerRpc(NetworkObjectReference entityReference)
    {
        if (entityReference.TryGet(out NetworkObject entityObject))
        {
            var entity = entityObject.GetComponent<Entity>();
            if (entity != null)
            {
                // Update the selection in the Player class
                _player.AddToSelection(entityReference);

                // Set the entity's selection state
                entity.SetSelectedServerRpc(true);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ClearSelectionServerRpc()
    {
        foreach (var entityReference in _player.SelectedEntities)
        {
            if (entityReference.TryGet(out NetworkObject entityObject))
            {
                var entity = entityObject.GetComponent<Entity>();
                if (entity != null)
                {
                    entity.SetSelectedServerRpc(false);
                }
            }
        }
        _player.ClearSelection();
    }
}
