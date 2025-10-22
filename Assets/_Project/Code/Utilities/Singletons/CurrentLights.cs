using System.Collections.Generic;
using _Project.Code.Core.Patterns;
using _Project.Code.Optimization;

namespace _Project.Code.Utilities.Singletons
{
    public class CurrentLights : Singleton<CurrentLights>
    {
        public List<LightObject> Lights = new List<LightObject>();
        public void AddLight(LightObject playerObj)
        {
            Lights.Add(playerObj);

        }
        public void RemoveLight(LightObject playerObj)
        {
            Lights.Remove(playerObj);

        }
        public void ClearLights()
        {
            Lights.Clear();

        }
    }
}
