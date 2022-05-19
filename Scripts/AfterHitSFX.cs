using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterHitSFX : MonoBehaviour
{
    public float pitchRange = 0.1f;
    [HideInInspector]
    public float hitAmount;

    private float pitchAmount;
    private float volumeAmount;

    public float medium;

    public AnimationCurve VmM;
    public AnimationCurve PmM;

    // Start is called before the first frame update
    void Start()
    {
        ParticleSystem ps = GetComponent<ParticleSystem>();
        var psE = ps.emission;

        //psE.type = ParticleSystemEmissionType.Time;

        psE.SetBursts(
              new ParticleSystem.Burst[] {
                  new ParticleSystem.Burst (0.01f, Mathf.RoundToInt(hitAmount/2) + 1)
              });

        AudioSource AuS = GetComponent<AudioSource>();
        volumeAmount = VmM.Evaluate(hitAmount/medium);
        pitchAmount = PmM.Evaluate(hitAmount/medium);

        AuS.pitch = pitchAmount + Random.Range(-pitchRange, pitchRange);
        AuS.volume = volumeAmount;

        AuS.PlayOneShot(AuS.clip);

        ps.Play();
    }
}
