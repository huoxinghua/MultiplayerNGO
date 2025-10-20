using _Project.Code.Core.Patterns;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

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
