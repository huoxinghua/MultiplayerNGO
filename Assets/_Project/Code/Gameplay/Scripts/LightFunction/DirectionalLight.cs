using _Project.Code.Core.Patterns;
using _Project.Code.Gameplay.Player.MiscPlayer;
using _Project.Code.Utilities.EventBus;
using _Project.Code.Utilities.Utility;
using Unity.Netcode;
using UnityEngine;

public class DirectionalLight : MonoBehaviour
{
    private Light _light;
    private void Awake()
    {
        _light = GetComponent<Light>();
        EventBus.Instance.Subscribe<OnEnterHospitalEvent>(this, DisableSun);
        EventBus.Instance.Subscribe<OnExitHospitalEvent>(this, EnableSun);
    }

    public void DisableSun(OnEnterHospitalEvent e)
    {
        _light.enabled = false;
    }

    public void EnableSun(OnExitHospitalEvent e)
    {
        _light.enabled = true;
    }

}
