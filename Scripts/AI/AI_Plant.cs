using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Plant : EnemyAI
{
    /*
     * Behaviour for Piranha Plants.
     * 
     * Alternates between popping out of pipes to attack and hiding inside every three seconds.
     * If Mario is stood on the pipe, they won't exit it until he moves.
     */

    public bool PlantIsUp = false;
    public float ChangeTimer = 3f;

    private void Start()
    {
        Ani = GetComponent<Animator>();
        RB = GetComponent<Rigidbody2D>();
        Coll = GetComponent<Collider2D>();

        Ani.SetInteger("EnemyType", (int)type);
    }

    // Update is called once per frame
    void Update()
    {
        if (OnScreen)
        {
            if (ChangeTimer > 0)
            {
                ChangeTimer -= Time.deltaTime;
            }

            MarioControl m = CheckForMario();

            if (PlantIsUp)
            {
                if (m != null)
                {
                    if (!m.HasStar && Vector2.Distance(transform.position, m.transform.position) <= 1.25f)
                    {
                        m.TakeDamage();
                    }
                }

                if (ChangeTimer <= 0)
                {
                    //If plant is still up and it's time to swap, swap as normal
                    ChangeTimer = 3;
                    PlantIsUp = false;
                    Ani.SetTrigger("Continue");
                }
            }
            else
            {
                //If Mario is not above plant and timer is up, swap to rising
                if(m == null && ChangeTimer <= 0)
                {
                    ChangeTimer = 3;
                    PlantIsUp = true;
                    Ani.SetTrigger("Continue");
                }
            }
        }
    }

    public void ForceDown()
    {
        //If Mario is using a pipe that the plant is in, make sure that it can't hurt him on the exit.
        PlantIsUp = false;
        ChangeTimer = 3;
        Ani.SetTrigger("SkipToDown");
    }

    public override void TakeDamage(bool HitByTool, int comboModifier)
    {
        //Plants can only be killed by fire flowers or stars when out of the pipe.

        if (HitByTool)
        {
            Health -= 1; //Can't take more than one hit from fireballs/shells
            GameManager.GM.AddPoints(1 + comboModifier, transform.position);
            StartCoroutine(DeathAnimation(HitByTool));
        }
    }
}
