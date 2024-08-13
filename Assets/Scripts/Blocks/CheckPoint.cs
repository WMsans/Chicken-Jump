using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    private void Update()
    {
        if(transform.localScale.x < 1f)
        {
            transform.localScale += (Vector3.one - transform.localScale) * 0.3f;
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Set player checkpoint
            PlayerController.Instance.SetCheckPoint(transform.position);
            // Play animation
            transform.localScale = Vector3.one * 0.5f;

            Debug.Log("Saved");
        }
    }
}
