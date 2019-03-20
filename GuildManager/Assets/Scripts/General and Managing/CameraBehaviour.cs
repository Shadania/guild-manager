using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// rotates camera around player so it stays third person
public class CameraBehaviour : MonoBehaviour
{
    // Declarations
    public float RotateSpeed = 10.0f;
    private float _xRotation = 0.0f;
    private Vector3 _cameraOffset;



    private void Start()
    {
        // setting the camera offset to what it is inside the editor on start
        _cameraOffset = Camera.main.transform.parent.parent.transform.position - transform.position;
    }


    private void Update()
    {
        // set cursor state depending on if RotateCamera button is held down
        if (Input.GetButton("RotateCamera"))
            Cursor.lockState = CursorLockMode.Locked;
        else
            Cursor.lockState = CursorLockMode.None;
    }

    // LateUpdate so the camera's always right behind the player
    void LateUpdate()
    {
        UpdateCameraRotation();
    }

    void UpdateCameraRotation()
    {
        // only rotate camera + player if cursor is in locked mode
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            float horizontal = Input.GetAxis("Mouse X") * RotateSpeed;
            _xRotation -= Input.GetAxis("Mouse Y") * RotateSpeed;
            GameManager.Instance.PlayerAvatar.transform.Rotate(0, horizontal, 0);
        }

        float desiredAngle = GameManager.Instance.PlayerAvatar.transform.eulerAngles.y;
        Quaternion rotation = Quaternion.Euler(_xRotation, desiredAngle, 0);
        transform.position = GameManager.Instance.PlayerAvatar.transform.position - (rotation * _cameraOffset);

        transform.LookAt(GameManager.Instance.PlayerAvatar.transform);
    }
}
