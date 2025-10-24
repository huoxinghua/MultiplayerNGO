using _Project.Code.Gameplay.Interactables;
using _Project.Code.Gameplay.NewItemSystem;
using _Project.Code.Gameplay.Player.RefactorInventory;
using _Project.Code.Utilities.ServiceLocator;
using QuickOutline.Scripts;
using UnityEngine;

namespace _Project.Code.Gameplay.Market.Sell
{
    [RequireComponent(typeof(Outline))]
    public class ResearchDeposit : MonoBehaviour, IInteractable
    {
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
            PlayerInventory playerInventory = interactingPlayer.GetComponent<PlayerInventory>();
            if(playerInventory != null)
            {
                if (playerInventory.IsHoldingSample())
                {
                    if (!ServiceLocator.TryGet<EconomyManager>(out var economy))
                    {
                        Debug.Log("Failed");
                    }
                    else
                    {
                        var economyManager = ServiceLocator.Get<EconomyManager>();
                        ScienceData scienceData = playerInventory.TrySell();
                        SampleMarketValue itemValues = economyManager.GetMarketValue(scienceData);
                        economyManager.SoldItem(itemValues);
                        Debug.Log($"current  money {economyManager.PlayerMoney} - current research progress {economyManager.ResearchProgress}");
                    }
                
                }
            }
      
        }
        public void HandleHover(bool isHovering)
        {
            if (OutlineEffect != null)
            {

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
}
