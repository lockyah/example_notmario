using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessPit : MonoBehaviour
{
    /*
     * Behaviour for the endless pits.
     * 
     * Calls the function on MarioControl to lose a life regardless of health when he's off-screen.
     */

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 6)
        {
            MarioControl m = collision.gameObject.GetComponent<MarioControl>();

            if (!m.OnScreen)
            {
                m.InstantDeath(true);
            }
        }
    }
}
