using System.Collections.Generic;
using UnityEngine;

namespace _Project.Code.Gameplay.Player
{
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
}
