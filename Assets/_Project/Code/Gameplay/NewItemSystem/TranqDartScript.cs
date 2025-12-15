using System;
using _Project.Code.Gameplay.Interfaces;
using Unity.Netcode;
using UnityEngine;

public class TranqDartScript : NetworkBehaviour
{
    public BoxCollider _dartCollider;
    private Rigidbody _rb;
    private float _damage = 0f;
    [SerializeField] private float _knockoutPower = 0f;
    public GameObject Owner { get; set; }

    private void Start()
    {
        _dartCollider = GetComponent<BoxCollider>();
        _rb = GetComponent<Rigidbody>();
    }
    
    public void SetVelocity(Vector3 direction, float speed)
    {
        if (_rb == null) _rb = GetComponent<Rigidbody>();
        _rb.linearVelocity = direction * speed;
    }

    public void SetDamage(float damage)
    {
        _damage = damage;
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
