using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSO", menuName = "Scriptable Objects/PlayerSO")]
public class PlayerSO : ScriptableObject
{
    [Header("Move")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 1.5f;

    [Header("Jump")]
    public float jumpStrength = 2f;
    public float fallMultiplier = 4f;

    [Header("Crouch")]
    public float standHeight = 1f;
    public float crouchHeight = 0.5f;
}
