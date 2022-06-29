using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireFlower : EntityBase
{
    /*
     * Behaviour for Fire Flowers.
     * 
     * Flowers don't move, but still uses DelayStart for the box animation.
     * Gives the Flower powerup when collected, allowing Mario to shoot fireballs.
     */

    private void Start()
    {
        Ani = GetComponent<Animator>();
        RB = GetComponent<Rigidbody2D>();
        Coll = GetComponent<Collider2D>();

        Coll.enabled = false;
        StartCoroutine(DelayStart());
    }

    IEnumerator DelayStart()
    {
        yield return new WaitForSeconds(1f); //Wait for block animation to end
        transform.parent = null; //De-parent from the block it spawned from
        Coll.enabled = true;
    }

    void Update()
    {
        MarioControl m = CheckForMario();

        if(m != null)
        {
            m.GivePowerUp(1, false);
            GameManager.GM.AddPoints(6, transform.position);

            Destroy(gameObject);
        }
    }
}
