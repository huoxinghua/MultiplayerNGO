using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    public float CellSize = 20f;
    public float ActiveRange = 30f;
    public float CheckInterval = 0.5f;

    private SpatialGrid _grid;

    private CurrentLights _currentLights;
    private CurrentPlayers _currentPlayers;
    private List<LightObject> allLights => _currentLights.Lights;
    public void Awake()
    {
        _currentPlayers = CurrentPlayers.Instance;
        _currentLights = CurrentLights.Instance;
    }
    void Start()
    {
        _grid = new SpatialGrid(CellSize);

        foreach (var light in allLights)
        {
            _grid.AddLight(light);
        }

        StartCoroutine(CheckLightsRoutine());
    }

    IEnumerator CheckLightsRoutine()
    {
        while (true)
        {
            HashSet<LightObject> lightsToEnable = new();

            foreach (var player in _currentPlayers.PlayerGameObjects)
            {
                var nearby = _grid.GetNearbyLights(player.transform.position, Mathf.CeilToInt(ActiveRange / CellSize));
                foreach (var light in nearby)
                {
                    if ((light.transform.position - player.transform.position).sqrMagnitude <= ActiveRange * ActiveRange)
                    {
                        lightsToEnable.Add(light);
                    }
                }
            }

            foreach (var light in allLights)
            {
                light.SetActive(lightsToEnable.Contains(light));
            }

            yield return new WaitForSeconds(CheckInterval);
        }
    }
}
