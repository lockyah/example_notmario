using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    /*
     * Behaviour for the Camera.
     * 
     * When "FollowMario" is enabled, the Camera will Lerp to his horizontal position if he passes the middle of the screen.
     * A collider attached to the left side of the camera prevents him backtracking.
     */

    public bool FollowMario;

    private void OnTriggerStay2D(Collider2D collision)
    {
        //If player is in trigger over half the screen, the camera needs to move to follow them!
        if(collision.gameObject.layer == 6 && collision.gameObject.transform.position.x > transform.position.x && FollowMario)
        {
            transform.position = new Vector3(Mathf.Lerp(transform.position.x, collision.transform.position.x, 0.25f), transform.position.y, -10);
        }
    }
}
