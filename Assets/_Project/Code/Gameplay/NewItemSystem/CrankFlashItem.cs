using System.Collections;
using _Project.Code.Gameplay.NPC.Violent.Brute;
using _Project.Code.Utilities.EventBus;
using _Project.ScriptableObjects.ScriptObjects.ItemSO.CrankFlashSO;
using UnityEngine;

namespace _Project.Code.Gameplay.NewItemSystem
{
    public class CrankFlashItem : FlashlightItem
    {
        private bool _isCracking;
        public override void SecondaryUse(bool isPerformed)
        {
            _isCracking = isPerformed;
        }
    
        private IEnumerator CrackingSoundBroadcast()
        {
            while (_isCracking)
            {
                yield return new WaitForSeconds(.35f);
                if(_itemSO is CrankFlashSO crankSO)
                {
                    EventBus.Instance.Publish<AlertingSound>(new AlertingSound { WasPlayerSound = true, SoundRange = crankSO.SoundRange, SoundSource = _owner.transform});
                }
          
            }
        }
    }
}
