using System;
using _Project.Code.Gameplay.Interfaces;
using Unity.Netcode;
using UnityEngine;

public class TranqDartScript : NetworkBehaviour
{
    public float _dartSpeed = 1.0f;
    public BoxCollider _dartCollider;
    private Rigidbody _rb;
    [SerializeField] private float _damage = 100f;
    [SerializeField] private float _knockoutPower = 0f;
    public GameObject Owner { get; set; }

    private void Start()
    {
        _dartCollider = GetComponent<BoxCollider>();
        _rb = GetComponent<Rigidbody>();
    }
    
    public void SetVelocity(Vector3 direction)
    {
        if (_rb == null) _rb = GetComponent<Rigidbody>();
        _rb.linearVelocity = direction * _dartSpeed;
    }
    

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;
        var hitable = collision.collider.GetComponent<IHitable>();
        if (hitable != null)
        {
            hitable.OnHit(Owner,_damage,_knockoutPower);
        }
        
        NetworkObject.Despawn();
    }
}
