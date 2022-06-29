using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : EntityBase
{
    /*
     * Behaviour for fireballs.
     * 
     * Bounces along the ground until it hits an enemy or wall.
     */

    void Start()
    {
        Ani = GetComponent<Animator>();
        RB = GetComponent<Rigidbody2D>();
        Coll = GetComponent<Collider2D>();
    }

    public void SetMovingRight(bool b)
    {
        GetComponent<SpriteRenderer>().flipX = !b;
        MoveRight = b;
    }

    void HandleMovement()
    {
        if (IsGrounded())
        {
            //Bounce!
            VertSpeed = 6f;
        }
        else
        {
            if (VertSpeed > 0 && IsHittingBlock())
            {
                VertSpeed = 0f;
            }
        }

        if(HorizSpeed != 0)
        {
            if (CheckForWall())
            {
                StartCoroutine(Burst(null));
            }

            //If still moving, change vert speed
            VertSpeed -= 20f * Time.deltaTime;
            VertSpeed = Mathf.Clamp(VertSpeed, -25f, 13f);
        }

        RB.velocity = new Vector2((HorizSpeed * HorizSpeedMultiplier) * (MoveRight ? 1 : -1), VertSpeed);
    }

    private void Update()
    {
        if (CanMove && OnScreen)
        {
            HandleMovement();

            if(HorizSpeed > 0)
            {
                RaycastHit2D[] r = Physics2D.CircleCastAll(transform.position, 0.25f, Vector2.right, 0.25f, LayerMask.GetMask("Enemies"));
                foreach (RaycastHit2D rh in r)
                {
                    if (rh.collider != null)
                    {
                        EnemyAI e = rh.collider.GetComponent<EnemyAI>();
                        if (e != null && e.GetType() != typeof(Fireball))
                        {
                            if(e.GetType() == typeof(AI_Plant))
                            {
                                //Ignore hidden plants
                                if ( ((AI_Plant)e).PlantIsUp )
                                {
                                    StartCoroutine(Burst(e));
                                }
                            } else
                            {
                                StartCoroutine(Burst(e));
                            }
                            
                            break;
                        }
                    }
                }
            }
        }
    }

    protected override bool CheckForWall()
    {
        if (OnScreen)
        {
            return Physics2D.Raycast(transform.position, new Vector2(WallDistance * (MoveRight ? 1 : -1), 0), WallDistance, LayerMask.GetMask("Ground"));
        }
        else
        {
            return false;
        }
    }
    private void OnDrawGizmos()
    {
        //Draw ray for the CheckForWall behaviour
        Debug.DrawRay(transform.position, new Vector2(WallDistance * (MoveRight ? 1 : -1), 0), Color.red);
    }

    IEnumerator Burst(EnemyAI enemy)
    {
        Ani.SetTrigger("Burst");
        Coll.enabled = false;

        RB.constraints = RigidbodyConstraints2D.FreezeAll; //Stop in place
        HorizSpeed = 0f;
        VertSpeed = 0f;

        if (enemy != null)
        {
            enemy.TakeDamage(true, 0);
        } else
        {
            GameManager.GM.PlaySound("Bump", false);
        }

        yield return new WaitForSeconds(0.6f);

        Destroy(gameObject);
    }
}
