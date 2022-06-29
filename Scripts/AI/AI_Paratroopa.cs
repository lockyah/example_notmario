using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Paratroopa : AI_Koopa
{
    /*
     * AI for Paratroopas (Flying Koopas.)
     * 
     * Jumps in midair until stomped, where it reverts to Red Koopa behaviour.
     */

    bool HasWings = true;
    float FlapTimer = 0.5f;

    private void Start()
    {
        Ani = GetComponent<Animator>();
        RB = GetComponent<Rigidbody2D>();
        Coll = GetComponent<Collider2D>();

        CanMove = false;
        Ani.SetInteger("EnemyType", (int)type);
    }


    private void Update()
    {
        if (OnScreen)
        {
            CanMove = true;
        }

        if (CanMove)
        {
            if (HasWings)
            {
                if (FlapTimer <= 0)
                {
                    VertSpeed = 10f;
                    FlapTimer = 1f;
                    Ani.SetTrigger("FlapWings");
                }
                else
                {
                    FlapTimer -= Time.deltaTime;
                }
            }
            else
            {
                //Regular Koopa behaviour

                if (StompCooldown > 0f)
                {
                    StompCooldown -= Time.deltaTime;
                }
                else
                {
                    CanBeStomped = true;
                }
            }

            HandleMovement();
            HandleHitDetection();

            Ani.SetFloat("HorizSpeed", HorizSpeed * (MoveRight ? 1f : -1f));
            Ani.SetBool("InShell", InShell);

        }
    }

    public override void TakeDamage(bool HitByTool, int comboModifier)
    {
        //If hit by a tool, it's instant death!
        //Otherwise, check if the Koopa is in its shell already. If hit by a tool, it dies, but if not, it begins moving.

        if (HitByTool)
        {
            Health -= 1; //Can't take more than one hit from fireballs/shells
            HasWings = false; //Disable flying
            GameManager.GM.AddPoints(1 + comboModifier, transform.position);
            StartCoroutine(DeathAnimation(HitByTool));
        }
        else
        {
            CanBeStomped = false;
            StompCooldown = 0.3f;

            if (HasWings)
            {
                HasWings = false;
                type = EnemyType.Red;
                TurnAtPits = true;
                Ani.SetInteger("EnemyType", (int)type);
            } else
            {
                if (!InShell)
                {
                    InShell = true;
                    HorizSpeedMultiplier = 0f;
                    GameManager.GM.PlaySound("Squish", false);
                }
                else
                {
                    GameManager.GM.PlaySound("Kick", false);

                    if (HorizSpeedMultiplier == 0f)
                    {
                        //Fire!
                        HorizSpeedMultiplier = 3f;
                    }
                    else
                    {
                        //Stops if stomped on again
                        HorizSpeedMultiplier = 0f;
                    }
                }
            }

            GameManager.GM.AddPoints(Mathf.Clamp(1 + comboModifier, 0, 11), transform.position);
        }
    }
}
