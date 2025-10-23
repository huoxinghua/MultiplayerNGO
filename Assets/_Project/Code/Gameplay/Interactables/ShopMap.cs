using _Project.Code.Gameplay.Interactables;
using _Project.Code.Gameplay.Player.PlayerStateMachine;
using UnityEngine;

public class ShopMap : MonoBehaviour , IInteractable
{
    [SerializeField] private Camera _shopCamera;
    public void OnInteract(GameObject interactingPlayer)
    {
        if (interactingPlayer.GetComponent<PlayerStateMachine>() != null)
        {
            interactingPlayer.GetComponent<PlayerStateMachine>().HandleOpenMenu(true);
            _shopCamera.enabled = true;
            interactingPlayer.GetComponentInChildren<Camera>().enabled = false;
        }
    }
    public void HandleHover(bool isHovering)
    {

    }
}
