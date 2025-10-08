using System;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    [SerializeField] private float distanceThreshold = .15f;
    private float _timeSinceChange;
    public event Action<bool> OnGroundedChanged;
    [SerializeField] private float _changeDelay = .01f;
    [SerializeField] private Transform _checkLocation;
    public bool IsGrounded
    {
        get
        {
            return _isGrounded;
        } private set
        {
            if (value != _isGrounded)
            {
                _timeSinceChange = 0f;
                OnGroundedChanged?.Invoke(_isGrounded);
            }
            _isGrounded = value;
        }
    } 

    private bool _isGrounded;
    public event Action Grounded;
    const float OriginOffset = .001f;
    public float CoyoteTime => !_isGrounded? _timeSinceChange: 0.0f;
    private Vector3 RaycastOrigin => _checkLocation.position;//transform.position + Vector3.up * OriginOffset;
    float RaycastDistance => distanceThreshold + OriginOffset;

    void LateUpdate()
    {
        bool isGroundedNow = Physics.Raycast(RaycastOrigin, Vector3.down, distanceThreshold * 2);
        _timeSinceChange += Time.deltaTime;
        if (_timeSinceChange < _changeDelay) return;
        IsGrounded = isGroundedNow;
        //OnDrawGizmosSelected();
    }
    void OnDrawGizmosSelected()
    {
        Debug.DrawLine(RaycastOrigin, RaycastOrigin + Vector3.down * RaycastDistance, IsGrounded ? Color.white : Color.red);
    }
}
