using _Project.Code.Utilities.EventBus;
using Unity.Netcode;
using UnityEngine;

public class DayPast : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        EventBus.Instance.Publish<DayStartEvent>(new DayStartEvent());
    }
}
