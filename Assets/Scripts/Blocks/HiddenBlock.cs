using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class HiddenBlock : MonoBehaviour
{
    SpriteRenderer sr;
    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.enabled = false;
    }
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            ShowBlock();
        }
    }
    public void ShowBlock()
    {
        //activate the sr
        sr.enabled = true;
    }
}
