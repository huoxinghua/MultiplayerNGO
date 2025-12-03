using UnityEngine;

namespace _Project.ScriptableObjects.ScriptObjects.GameTime
{
    [CreateAssetMenu(fileName = "GameTimeUI", menuName = "GameTime/GameTimeUI")]
    public class GameTimeVisualSO : ScriptableObject
    {
        [field: SerializeField] public float DesiredGameTime { get; private set; }
        [field: SerializeField] public int StartTimeMilitaryTime { get; private set; }
        [field: SerializeField] public int EndTimeMilitaryTime { get; private set; }
        private float DesiredHoursRange
        {
            get
            {
                if (EndTimeMilitaryTime < StartTimeMilitaryTime)
                {
                    return  EndTimeMilitaryTime+24 - StartTimeMilitaryTime;
                }
                else
                {
                    return EndTimeMilitaryTime - StartTimeMilitaryTime;
                }
            }
        }

        public float SecondsToGameMinutes => DesiredHoursRange / DesiredGameTime;
    }
}