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
                if (dot < -0.9f)
                {
                    // launch that hoe
                    var rb = collision.gameObject.GetComponent<Rigidbody>();
                    rb.AddForce(transform.up * BounceForce, ForceMode.Impulse);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawLine(transform.position, transform.position + transform.up * 10f);
    }

}
