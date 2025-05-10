using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : MonoBehaviour
{
    
    
    public static UnityEvent PlayerPrefsUpdated = new();
    public static UnityEvent OnDash = new();
    public static UnityEvent OnTeleport = new();


}
