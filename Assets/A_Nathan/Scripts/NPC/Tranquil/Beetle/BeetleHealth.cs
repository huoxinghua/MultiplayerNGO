using System.Collections.Generic;
using UnityEngine;

public class BeetleHealth : MonoBehaviour
{
    //add players who attacked to list
    public List<GameObject> hostilePlayers = new List<GameObject>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    public bool IsPlayerHostile(GameObject playerToCheck)
    {
        bool isHostile = false;
        foreach(var hostilePlayer  in hostilePlayers)
        {
            if(playerToCheck == hostilePlayer)
            {
                isHostile = true;
            }
        }
        return isHostile;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
