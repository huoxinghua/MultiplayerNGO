using UnityEngine;

namespace _Project.Code.Gameplay.Interfaces
{
    public interface IHitable
    {
        public void OnHit(GameObject attacker, float damage, float knockoutPower);
    }
}
