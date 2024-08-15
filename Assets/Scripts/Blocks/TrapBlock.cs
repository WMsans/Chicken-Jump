using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TrapBlock : MonoBehaviour
{
    Vector2 saveVelocity;
    bool checkingX = false;
    float rot;
    float rotDiff;
    private void Start()
    {
        // Find save velocity
        checkingX = false;
         rot = transform.rotation.z * 180;
         rotDiff = Mathf.DeltaAngle(rot, 0);
        if (rotDiff < 45) saveVelocity = new(0, 1);
        else if (rotDiff < 135)
        {
            if(rotDiff < 0) saveVelocity = new(-1 ,0);
            else saveVelocity = new(1,0);
            checkingX = true;
        }
        else saveVelocity = new(0, -1);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        var other = collision.gameObject;
        if (other.CompareTag("Player"))
        {
            if (!checkingX)
            {
                if (Mathf.Sign( other.GetComponent<Rigidbody2D>().velocity.y) != saveVelocity.y)
                    other.GetComponent<PlayerController>().Die();
            }
            else
            {
                if (Mathf.Sign(other.GetComponent<Rigidbody2D>().velocity.x) != saveVelocity.x)
                    other.GetComponent<PlayerController>().Die();
            }

        }
    }
}
