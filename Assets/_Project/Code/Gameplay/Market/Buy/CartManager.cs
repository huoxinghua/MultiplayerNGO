using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CartManager : MonoBehaviour
{
    [SerializeField] private StoreSO StoreSO;
    [SerializeField] private Transform CartSorter;
    private List<BaseCartItem> CurrentItemsInCart = new List<BaseCartItem>();
    [SerializeField] private TMP_Text _cartTotal;
    public void AddToCart(ItemIds itemID)
    {
        bool isInList = false;
        foreach(var item in CurrentItemsInCart)
        {
            if(item.ThisItemId == itemID)
            {
                isInList = true;
                item.HandleAddButton();
            }
        }
        if (isInList) return;
        BaseCartItem temp = Instantiate(StoreSO.GetItemData(itemID).ItemPrefab, CartSorter);
        temp.transform.parent = CartSorter;
        CurrentItemsInCart.Add(temp);
        temp.OnAddToCart(this);
        

    }
    public void OnEnable()
    {
        UpdateTotalText();
    }
    public void UpdateTotalText()
    {
        int newTotal = 0;
        foreach(var item in CurrentItemsInCart)
        {
            newTotal += item.GetCurrentPrice();
        }
        _cartTotal.SetText($"Total: @{newTotal}");
        if(newTotal > WalletBankton.Instance.TotalMoney)
        {
            _cartTotal.color = Color.red;
        }
        else
        {
            _cartTotal.color = Color.black;
        }
    }
    public void RemoveFromCart(BaseCartItem item)
    {
        CurrentItemsInCart?.Remove(item);
        Destroy(item.gameObject);
        UpdateTotalText();
    }
}
