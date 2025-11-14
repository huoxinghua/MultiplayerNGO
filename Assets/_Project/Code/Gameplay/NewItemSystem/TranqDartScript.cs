using System;
using UnityEngine;

public class TranqDartScript : MonoBehaviour
{
    public float _dartSpeed = 1.0f;
    public BoxCollider _dartCollider;
    public Rigidbody rb;

    private void Start()
    {
        _dartCollider = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        rb.linearVelocity = (transform.forward * _dartSpeed) * Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }
}
