using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
namespace _Project.Code.Gameplay.NewItemSystem
{
    public interface IInventoryItem 
    {
        public void PickupItem(GameObject player, Transform playerHoldPosition, NetworkObject networkObject);
        public void DropItem(Transform dropPoint);
        public void UseItem();
        public void UnequipItem();
        public void EquipItem();
        public string GetItemName();
        public bool IsPocketSize();
        public GameObject GetHeldVisual();
        public Image GetUIImage();
        public bool CanBeSold();
        public void WasSold();
        //change to raw value struct
        public ScienceData GetValueStruct();
     
    }
    public struct ScienceData
    {
        public string KeyName;
        public float RawTranquilValue;
        public float RawViolentValue;
        public float RawMiscValue;

    }
}