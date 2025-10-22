using _Project.Code.Core.ServiceLocator;
using UnityEngine;

public class ResearchDeposit : MonoBehaviour, IInteractable
{
    
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
}
