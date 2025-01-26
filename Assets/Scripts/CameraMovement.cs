using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerCamera : NetworkBehaviour
{
    [SerializeField] private float _moveSpeed = 5.0f;
    [SerializeField] private int _zoomLevel = 5;
    [SerializeField] private float _edgeSize = 30f;

    private InputAction _moveAction;
    private InputAction _zoomAction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        _moveAction = InputSystem.actions.FindAction("Move");
        _zoomAction = InputSystem.actions.FindAction("Zoom");
    }

    // Update is called once per frame
    private void Update()
    {
        Vector2 moveValue = _moveAction.ReadValue<Vector2>();
        transform.position += new Vector3(moveValue.x, 0, moveValue.y) * _moveSpeed * Time.deltaTime;

        if (Application.isFocused) {
            if (Mouse.current.position.ReadValue().x > Screen.width - _edgeSize) {
                transform.position += Vector3.right * _moveSpeed * Time.deltaTime;
            }
            if (Mouse.current.position.ReadValue().x < _edgeSize) {
                transform.position -= Vector3.right * _moveSpeed * Time.deltaTime;
            }
            if (Mouse.current.position.ReadValue().y > Screen.height - _edgeSize) {
                transform.position += Vector3.forward * _moveSpeed * Time.deltaTime;
            }
            if (Mouse.current.position.ReadValue().y < _edgeSize) {
                transform.position -= Vector3.forward * _moveSpeed * Time.deltaTime;
            }
        }

        float zoomValue = _zoomAction.ReadValue<float>();
        if (zoomValue > 0)
        {
            _zoomLevel += 1;
            transform.position -= new Vector3(0, 50f, 0) * Time.deltaTime;
        }
        else if (zoomValue < 0)
        {
            _zoomLevel -= 1;
            transform.position -= new Vector3(0, -50f, 0) * Time.deltaTime;
        }
    }
}