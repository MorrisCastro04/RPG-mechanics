using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEvents : MonoBehaviour
{
    PlayerMotion playerMotion;
    void Awake()
    {
        playerMotion = GetComponentInParent<PlayerMotion>();
    }

    /// <summary>
    /// Called when the player Lands.
    /// </summary>
    public void Land()
    {
        playerMotion.FallEnd();
    }
}
