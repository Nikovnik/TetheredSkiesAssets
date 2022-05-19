using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldControl : MonoBehaviour
{
    public enum DayPart { morning, day, eve, night, realtime, random}

    public float timeSpeed = 0;
    public enum DayMood { clear, cloudy, rainy, storm}

    public bool isSnowy = false;

    public DayMood dayMood;
    public DayPart dayPart;

    public Light sun_light;
    public float pos_y = -15;
    public float pos_x = 25;

    private float dus_colour;
    public float max_dus_colour = 0.4f;
    public float dus_speed;

    public float light_min = 0;
    public float light_max = 1;

    [Range(1, 20)]
    public float dawnLength = 1;

    [Range(0.25f, 0.75f)]
    public float dayBias = 0.5f;

    public GameObject snow;
    public GameObject rain;
    public GameObject clouds;
    public GameObject water;
    public GameObject terrain;

    public bool rainfalling;
    public float moisture;
    public float wind_strenght = 500;
    public float temperature = 25;
    float dayCycle;

    float time_current = 0;

    private float dayPeriodSec = 86400;

    public Vector2 world_speed;

    Canvas canvas;

    public Vector2 World_Border;

    // Start is called before the first frame update
    void Start()
    {
        clouds.GetComponent<Renderer>().material.SetVector("world_speed", new Vector4(world_speed.x, world_speed.y));
        terrain.GetComponent<Renderer>().material.SetVector("world_speed", new Vector4(world_speed.x, world_speed.y));
        water.GetComponent<Renderer>().material.SetVector("world_speed", new Vector4(world_speed.x, world_speed.y));

        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();

        sun_light.intensity = 1;
        sun_light.intensity = 0;

        switch (dayPart)
        {
            case (DayPart.day):
                time_current = dayPeriodSec / 2;
                break;

            case (DayPart.eve):
                time_current = dayPeriodSec * 3 / 4;
                break;

            case (DayPart.night):
                time_current = 0;
                break;

            case (DayPart.morning):
                time_current = dayPeriodSec / 4;
                break;
            case (DayPart.realtime):
                time_current = RealTime();
                break;
            case (DayPart.random):
                time_current = dayPeriodSec * UnityEngine.Random.Range(0f, 1f);
                break;
        }

        switch (dayMood)
        {
            case (DayMood.clear):
                moisture = 0;
                rain.SetActive(false);
                snow.SetActive(false);
                break;

            case (DayMood.cloudy):
                moisture = 75f;
                rain.SetActive(false);
                snow.SetActive(false);
                break;

            case (DayMood.rainy):
                moisture = 100f;
                if (isSnowy)
                {
                    rain.SetActive(false);
                    snow.SetActive(true);
                } else
                {
                    rain.SetActive(true);
                    snow.SetActive(false);
                }
                break;

            case (DayMood.storm):
                moisture = 100f;
                if (isSnowy)
                {
                    snow.SetActive(true);
                    rain.SetActive(false);
                } else
                {
                    snow.SetActive(false);
                    rain.SetActive(true);
                }
                break;

        }

        rain.GetComponent<Renderer>().material.SetFloat("Speed", wind_strenght);
        snow.GetComponent<Renderer>().material.SetFloat("Speed", wind_strenght);

        water.GetComponent<Renderer>().material.SetFloat("cloudiness", moisture / 100);
        clouds.GetComponent<Renderer>().material.SetFloat("cloudiness", moisture / 100);

        clouds.GetComponent<Renderer>().material.SetVector("Cloud_Speed", world_speed / 10);
        terrain.GetComponent<Renderer>().material.SetVector("World_Speed", world_speed / 10);
    }

    // Update is called once per frame
    void LateUpdate()
    {

        //daylight control

        
        time_current += Time.deltaTime * timeSpeed;
        

        dayCycle = dawnLength * (-Mathf.Cos(2 * Mathf.PI/ dayPeriodSec * time_current)) + dayBias;

        //if (sun_light.intensity < 0){ sun_light.intensity = 0; }
        //if (sun_light.intensity > 1){ sun_light.intensity = 1; }

        sun_light.intensity = Mathf.Clamp(dayCycle, light_min, light_max);
        float t_pos_x = pos_x * Mathf.Sin(2 * Mathf.PI / dayPeriodSec * time_current);
        sun_light.transform.rotation = Quaternion.Euler(pos_y, t_pos_x, 0);

        dus_colour = Mathf.Clamp(Mathf.Abs(dayCycle - dayBias) + dayBias, max_dus_colour, 1);

        sun_light.color = new Color(sun_light.color.r, dus_colour, dus_colour);




        //info display control

        foreach (var display in canvas.GetComponentsInChildren<Text>())
        {
            if (display.name == "Clock")
            {
                float hours = time_current / dayPeriodSec * 24;
                int minutes = Mathf.FloorToInt(hours * 60 % 60);

                string minuteS = minutes < 10 ? "0" + minutes : minutes.ToString();
                display.text = Mathf.FloorToInt(hours) +":"+minuteS;
            }

            if (display.name == "Termometer")
            {
                display.text = "T: " + temperature.ToString("0") + " °C";
            }

            if(display.name == "Humidity meter")
            {
                display.text = "W: " + moisture.ToString("0") + " %";
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(2*World_Border.x, 2*World_Border.y));
    }

    float RealTime()
    {
        return System.DateTime.Now.TimeOfDay.Seconds + System.DateTime.Now.TimeOfDay.Minutes * 60 + System.DateTime.Now.TimeOfDay.Hours * 3600;
    }
}
