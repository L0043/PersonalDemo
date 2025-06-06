using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncePad : MonoBehaviour
{
    [SerializeField] float BounceForce = 20000f;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.contacts.Length > 0)
        {
            // Check if the contact point is below the player
            foreach (ContactPoint contact in collision.contacts)
            {
                float dot = Vector3.Dot(contact.normal, transform.up);
                if (dot < 0f)
                {
                    // launch that hoe
                    var rb = collision.gameObject.GetComponent<Rigidbody>();
                    if (!rb)
                        return;
                    rb.velocity = Vector3.zero;
                    rb.AddForce(transform.up * BounceForce, ForceMode.Impulse);
                    var control = collision.gameObject.GetComponent<Controls>();
                    if (control) 
                    {
                        control.StopSlam();
                    }
                }
            }
        }
    }
}
