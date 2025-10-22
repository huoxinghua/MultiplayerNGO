using UnityEngine;

namespace _Project.Code.Gameplay.FirstPersonController
{
    [CreateAssetMenu(fileName = "PlayerSO", menuName = "Scriptable Objects/PlayerSO")]
    public class PlayerSO : ScriptableObject
    {
        [field: Header("Move")]
        [field: SerializeField] public float MoveSpeed {get; private set; }
        [field: SerializeField] public float CrouchMoveMultiplier { get; private set; }
        [field: SerializeField] public float SprintMultiplier { get; private set; }
        [field: Header("Noise Ranges")]
        [field: SerializeField] public float WalkSoundRange {  get; private set; }
        [field: SerializeField] public float SprintSoundRange { get; private set; }
        [field: SerializeField] public float CrouchSoundRange { get; private set; }
        [field: SerializeField] public float LandingSoundRange { get; private set; }

        [field: Header("Jump")]
        [field: SerializeField] public float JumpStrength { get; private set; }
        [field: SerializeField] public float FallMultiplier { get; private set; }
        [field: SerializeField] public float PlayerGravity {  get; private set; }
        [field: SerializeField] public float CoyoteTime { get; private set; }
        [field: SerializeField] public float AirSpeedMult { get; private set; }

        [field: Header("Crouch")]
        [field: SerializeField] public float StandHeight {get; private set;}
        [field: SerializeField] public float CrouchHeight { get; private set; }

        [field: Header("Player Camera")]
        [field: SerializeField] public float StandingCameraHeight { get; private set; }
        [field: SerializeField] public float CrouchingCameraHeight { get; private set; }
        [field: SerializeField] public float CameraTransitionSpeed { get; private set; }
    }
}
