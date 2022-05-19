using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorScript : MonoBehaviour
{
    [HideInInspector]
    public RectTransform RectTransform;
    public Vector2 LockOn;
    Vector2 Offset = Vector2.zero;
    public Vector2 PlayerOnScreen;
    public float cursorSpeed;

    Vector2 Mouse;
    Vector2 Joystick = Vector2.up;
    

    public float JoystickCursorDistance = 10;

    bool mouseOn;

    // Start is called before the first frame update
    void Start()
    {
        RectTransform = GetComponent<RectTransform>();
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Mouse = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        Vector2 rJoystick = new Vector2(Input.GetAxisRaw("Joystick X"), Input.GetAxisRaw("Joystick Y"));

        if (rJoystick.magnitude > 0.01f)
        {
            Joystick = rJoystick.normalized;
            mouseOn = false;
        }
        if (Mouse.magnitude > 0)
        {
            mouseOn = true;
        }

        if (mouseOn)
        {
            RectTransform.localPosition = Input.mousePosition - (Vector3)Camera.main.pixelRect.size / 2;
        } else
        {
            RectTransform.localPosition = Joystick * Camera.main.pixelRect.size.magnitude * JoystickCursorDistance / 100 + PlayerOnScreen - Camera.main.pixelRect.size / 2;
        }
    }
}
