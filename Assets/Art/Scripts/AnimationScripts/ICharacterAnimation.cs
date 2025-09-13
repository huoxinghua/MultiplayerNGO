using UnityEngine;

public interface ICharacterAnimation
{
    public void OnIdle(float idleDuration);
    public void OnWalk(float currentSpeed, float maxSpeed);
    public void OnRun(float currentSpeed, float maxSpeed);
    public void OnJump();
    public void OnAttack();
}
