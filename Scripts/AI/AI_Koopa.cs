using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Koopa : EnemyAI
{
    /*
     * Behaviour for Koopas and Red Koopas.
     * 
     * Patrols until a wall is found. Green Koopas will fall into pits, Red Koopas will not.
     * Parent class of Paratroopas.
     */

    protected bool TurnAtPits, InShell = false;
    protected float StompCooldown = 0f;
    protected int ShellCombo = 0;

    private void Start()
    {
        Ani = GetComponent<Animator>();
        RB = GetComponent<Rigidbody2D>();
        Coll = GetComponent<Collider2D>();

        CanMove = false;
        TurnAtPits = type == EnemyType.Red;
        Ani.SetInteger("EnemyType", (int)type);
    }

    private void Update()
    {
        if (OnScreen)
        {
            //Koopa should keep moving even if offscreen once activated
            CanMove = true;
        }

        if(StompCooldown > 0f)
        {
            StompCooldown -= Time.deltaTime;
        } else
        {
            CanBeStomped = true;
        }

        if (CanMove)
        {
            HandleMovement();
            HandleHitDetection();

            Ani.SetFloat("HorizSpeed", HorizSpeed * (MoveRight ? 1f : -1f));
            Ani.SetBool("InShell", InShell);

        }
    }

    protected void HandleMovement()
    {
        if (Health > 0)
        {
            if (IsGrounded())
            {
                if (VertSpeed < 0)
                {
                    VertSpeed = 0f;
                }

                //If wall is found or a pit, turn around if not in shell
                if (CheckForWall() || (!Physics2D.Raycast(transform.position + new Vector3(0.5f * (MoveRight ? 1f : -1f), 0f),Vector2.down * 2f) && TurnAtPits && !InShell))
                {
                    MoveRight = !MoveRight;
                }
            }
            else
            {
                VertSpeed -= 20f * Time.deltaTime;
                VertSpeed = Mathf.Clamp(VertSpeed, -25f, 13f);
            }
        }

        RB.velocity = new Vector2((HorizSpeed * HorizSpeedMultiplier) * (MoveRight ? 1 : -1), VertSpeed);
    }

    protected void HandleHitDetection()
    {
        if (InShell && HorizSpeedMultiplier > 1f)
        {
            //Shell is on the move!
            RaycastHit2D[] r = Physics2D.RaycastAll(transform.position + new Vector3(0, 0.5f), Vector2.right * WallDistance * (MoveRight ? 1 : -1), WallDistance);
            foreach(RaycastHit2D rh in r)
            {
                if (rh.collider != null && rh.collider.gameObject != gameObject)
                {
                    //Check if something ahead is an enemy (7) or the ground (3)
                    if (rh.collider.gameObject.layer == 7)
                    {
                        //Shells count as a tool, so a different animation is used.
                        rh.collider.gameObject.GetComponent<EnemyAI>().TakeDamage(true, ShellCombo);
                        ShellCombo++;
                    } else if (rh.collider.gameObject.layer == 3)
                    {
                        GameManager.GM.PlaySound("Bump", false);
                        MoveRight = !MoveRight;              
                    }
                }
            }
            
        }

        MarioControl m = CheckForMario();
        if (m != null && Health > 0)
        {
            if (Vector3.Distance(m.transform.position, transform.position) <= 1f)
            {
                if (m.transform.position.y <= transform.position.y + 0.5f)
                {
                    if (!m.HasStar && StompCooldown <= 0)
                    {
                        m.TakeDamage();
                    }
                    else if(StompCooldown <= 0)
                    {
                        TakeDamage(true, m.BounceCombo);
                        m.BounceCombo++;
                    }
                }
                else if (CanBeStomped)
                {
                    MoveRight = m.transform.position.x < transform.position.x; //Fire in the opposite direction of where Mario jumped onto it

                    TakeDamage(false, m.BounceCombo);
                    Instantiate(Resources.Load<GameObject>("Effects/Stomp"), m.transform.position, Quaternion.Euler(new Vector3(-90, 0, 0)));
                    m.ForceJump();
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Debug.DrawRay(transform.position + new Vector3(0, 0.5f), new Vector2(WallDistance * (MoveRight ? 1 : -1), 0), Color.red);
        Debug.DrawRay(transform.position + new Vector3(0.5f * (MoveRight ? 1f : -1f), 0f), Vector2.down * 2f, Color.blue);
    }

    public override void TakeDamage(bool HitByTool, int comboModifier)
    {
        //If hit by a tool, it's instant death!
        //Otherwise, check if the Koopa is in its shell already. If hit by a tool, it dies, but if not, it begins moving.

        if (HitByTool)
        {
            Health -= 1; //Can't take more than one hit from fireballs/shells
            GameManager.GM.AddPoints(1 + comboModifier, transform.position);
            StartCoroutine(DeathAnimation(HitByTool));
        }
        else
        {
            CanBeStomped = false;
            StompCooldown = 0.3f;

            GameManager.GM.AddPoints(Mathf.Clamp(1 + comboModifier, 0, 11), transform.position);

            if (!InShell)
            {
                InShell = true;
                HorizSpeedMultiplier = 0f;
                GameManager.GM.PlaySound("Squish", false);
            } else
            {
                GameManager.GM.PlaySound("Kick", false);

                if (HorizSpeedMultiplier == 0f)
                {
                    //Fire!
                    HorizSpeedMultiplier = 3f;
                } else
                {
                    //Stops if stomped on again
                    HorizSpeedMultiplier = 0f;
                }
            }
        }
    }
}
