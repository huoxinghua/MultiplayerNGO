using TMPro;
using UnityEngine;

public class BaseCartItem : MonoBehaviour
{
    [SerializeField] protected TMP_Text QuantityText;
    [SerializeField] protected ItemIds ItemIds;
    protected int _quantity;
    protected CartManager _cartManager;
    protected int CurrentPrice => StoreSO.GetItemData(ItemIds).Cost * _quantity;
    [SerializeField] protected StoreSO StoreSO;
    public ItemIds ThisItemId => ItemIds;
    public virtual void OnAddToCart(CartManager cartManager)
    {

        _cartManager = cartManager;
        _quantity = 1;
        QuantityText.SetText(_quantity.ToString());
        _cartManager.UpdateTotalText();
    }
    public virtual void HandleAddButton()
    {
        if (_quantity >= 10) return;
        _quantity++;
        QuantityText.SetText(_quantity.ToString());
        _cartManager.UpdateTotalText();
    }
    public virtual void HandleSubtractButton()
    {
        if (_quantity <= 1) return;
        _quantity--;
        QuantityText.SetText(_quantity.ToString());
        _cartManager.UpdateTotalText();
    }
    public virtual void HandleRemoveFromCart()
    {
        _cartManager.RemoveFromCart(this);
    }
    public virtual int GetCurrentPrice()
    {
        return CurrentPrice;
    }
}
