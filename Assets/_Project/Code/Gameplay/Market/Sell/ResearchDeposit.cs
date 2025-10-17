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
                var economyManager = ServiceLocator.Get<EconomyManager>();
                ScienceData scienceData = playerInventory.TrySell(); 
                Debug.Log($"Tranquil Value {scienceData.RawTranquilValue} -> {economyManager.GetTranquilMarketValue(scienceData.RawTranquilValue,scienceData.KeyName)}");
            }
        }
      
    }
}
