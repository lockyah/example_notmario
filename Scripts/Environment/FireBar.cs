using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBar : MonoBehaviour
{
    /*
     * Behaviour for the long fireball obstacles in the Castle level.
     * 
     * Slowly rotates in the set direction once onscreen. Damages Mario on contact.
     */

    Transform BarObject;
    bool Active;

    public bool Clockwise;
    [SerializeField] float StartAngle;

    void Start()
    {
        BarObject = transform.GetChild(0);
    }

    private void OnDrawGizmos()
    {
        transform.GetChild(0).rotation = Quaternion.Euler(0f, 0f, StartAngle);
    }

    private void OnBecameVisible()
    {
        Active = true;
    }

    private void Update()
    {
        if (Active)
        {
            BarObject.rotation = Quaternion.Euler(0f, 0f, BarObject.rotation.eulerAngles.z + Time.deltaTime * 50 * (Clockwise ? -1 : 1));
            foreach (Transform t in BarObject.transform)
            {
                t.rotation = Quaternion.Euler(0f, 0f, -BarObject.rotation.z);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == 6)
        {
            collision.GetComponent<MarioControl>().TakeDamage();
        }
    }

}
