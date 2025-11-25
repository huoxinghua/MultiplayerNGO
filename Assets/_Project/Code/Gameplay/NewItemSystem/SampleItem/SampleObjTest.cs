using _Project.Code.Gameplay.Interactables;
using _Project.Code.Gameplay.Interfaces;
using UnityEngine;

namespace _Project.Code.Gameplay.NewItemSystem.SampleItem
{
    public class SampleObjTest : MonoBehaviour,ISampleable
    {
        [SerializeField] SampleSO sampleSO;

        public SampleSO GetSample()
        {
            return sampleSO;
        }

  
    }
}
