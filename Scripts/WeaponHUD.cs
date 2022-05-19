using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHUD : MonoBehaviour
{
    ArnamentP Weapon;

    public GameObject Vpart;
    float v = 0.07f;
    public GameObject Hpart;
    float h = 0.035f;

    float angle_range;
    float range;
    Vector2 target;

    // Start is called before the first frame update
    void Start()
    {
        if (transform.parent != null)
        {
            Weapon = GetComponentInParent<ArnamentP>();
            range = Weapon.range;
            
        }

    }

    public static Vector2 RadianToVector2(float radian)
    {
        return new Vector2(Mathf.Cos(radian + Mathf.PI/2), Mathf.Sin(radian + Mathf.PI / 2));
    }

    public static Vector2 DegreeToVector2(float degree)
    {
        return RadianToVector2(degree * Mathf.Deg2Rad);
    }

    // Update is called once per frame
    void Update()
    {
        target = Weapon.targetVector;
        float fi = Mathf.Atan2(target.y - Weapon.transform.position.y, target.x - Weapon.transform.position.x) * Mathf.Rad2Deg - 90f;
        float d = (target - new Vector2(Weapon.transform.position.x, Weapon.transform.position.y)).magnitude;
        d = Mathf.Clamp(d, 0, Weapon.range);
        transform.localPosition = Vector2.up * d;

        Hpart.transform.localScale = new Vector3(h * d * Weapon.inaccuracy, Hpart.transform.localScale.y, 1);
        Vpart.transform.localScale = new Vector3(Vpart.transform.localScale.x, v * d * Weapon.inaccuracy, 1);
    }

    public static Vector2 RadialPosition(float fi, float d)
    {
        return DegreeToVector2(fi) * d;
    }

    public IEnumerator ShutDown()
    {
        float t = 1;

        while (t > 0)
        {
            transform.localScale = Vector3.one * t;
            t -= 1 * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        transform.localScale = Vector3.zero;

        gameObject.SetActive(false);
    }
    public IEnumerator TurnOn()
    {
        gameObject.SetActive(true);

        float t = 0;

        while (t < 1)
        {
            transform.localScale = Vector3.one * t;
            t += 1 * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        transform.localScale = Vector3.one;
    }
}
