using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{

    public GameObject pauseMenu;

    public Deployment[] Squads;
    private int currentSquad = 0;

    [Space(30)]

    public static bool GamePaused = false;
    public AudioMixerSnapshot paused_F;
    public AudioMixerSnapshot default_F;

    public ObjectManager m_craft, m_main, m_side;

    // Start is called before the first frame update
    void Start()
    {
        if (!GamePaused)
            SquadEditSetActive(false, true);
            pauseMenu.gameObject.SetActive(false);

        Squads[currentSquad].player_spawn = true;
        //Squads = FindObjectsOfType<Deployment>();
        Invoke("Pause", 1.5f);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (!GamePaused)
            {
                Pause();
            } else
            {
                Resume();
            }
        }
    }

    public void ChangeSquad()
    {
        if(GamePaused)
            SquadEditSetActive(false, true);

        Squads[currentSquad].player_spawn = false;
        if (currentSquad == Squads.Length - 1)
        {
            currentSquad = 0;
        } else
        {
            currentSquad++;
        }
        Squads[currentSquad].player_spawn = true;

        if (GamePaused)
            SquadEditSetActive(true);
    }

    void SquadEditSetActive(bool activeState, bool toAll = false)
    {
        foreach (var squad in Squads)
        {
            if (squad.player_spawn || toAll)
            {
                squad.SquadEditPanel.gameObject.SetActive(activeState);
            }
        }
    }

    //Game pause
    public void Pause()
    {
        GamePaused = true;
        pauseMenu.gameObject.SetActive(true);
        SquadEditSetActive(true);
        Time.timeScale = 0;

        paused_F.TransitionTo(0);
    }

    public void Resume()
    {
        GamePaused = false;
        pauseMenu.gameObject.SetActive(false);
        SquadEditSetActive(false, true);
        Time.timeScale = 1;

        default_F.TransitionTo(0);
    }

    public void QuitGame()
    {
        Debug.Log("Quiting game");
        Application.Quit();
    }
}
