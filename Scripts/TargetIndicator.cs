using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//Attach this class to the GameObject you want the arrow to be pointing at.
public class TargetIndicator : MonoBehaviour
{

    public GameObject offscreen_icon;

    public GameObject onscreen_icon;
    public GameObject speedIndicator;

    public Text distance_meter;
    private float iconSize = 35f;

    private AircraftController ac;
    private float velScale = 1/6f;

    public Color colour = Color.white;

    Vector3 offscreen_size;

    Camera cam;
    bool visible = true; //Whether or not the object is visible in the camera.

    public void ChangeColor(Color col)
    {
        offscreen_icon.GetComponent<Image>().color *= col;
        onscreen_icon.GetComponent<SpriteRenderer>().color *= col;
        distance_meter.GetComponent<Text>().color *= col;
    }

    void Start()
    {
        ChangeColor(colour);

        ac = GetComponentInParent<AircraftController>();

        onscreen_icon.GetComponentInParent<Canvas>().worldCamera = cam;
        offscreen_size = offscreen_icon.transform.localScale;

        cam = Camera.main;

        

        offscreen_icon.transform.SetParent(GameObject.Find("Canvas").transform);
        //distance_meter.transform.parent = offscreen_icon.transform;
        distance_meter.transform.SetParent(offscreen_icon.transform);
    }

    private void LateUpdate()
    {
        visible = onscreen_icon.GetComponent<Renderer>().isVisible;

        offscreen_icon.transform.localScale = offscreen_size * iconSize;
        //onscreen_icon.transform.localScale = onscreen_size * cam.orthographicSize / iconSize;
        //distance_meter.transform.localScale = meter_size * cam.orthographicSize / iconSize;

        if (ac != null)
        {
            Vector2 vel = velScale * (ac.rb.velocity - new Vector2(cam.velocity.x, cam.velocity.y));

            speedIndicator.transform.Rotate(0, 0, vel.magnitude);
        }

        if (!visible)
        {
            if (offscreen_icon.activeSelf == false)
            {
                offscreen_icon.SetActive(true);
            }
            if (speedIndicator.activeSelf == false)
            {
                speedIndicator.SetActive(true);
            }

            Vector2 camPos = new Vector2(cam.transform.position.x, cam.transform.position.y);
            Vector2 disPos = new Vector2(transform.position.x, transform.position.y) - camPos;
            float distance = disPos.magnitude;

            distance_meter.text = distance.ToString("0");
            distance_meter.transform.rotation = cam.transform.rotation;

            Vector2 dir = disPos.normalized;

            /*if (dir.magnitude != 1)
            {
                dir = dir.normalized;
            }*/

            

            offscreen_icon.transform.rotation = transform.parent.rotation;

            RaycastHit2D hit = Physics2D.Linecast(transform.position, camPos, LayerMask.GetMask("Camera"));

            Debug.DrawLine(transform.position, camPos, Color.red);

            if (hit.collider != null)
            {
                offscreen_icon.transform.position = Vector2.Lerp(offscreen_icon.transform.position, hit.point, 25 * Time.deltaTime);
            }
            //offscreen_icon.transform.position = new Vector2(Screen.width / 2 * dir.x, Screen.height / 2 * dir.y) / range * cam.orthographicSize + camPos;


        }
        else
        {
            if (offscreen_icon.activeSelf == true)
            {
                offscreen_icon.SetActive(false);
            }
            onscreen_icon.transform.rotation = cam.transform.rotation;
        }
    }

    private void OnDisable()
    {
        if (offscreen_icon != null)
        {
            offscreen_icon.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        Destroy(offscreen_icon);
    }
}