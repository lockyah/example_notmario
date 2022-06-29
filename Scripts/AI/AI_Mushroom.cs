using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Mushroom : EntityBase
{
    /*
     * Behaviour for Super and 1-Up Mushrooms.
     * 
     * Once instantiated, delays to allow the box animation to play, then "walks" back and forth between walls.
     * If Is1Up, grants an extra life. If not, gives Mario the Mushroom powerup, which gives him more health and the ability to break bricks.
     */

    [SerializeField] bool Is1Up = false;

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
            if(VertSpeed < 0)
            {
                VertSpeed = 0f;
            }

            if (CheckForWall())
            {
                MoveRight = !MoveRight;
            }
        } else
        {
            VertSpeed -= 20f * Time.deltaTime;
            VertSpeed = Mathf.Clamp(VertSpeed, -25f, 13f);
        }
        
        RB.velocity = new Vector2((HorizSpeed * HorizSpeedMultiplier) * (MoveRight ? 1 : -1), VertSpeed);
    }

    // Update is called once per frame
    void Update()
    {
        if (CanMove && OnScreen)
        {
            HandleMovement();

            MarioControl m = CheckForMario();
            if (m)
            {
                

                if (Is1Up)
                {
                    GameManager.GM.AddPoints(11, transform.position);
                } else
                {
                    m.GivePowerUp(0, false);
                    GameManager.GM.AddPoints(6, transform.position);
                }

                Destroy(gameObject);
            }
        }
    }
}
