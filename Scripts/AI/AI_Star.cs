using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Star : EntityBase
{
    /*
     * Behaviour for the Invincibility Star.
     * 
     * Once instantiated, it delays to allow animation, then jumps regularly and bounces from walls.
     * Gives Mario 10 seconds of invincibility.
     */

    private void Start()
    {
        Ani = GetComponent<Animator>();
        RB = GetComponent<Rigidbody2D>();
        Coll = GetComponent<Collider2D>();

        CanMove = false;
        Coll.enabled = false;
        StartCoroutine(DelayStart());
    }

    IEnumerator DelayStart()
    {
        yield return new WaitForSeconds(1f); //Wait for block animation to end
        transform.parent = null; //De-parent from the block it spawned from
        CanMove = true;
        Coll.enabled = true;
    }

    void HandleMovement()
    {
        if (IsGrounded())
        {
            //Bounce!
            VertSpeed = 2.5f;
        } else {
            if(VertSpeed > 0 && IsHittingBlock())
            {
                VertSpeed = 0f;
            }
        }

        if (CheckForWall())
        {
            MoveRight = !MoveRight;
        }

        VertSpeed -= Time.deltaTime * 2f;
        RB.velocity = new Vector2((HorizSpeed * HorizSpeedMultiplier) * (MoveRight ? 1 : -1), VertSpeed);
    }

    private void Update()
    {
        if (CanMove && OnScreen)
        {
            HandleMovement();

            MarioControl m = CheckForMario();
            if (m)
            {
                GameManager.GM.AddPoints(6, transform.position);
                m.GivePowerUp(2, false);

                Destroy(gameObject);
            }
        }
    }
}
