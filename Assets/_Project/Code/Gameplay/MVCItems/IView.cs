using UnityEngine;

namespace _Project.Code.Gameplay.MVCItems
{
    public interface IView
    {
        GameObject GetCurrentVisual();
        void SetVisible(bool visible);
        void SetPhysicsEnabled(bool enabled);
        void MoveToPosition(Vector3 position);
        void SetLightEnabled(bool on);
        void DisplayHeld(Transform position);
        public void DestroyHeldVisual();
    }
}
