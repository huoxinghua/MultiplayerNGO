using _Project.Code.Core.Patterns;
using System.Collections.Generic;
using UnityEngine;

public class CurrentPlayers : Singleton<CurrentPlayers>
{
    public List<Transform> PlayerTransforms = new List<Transform>();
    public List<GameObject> PlayerGameObjects = new List<GameObject>();

    public void AddPlayer(GameObject playerObj)
    {
        PlayerGameObjects.Add(playerObj);
        PlayerTransforms.Add(playerObj.transform);
    }
    public void RemovePlayer(GameObject playerObj) 
    {
        PlayerGameObjects.Remove(playerObj);
        PlayerTransforms.Remove(playerObj.transform);
    }
    public void ClearLists()
    {
        PlayerGameObjects.Clear();
        PlayerTransforms.Clear();
    }
}
