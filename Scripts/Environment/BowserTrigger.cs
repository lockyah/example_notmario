using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowserTrigger : MonoBehaviour
{
    /*
     * Activates Bowser's distant fire attacks and stops the camera advancing past the Axe.
     */

    AI_Bowser Bowser;
    public Vector2 SecondPosition;

    private void Start()
    {
        Bowser = transform.parent.GetComponent<AI_Bowser>();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawLine(SecondPosition, SecondPosition + new Vector2(0, 1f));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == 6)
        {
            if (!Bowser.CanAct)
            {
                //Bowser activates slightly before he's actually on-screen to shoot fireballs at Mario.
                Bowser.CanAct = true;

                transform.position = SecondPosition;
                transform.parent = null; //Deparent to remain in place
            }
            else
            {
                //When triggered again, Bowser stops the camera scrolling past the axe.
                Camera.main.GetComponent<CameraFollow>().FollowMario = false;
                Destroy(gameObject);
            }
        }
    }
}
