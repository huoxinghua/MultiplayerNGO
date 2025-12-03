using System.Collections.Generic;
using UnityEngine;

namespace _Project.Code.Gameplay.Player
{
    public class PlayerList : MonoBehaviour
    {
//CANT DELETE YET MUST FIX - BAD SYSTEM
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
}
