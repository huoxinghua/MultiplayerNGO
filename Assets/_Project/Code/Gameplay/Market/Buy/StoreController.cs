using _Project.Code.Gameplay.FirstPersonController;
using _Project.Code.Utilities.EventBus;
using TMPro;
using UnityEngine;

public class StoreController : MonoBehaviour
{
    [SerializeField] private CartManager _cartManager;
    [SerializeField] private TMP_Text _walletMoney;
    public void OnEnable()
    {
        EventBus.Instance.Subscribe<WalletUpdate>(this, HandleWalletUpdateEvent);
        UpdateCashText();
    }
    public void OnDisable()
    {
        EventBus.Instance.Unsubscribe<WalletUpdate>(this);
    }
    public void HandleWalletUpdateEvent(WalletUpdate wallet)
    {
        UpdateCashText();
    }
    public void UpdateCashText()
    {
        _walletMoney.SetText($"Cash: @{WalletBankton.Instance.TotalMoney}");
    }
    public void HandleStoreClicked(int itemID)
    {
        _cartManager.AddToCart((ItemIds)itemID);
    }
}
public struct WalletUpdate : IEvent
{

}