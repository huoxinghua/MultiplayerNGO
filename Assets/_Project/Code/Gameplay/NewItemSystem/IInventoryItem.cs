using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
namespace _Project.Code.Gameplay.NewItemSystem
{
    public interface IInventoryItem
    {
        public void PickupItem(GameObject player, Transform fpsItemParent, Transform tpsItemParent, NetworkObject networkObject);
        public void DropItem(Transform dropPoint);
        public bool TryUse();
        public void UnequipItem();
        public void EquipItem();
        public string GetItemName();
        public bool IsPocketSize();
        public GameObject GetHeldVisual();
        public Sprite GetUIImage();
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