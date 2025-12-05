using _Project.Code.Core.Patterns;
using _Project.Code.Gameplay.Player.MiscPlayer;
using _Project.Code.Utilities.EventBus;
using _Project.Code.Utilities.Utility;
using Unity.Netcode;
using UnityEngine;

public class DirectionalLightChange : NetworkBehaviour
{

    // Kyle you mf. This is attached to any and al camera, OK?!? Publish event when below y value that you set. Publish event when above. 
    //Only publish when you change, not every frame you are above or below. Use Transform.position.y. Variable for which y is the middle of above below

    public Transform playerTransform;
    private float _yThreshold = -5;
    private float _checkFrequency = .2f;
    private bool _inInterior = false;
    private bool _hasInit = false;
    private Timer _checkTimer;
    private void Awake()
    {
       
        
    }
    public override void OnNetworkSpawn()
    {

        base.OnNetworkSpawn();
        if (!IsOwner)
        {
            this.enabled = false;
            Debug.Log("Owner?");
            return;
        }
        if (_hasInit) return;
        _hasInit = true;
        
        _checkTimer = new Timer(_checkFrequency);
        _checkTimer.Start();
    }
    private void OnEnable()
    {
        if (_hasInit) return;
        _hasInit = true;
        _checkTimer = new Timer(_checkFrequency);
        _checkTimer.Start();
    }

    public void EnterHospital()
    {
            EventBus.Instance.Publish<OnEnterHospitalEvent>(new OnEnterHospitalEvent());
            _inInterior = true;
    }

    public void ExitHospital()
    {
            EventBus.Instance.Publish<OnExitHospitalEvent>(new OnExitHospitalEvent());
            _inInterior = false;
        
    }
    private void CheckHeight()
    {
        if (playerTransform.position.y >= _yThreshold)
        {
            if (_inInterior)
            {
                ExitHospital();
            }
        }
        else
        {
            if (!_inInterior)
            {
                EnterHospital();
            }
        }
    }



    void FixedUpdate()
    {
        _checkTimer.TimerUpdate(Time.deltaTime);
        if (_checkTimer.IsComplete)
        {
            CheckHeight();
            _checkTimer.Reset();
        }
    }
}
public struct OnEnterHospitalEvent : IEvent
{

}
public struct OnExitHospitalEvent : IEvent
{

}