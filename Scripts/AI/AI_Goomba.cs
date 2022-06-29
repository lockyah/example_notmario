using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Goomba : EnemyAI
{
    /*
     * Behaviour for Goombas.
     * 
     * Walks back and forth between walls. Will walk off edges.
     */

    private void Update()
    {
        if (OnScreen)
        {
            HandleMovement();

            MarioControl m = CheckForMario();
            if (m != null)
            {
                if(Vector3.Distance(m.transform.position, transform.position) <= 1f)
                {
                    if (m.transform.position.y <= transform.position.y + 0.5f)
                    {
                        //Whether Mario takes damage or the Goomba depends on his invincibility.
                        if(!m.HasStar)
                        {
                            m.TakeDamage();
                        } else
                        {
                            TakeDamage(true, m.BounceCombo);
                            m.BounceCombo++;
                        }
                    }
                    else
                    {
                        TakeDamage(false, m.BounceCombo);
                        Instantiate(Resources.Load<GameObject>("Effects/Stomp"), m.transform.position, Quaternion.Euler(new Vector3(-90, 0, 0)));
                        m.ForceJump();
                    }
                }
            }
        }
    }

    void HandleMovement()
    {
        if(Health > 0)
        {
            if (IsGrounded())
            {
                if (VertSpeed < 0)
                {
                    VertSpeed = 0f;
                }

                if (CheckForWall())
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

    public override void TakeDamage(bool HitByTool, int comboModifier)
    {
        Health -= 1; //Goombas die in one hit to anything

        GameManager.GM.AddPoints(1 + comboModifier, transform.position);
        StartCoroutine(DeathAnimation(HitByTool));
    }

    public void RemoveEntity()
    {
        //Used at the end of the stomp animation.
        //Goombas are the only enemy in the demo that can be killed by stomping, so only they have this for now.

        Destroy(gameObject);
    }
}
