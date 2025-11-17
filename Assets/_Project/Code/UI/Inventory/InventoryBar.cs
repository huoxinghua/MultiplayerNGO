using _Project.Code.Gameplay.NewItemSystem;
using _Project.Code.Gameplay.Player.MiscPlayer;
using _Project.Code.Utilities.EventBus;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Code.UI.Inventory
{
    public class InventoryBar : MonoBehaviour
    {
        [field: SerializeField] public Image[] UIItemSlotImages { get; private set; } = new Image[5];
        [field: SerializeField] public Image[] UISlotBackgrounds { get; private set; } = new Image[5];
        [field: SerializeField] public InventoryUISO InventoryUISO { get; private set; }

        #region Setup

        private void Awake()
        {
            ChangeSlotBackgrounds(new InventorySlotIndexChangedEvent{NewIndex = 0});
            EventBus.Instance.Subscribe<InventorySlotIndexChangedEvent>(this, ChangeSlotBackgrounds);
            EventBus.Instance.Subscribe<InventoryListModifiedEvent>(this, ChangeUIItemDisplay);
        }

        private void OnDisable()
        {
            EventBus.Instance.Unsubscribe<InventorySlotIndexChangedEvent>(this);
            EventBus.Instance.Unsubscribe<InventoryListModifiedEvent>(this);
        }

        #endregion

        #region UI Change Logic

        public void ChangeUIItemDisplay(InventoryListModifiedEvent newInventoryItems)
        {
            for (int i = 0; i < UIItemSlotImages.Length; i++)
            {
                if (newInventoryItems.NewInventory[i] == null)
                {
                    UIItemSlotImages[i].sprite = null;
                    UIItemSlotImages[i].color = new Color(0, 0, 0, 0);
                }
                else
                {
                    UIItemSlotImages[i].sprite = newInventoryItems.NewInventory[i].GetUIImage();
                    UIItemSlotImages[i].color = Color.white;
                }
            }
        }

        public void ChangeSlotBackgrounds(InventorySlotIndexChangedEvent slotIndexChangedEvent)
        {
            for (int i = 0; i < UISlotBackgrounds.Length; i++)
            {
                if (i == slotIndexChangedEvent.NewIndex)
                {
                    UISlotBackgrounds[i].color = InventoryUISO.SelectedBackgroundColor;
                }
                else
                {
                    UISlotBackgrounds[i].color = InventoryUISO.NonSelectedBackgroundColor;
                }
            }
        }

        #endregion
    }

    #region IEvents

    public struct InventorySlotIndexChangedEvent : IEvent
    {
        public int NewIndex;
    }

    public struct InventoryListModifiedEvent : IEvent
    {
        public BaseInventoryItem[] NewInventory;
    }

    public struct ItemSoldEvent : IEvent
    {
        public ScienceData SoldItemData;
    }

    #endregion
}