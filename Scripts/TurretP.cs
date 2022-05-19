using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretP : MonoBehaviour
{
    public float rotation_speed = 10;
    public float angle_range = 30;
    public float angle_offset = 30;
    public float angle;
    public float parent_angle;

    private void FixedUpdate()
    {
        //WARINING TEST ONLY
        Transform parent_object = gameObject.transform.parent.transform;
        parent_angle = parent_object.rotation.eulerAngles.z;
        
        Vector2 target = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        Quaternion q_angle = Quaternion.Euler(0, 0, Mathf.Atan2(target.y, target.x) * Mathf.Rad2Deg + 270f);
        angle = q_angle.eulerAngles.z;

        Quaternion min_clamp = Quaternion.Euler(0, 0, angle_range + angle_offset + parent_angle);
        Quaternion max_clamp = Quaternion.Euler(0, 0, -angle_range + angle_offset + parent_angle);

        Vector3 currentRotation = transform.rotation.eulerAngles;

        if (min_clamp.eulerAngles.z > max_clamp.eulerAngles.z)
        {
            if (angle < 180) currentRotation.z = Mathf.Clamp(angle, 0, max_clamp.eulerAngles.z);
            if (angle > 180) currentRotation.z = Mathf.Clamp(angle, min_clamp.eulerAngles.z, 360);

        } else
        {
            currentRotation.z = Mathf.Clamp(angle, min_clamp.eulerAngles.z, max_clamp.eulerAngles.z);
        }
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(currentRotation), rotation_speed);
    }
}