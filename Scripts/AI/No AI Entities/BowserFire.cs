using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowserFire : EntityBase
{
    /*
     * Behaviour for Bowser's fireballs.
     * 
     * Travels in a straight line until it hits a block, Mario, or falls offscreen.
     */

    private void Update()
    {
        RB.velocity = new Vector2(HorizSpeed, 0f);

        MarioControl m = CheckForMario();
        if (m != null && Vector2.Distance(m.transform.position + new Vector3(0, 1f), transform.position) <= 1f)
        {
            StartCoroutine(Burst(m));
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        StartCoroutine(Burst(null));
    }

    IEnumerator Burst(MarioControl m)
    {
        Ani.SetTrigger("Burst");
        Coll.enabled = false;
        HorizSpeed = 0f;
        VertSpeed = 0f;

        if (m != null)
        {
            m.TakeDamage();
        }
        else
        {
            GameManager.GM.PlaySound("Bump", false);
        }

        yield return new WaitForSeconds(0.6f);

        Destroy(gameObject);
    }
}
