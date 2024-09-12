using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class FallingTrap : MonoBehaviour, IDynamicSceneObjects
{
    [SerializeField] HingeJoint2D trapJoint;
    [SerializeField] Transform trapTransform;
    Vector2 oriPosition;

    private void Start()
    {
        oriPosition = trapTransform.position;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            trapJoint.enabled = false;
        }
    }

    public void Restore()
    {
        trapJoint.enabled = true;
        trapTransform.position = oriPosition;
    }
}
