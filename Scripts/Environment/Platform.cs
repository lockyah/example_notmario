using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    /*
     * Behaviour for the moving platforms.
     * 
     * When an entity stands on top of it, the platform becomes its parent so the entity travels with it.
     * Can move horizontally or vertically in a set distance or looping across the camera (i.e. 1-2)
     */

    public bool OnScreen, MoveHorizontal;
    public float Distance;
    public float Speed;
    Vector3 TargetPosition;
    bool MovingForward;

    private void Start()
    {
        TargetPosition = transform.position + new Vector3(MoveHorizontal ? Distance : 0, !MoveHorizontal ? Distance : 0);
    }

    private void OnBecameVisible()
    {
        OnScreen = true;
    }

    private void OnBecameInvisible()
    {
        if (OnScreen && Time.timeScale != 0)
        {
            if (Distance != 0)
            {
                //Cull object
                Destroy(gameObject);
            } else
            {
                //Looping objects should check whether they need to loop or not

                float CameraHalfHeight = Camera.main.orthographicSize;
                float CameraHalfWidth = CameraHalfHeight * Camera.main.aspect;

                if (transform.position.x < Camera.main.transform.position.x - CameraHalfWidth && !MoveHorizontal)
                {
                    Destroy(gameObject);
                } else
                {
                    //De-parent anything stood on the platform.
                    if(transform.childCount > 0)
                    {
                        foreach(Transform t in transform)
                        {
                            t.parent = null;
                        }
                    }

                    //Displace the object to loop around to the other side of the screen
                    transform.position += new Vector3((MoveHorizontal ? (CameraHalfWidth * 2) + 3f : 0) * (Speed > 0 ? -1 : 1), (!MoveHorizontal ? (CameraHalfHeight * 2) + 0.5f : 0) * (Speed > 0 ? -1 : 1));
                }
            }
        }
        
    }

    private void OnDrawGizmosSelected()
    {
        Debug.DrawLine(transform.position, transform.position + new Vector3(MoveHorizontal ? Distance : 0, !MoveHorizontal ? Distance : 0));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.transform.position.y > transform.position.y)
        {
            collision.transform.parent = transform;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.parent == transform)
        {
            collision.transform.parent = null;
        }
    }

    private void Update()
    {
        if (OnScreen)
        {
            if(Distance != 0)
            {
                //Looping in a set distance

                if (Vector2.Distance(transform.position, TargetPosition) < 0.1f)
                {
                    TargetPosition = transform.position + (new Vector3(MoveHorizontal ? Distance : 0, !MoveHorizontal ? Distance : 0) * (MovingForward ? 1 : -1));
                    MovingForward = !MovingForward;
                }

                transform.position += new Vector3(MoveHorizontal ? Speed : 0, !MoveHorizontal ? -Speed : 0) * Time.deltaTime * (MovingForward ? 1 : -1);
            } else
            {
                //Scroll across the screen continually - resets to other side when off camera
                transform.position += new Vector3(MoveHorizontal ? Speed : 0, !MoveHorizontal ? Speed : 0) * Time.deltaTime;
            }
        }
    }
}
