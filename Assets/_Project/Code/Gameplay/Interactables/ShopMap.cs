using _Project.Code.Gameplay.Interactables;
using _Project.Code.Gameplay.Player.PlayerStateMachine;
using QuickOutline.Scripts;
using UnityEngine;

[RequireComponent(typeof(Outline))]
public class ShopMap : MonoBehaviour , IInteractable
{
    [SerializeField] private Camera _shopCamera;
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private PlayerStateMachine _playerStateMachine;
    protected Outline OutlineEffect;
    public void Awake()
    {
        OutlineEffect = GetComponent<Outline>();
        if (OutlineEffect != null)
        {
            OutlineEffect.OutlineMode = Outline.Mode.OutlineHidden;
            OutlineEffect.OutlineWidth = 0;
        }
    }
    public void OnInteract(GameObject interactingPlayer)
    {
        if (interactingPlayer.GetComponent<PlayerStateMachine>() != null)
        {
            _playerStateMachine = interactingPlayer.GetComponent<PlayerStateMachine>();
            _playerStateMachine.HandleOpenMenu(true);
            _shopCamera.enabled = true;
            _playerCamera = interactingPlayer.GetComponentInChildren<Camera>();
            _playerCamera.enabled = false;
            HandleHover(false);
        }
    }
    public void OnClose()
    {
        _playerCamera.enabled = true;
        _shopCamera.enabled = false;
        _playerStateMachine.HandleOpenMenu(false);
        _playerStateMachine = null;
        _playerCamera = null;
    }

    public void HandleHover(bool isHovering)
    {
        if (OutlineEffect != null)
        {
            if (_playerCamera != null) { OutlineEffect.OutlineMode = Outline.Mode.OutlineHidden; return; }
            if (isHovering)
            {
                OutlineEffect.OutlineMode = Outline.Mode.OutlineVisible;
                OutlineEffect.OutlineWidth = 2;
            }
            else
            {
                OutlineEffect.OutlineMode = Outline.Mode.OutlineHidden;
                OutlineEffect.OutlineWidth = 0;
            }
        }
    }
}
