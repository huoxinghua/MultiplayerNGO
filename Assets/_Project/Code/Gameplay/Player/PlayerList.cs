using System.Collections.Generic;
using UnityEngine;

public class PlayerList : MonoBehaviour
{

    public static List<PlayerList> AllPlayers = new List<PlayerList>();

    void Awake()
    {
        AllPlayers.Add(this);
    }

    void OnDestroy()
    {
        AllPlayers.Remove(this);
    }
}
