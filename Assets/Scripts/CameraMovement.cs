using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class Camera : NetworkBehaviour
{
    public float MoveSpeed = 5.0f;
    public int ZoomLevel = 5;
    private InputAction _moveAction;
    private InputAction _zoomAction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _moveAction = InputSystem.actions.FindAction("Move");
        _zoomAction = InputSystem.actions.FindAction("Zoom");
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 moveValue = _moveAction.ReadValue<Vector2>();
        transform.position += new Vector3(moveValue.x, 0, moveValue.y) * MoveSpeed * Time.deltaTime;

        float zoomValue = _zoomAction.ReadValue<float>();
        if (zoomValue > 0)
        {
            ZoomLevel += 1;
            transform.position -= new Vector3(0, 50f, 0) * Time.deltaTime;
        }
        else if (zoomValue < 0)
        {
            ZoomLevel -= 1;
            transform.position -= new Vector3(0, -50f, 0) * Time.deltaTime;
        }
    }
}