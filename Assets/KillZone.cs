using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillZone : MonoBehaviour
{
    BoxCollider BoxCollider;

    private void Awake()
    {
        BoxCollider = GetComponent<BoxCollider>();
        if (BoxCollider == null)
        {
            BoxCollider = gameObject.AddComponent<BoxCollider>();
        }
        BoxCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == GameManager.Instance.Player)
            GameManager.Instance.ResetPlayerPosition();
    }

}
