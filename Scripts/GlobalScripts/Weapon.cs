using UnityEngine.Audio;
using UnityEngine;


[System.Serializable]
public class Weapon
{
    public string name;

    public float damage = 10;
    public float range = 15;
    public float projectile_speed = 20;
    public float inaccuracy = 0;
    public float fire_rate = 0.2f;
    public int multi_shot = 1;
    public int burst_fire = 3;

    public float heat_rate = 10;
    public bool sound_loop;
    public AudioClip gunshot;
    public GameObject bullet;

    public float rotation_speed = 5;
}
