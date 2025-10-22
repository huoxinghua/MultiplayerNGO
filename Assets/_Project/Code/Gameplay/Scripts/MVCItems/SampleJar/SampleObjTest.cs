using _Project.Code.Gameplay.Scripts.Interfaces;
using UnityEngine;

namespace _Project.Code.Gameplay.Scripts.MVCItems.SampleJar
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
