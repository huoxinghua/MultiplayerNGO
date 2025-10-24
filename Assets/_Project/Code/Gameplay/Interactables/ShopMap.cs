using _Project.Code.Gameplay.Interactables;
using _Project.Code.Gameplay.Player.PlayerStateMachine;
using UnityEngine;

public class ShopMap : MonoBehaviour , IInteractable
{
    [SerializeField] private Camera _shopCamera;
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private PlayerStateMachine _playerStateMachine;
    public void OnInteract(GameObject interactingPlayer)
    {
        if (interactingPlayer.GetComponent<PlayerStateMachine>() != null)
        {
            _playerStateMachine = interactingPlayer.GetComponent<PlayerStateMachine>();
            _playerStateMachine.HandleOpenMenu(true);
            _shopCamera.enabled = true;
            _playerCamera = interactingPlayer.GetComponentInChildren<Camera>();
            _playerCamera.enabled = false;
        }
    }
    public void OnClose()
    {
        _playerCamera.enabled = true;
        _shopCamera.enabled = false;
        _playerStateMachine.HandleOpenMenu(false);
    }

    public void HandleHover(bool isHovering)
    {

    }
}
