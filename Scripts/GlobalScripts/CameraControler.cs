using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Audio;

public class CameraControler : MonoBehaviour
{
    public Transform player_coords;
    public bool free_cam = false;
    public float indicatorMargin = 100f;

    public float free_cam_speed = 2;
    private float height;
    public float hardness = 10;

    private float camera_x;
    private float camera_y;

    [Range(0f, 10f)]
    public float dampTime = 0.15f;
    private Vector3 velocity = Vector3.zero;

    Vector3 destination;

    public float zoomSpeed = 1;
    public float targetOrtho;
    public float smoothSpeed = 2.0f;
    public float minOrtho = 1.0f;
    public float maxOrtho = 15.0f;

    private Vector3 shake_destination1;
    private Vector3 shake_destination2;
    private Volume volume;
    public float shakeStabilization = 0;
    public float shakeAmount;
    private float suppressDepleteSpeed = 1f;
    public float suppressAmount = 0;
    public float consciousness = 0;

    public Transform childTransform;

    private bool track_player;
    Vector2 CursorPosition;
    CursorScript CursorObject;

    private AudioLowPassFilter LowPassFilter;

    UnityEngine.Rendering.Universal.Vignette vignette;
    UnityEngine.Rendering.Universal.LiftGammaGain liftGammaGain;

    public bool blackout = false;

    WorldControl World;

    void Awake()
    {
        CursorObject = FindObjectOfType<CursorScript>();
        World = FindObjectOfType<WorldControl>();
    }
    // Start is called before the first frame update
    void Start()
    {
        Camera.main.orthographicSize = maxOrtho;
        targetOrtho = maxOrtho;
        

        height = transform.position.z;
        transform.position = new Vector2(0, 0);
        volume = FindObjectOfType<Volume>();

        if (!volume.profile) throw new System.NullReferenceException(nameof(UnityEngine.Rendering.VolumeProfile));

        if (!volume.profile.TryGet(out vignette)) throw new System.NullReferenceException(nameof(vignette));
        if (!volume.profile.TryGet(out liftGammaGain)) throw new System.NullReferenceException(nameof(liftGammaGain));

        vignette.intensity.Override(0);
        liftGammaGain.gain.Override(new Vector4(0, 0, 0, 0));

        LowPassFilter = GetComponentInChildren<AudioLowPassFilter>();

        if (LowPassFilter == null)
        {
            Debug.LogError("No AudioLowPassFilter in cameras children.");
        }
    }

    

    void LateUpdate()
    {
        CursorPosition = CursorObject.RectTransform.localPosition;

        GetComponent<BoxCollider2D>().size = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height) * indicatorMargin) - transform.position;

        PilotScript[] Pilots = FindObjectsOfType<PilotScript>();

        foreach (var pilot in Pilots)
        {
            if (pilot.manual)
            {
                player_coords = pilot.transform;
            }
        }

        //player


        if (player_coords == null)
        {
            track_player = false;
        } else
        {
            track_player = true;
        }


        float clampX = World.World_Border.x - Camera.main.orthographicSize * Camera.main.aspect;
        float clampY = World.World_Border.y - Camera.main.orthographicSize;

        if (!track_player)
        {

            destination.x += Input.GetAxisRaw("Horizontal") * free_cam_speed * Time.deltaTime * Camera.main.orthographicSize;
            destination.y += Input.GetAxisRaw("Vertical") * free_cam_speed * Time.deltaTime * Camera.main.orthographicSize;

            destination.z = height;
        } else
        {
            camera_x = player_coords.position.x + Camera.main.WorldToScreenPoint(CursorPosition).x * hardness * Camera.main.orthographicSize / Screen.width / 2;
            camera_y = player_coords.position.y + Camera.main.WorldToScreenPoint(CursorPosition).y * hardness * Camera.main.orthographicSize / Screen.height / 2;
            //Vector3 point = Camera.WorldToViewportPoint(Player.position);
            //Vector3 delta = Player.position - camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z)); //(new Vector3(0.5, 0.5, point.z));
            destination = new Vector3(camera_x, camera_y, height);
        }
        Vector3 deltaPos = Vector3.zero;
        //camera shake
        shakeStabilization = shakeAmount * 10 + 1;
        if (shakeAmount > 0)
        {

            deltaPos = new Vector3(Mathf.Cos(Random.Range(0, 360 * Mathf.Deg2Rad)), Mathf.Sin(Random.Range(0, 360 * Mathf.Deg2Rad)), 0) * shakeAmount * Camera.main.orthographicSize / 1000; // change to trigonometric random
            //transform.position = Vector3.Lerp(transform.position, pp, 0.5f);
            shakeAmount -= shakeStabilization * Time.deltaTime;
        } else
        {
            shakeAmount = 0;
        }

        destination = new Vector3(Mathf.Clamp(destination.x, -clampX, clampX), Mathf.Clamp(destination.y, -clampY, clampY), height);

        transform.position =Vector3.SmoothDamp(transform.position, destination, ref velocity, dampTime) + deltaPos;

        transform.position = new Vector3(Mathf.Clamp(transform.position.x, -clampX, clampX), Mathf.Clamp(transform.position.y, -clampY, clampY), transform.position.z);


        childTransform.position = new Vector2(transform.position.x, transform.position.y);

        //suppression effect

        float visibility = 0.1f + suppressAmount / 2 - consciousness;

        visibility = Mathf.Clamp(visibility, 0, 1f);

        suppressAmount = Mathf.Clamp(suppressAmount, 0, 2f);
        consciousness = Mathf.Clamp(consciousness, 0, 1f);
        

        vignette.intensity.Override(Mathf.Clamp(suppressAmount/4, 0, 0.25f));
        liftGammaGain.gain.Override(new Vector4(0, 0, 0, -1 + consciousness));
        LowPassFilter.cutoffFrequency = Mathf.Lerp(1000, 22000, consciousness);

        if (suppressAmount > 0)
        {
            suppressAmount -= suppressDepleteSpeed * Time.deltaTime;
        }

        if (!track_player && !blackout)
        {
            suppressAmount = Mathf.Lerp(suppressAmount, 0, 1f * Time.deltaTime);
            consciousness = Mathf.Lerp(consciousness, 1f, 1f * Time.deltaTime);
            LowPassFilter.cutoffFrequency = 22000;
        }

        //zooming
        if (true)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0.0f)
            {
                targetOrtho -= scroll * zoomSpeed * Camera.main.orthographicSize / 10;
                targetOrtho = Mathf.Clamp(targetOrtho, minOrtho, maxOrtho);
            }
        }
        else
        {
            Vector2 mousePos = CursorPosition/ Camera.main.pixelRect.center;
            targetOrtho = Mathf.Lerp(minOrtho, maxOrtho, mousePos.magnitude);
        }
        Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, targetOrtho, smoothSpeed * Time.deltaTime); //Mathf.Lerp(Camera.main.orthographicSize, targetOrtho, smoothSpeed * Time.fixeddeltaTime)
    }
}
