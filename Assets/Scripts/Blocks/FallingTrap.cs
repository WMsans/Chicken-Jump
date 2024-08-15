using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class FallingTrap : MonoBehaviour
{
    [SerializeField] HingeJoint2D trapJoint;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            trapJoint.enabled = false;
        }
    }
}
