using UnityEngine;

namespace _Project.Code.Gameplay.Scripts.MVCItems.SampleJar
{
    public class SampleJarModel 
    {
        public bool IsOn =false;
        public bool IsInHand = false;
        public GameObject Owner;

        public void Toggle()
        {
            IsOn = !IsOn;
        }
        public void InHand(bool inHand)
        {
            IsInHand = inHand;
        }
        public void SetOwner(GameObject player)
        {
            Owner = player;
        }

        public void ClearOwner()
        {
            Owner = null;
            IsOn = false;
        }
        public bool HasOwner => Owner != null;
    }
}
