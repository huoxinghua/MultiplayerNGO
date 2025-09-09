using UnityEngine;

public interface IView
{
    void SetVisible(bool visible);
    void SetPhysicsEnabled(bool enabled);
    void MoveToPosition(Vector3 position);
    void SetLightEnabled(bool on);
}
