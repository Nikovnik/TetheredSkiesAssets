using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactionManager : MonoBehaviour
{
    [System.Serializable]
    public class Team        //team blueprint
    {
        public string teamName;
        public Color teamColor;
        public List<Deployment> Squads;
        //public List<City> Cities; //for later

        public int publicProfile;
        public int aggressiveness;

        public int quality;
        public int quantity;

        public List<int> ReputationList;  //treat as a part of a symetrical 2D matrix where diagonaly is always 100
    }

    public List<Team> Teams;

    // Start is called before the first frame update
    void Awake()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int ReadReputation(Team A, Team B)
    {
        return Teams[GetTeamID(A)].ReputationList[GetTeamID(B)];
    }

    public void WriteReputation(Team A, Team B, int amount)
    {
        Teams[GetTeamID(A)].ReputationList[GetTeamID(B)] += amount;
        Teams[GetTeamID(B)].ReputationList[GetTeamID(A)] += amount;
    }

    private void SymetryzeReputation()
    {
        for(int i = 0; i < Teams.Count; i++)
        {
            for(int j = 0; j < Teams.Count; j++)
            {
                if( i == j)
                {
                    Teams[i].ReputationList[j] = 100;
                } else
                {
                    Teams[j].ReputationList[i] = Teams[i].ReputationList[j];
                }
            }
        }
    }

    public void AddTeam(Team T)
    {
        Teams.Add(T);

        foreach (var team in Teams)
        {
            if(team == T)
            {
                team.ReputationList.Add(100);
            }
            else
            {
                team.ReputationList.Add(T.ReputationList[GetTeamID(team)]);
            }
        }
    }

    public void RemoveTeam(Team T)
    {
        int index = GetTeamID(T);
        Teams.RemoveAt(index);

        foreach(var team in Teams)
        {
            team.ReputationList.RemoveAt(index);
        }
    }

    private int GetTeamID(Team T)
    {
        return Teams.FindIndex(x => x == T);
    }
}
