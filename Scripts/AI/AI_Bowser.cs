using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Bowser : EnemyAI
{
    /*
     * Behaviour for the Castle boss, Bowser.
     * 
     * When Mario approaches the end and first activates the BowserTrigger, he begins shooting Fireballs from offscreen.
     * When onscreen, he wanders back and forth, occasionally jumping and shoots fireballs regularly.
     */

    public bool CanAct;
    float FireTimer, JumpTimer;
    float LeftLimit, RightLimit;

    private void Start()
    {
        Ani = GetComponent<Animator>();
        RB = GetComponent<Rigidbody2D>();
        Coll = GetComponent<Collider2D>();

        CanMove = false;
        Health = 5;

        //Which x coordinates can Bowser walk in?
        RightLimit = transform.position.x;
        LeftLimit = RightLimit - 5;
    }

    private void Update()
    {
        if (CanAct)
        {
            HandleMovement();
            MarioControl m = CheckForMario();
            if (m != null)
            {
                if (Vector3.Distance(m.transform.position, transform.position) <= 2f)
                {
                    //Mario can't stomp Bowser, so he'll only take damage if Mario has a star.
                    if (!m.HasStar)
                    {
                        m.TakeDamage();
                    }
                    else
                    {
                        TakeDamage(true, m.BounceCombo);
                        m.BounceCombo++;
                    }
                }
            }

            if (FireTimer <= 0f)
            {
                FireTimer = 3f;

                Vector3 SpawnPosition = OnScreen ? transform.position + new Vector3(-2f, 1.4f) : Camera.main.transform.position +
                    new Vector3(Camera.main.orthographicSize * Camera.main.aspect, Random.Range(-3f, 1.01f), 10);

                Instantiate(Resources.Load<GameObject>("Items/BowserFire"), SpawnPosition, Quaternion.identity);

                GameManager.GM.PlaySound("Enemy Fire", false);
                Ani.SetTrigger("OpenMouth");
            } else
            {
                FireTimer -= Time.deltaTime;
            }
        }
    }

    void HandleMovement()
    {
        if (Health > 0)
        {
            if((!MoveRight && transform.position.x <= LeftLimit) || (MoveRight && transform.position.x >= RightLimit))
            {
                MoveRight = !MoveRight;
            }

            if (IsGrounded())
            {
                if (JumpTimer <= 0f)
                {
                    JumpTimer = Random.Range(3, 7.51f);
                    VertSpeed = 12.5f;
                }
                else
                {
                    JumpTimer -= Time.deltaTime;
                }

                if (VertSpeed < 0f)
                {
                    VertSpeed = 0f;
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
        if (Health > 0)
        {
            //HitByTool is for fireballs; !HitByTool is reserved for the axe.
            if (HitByTool)
            {
                Health -= 1;
            }
            else
            {
                Health = 0;
            }

            if (Health == 0)
            {
                GameManager.GM.AddPoints(9 + comboModifier, transform.position); //5000 points, plus any Star combo.
                Coll.isTrigger = true; //If the bridge is still there, let Bowser pass through.
                StartCoroutine(DeathAnimation(true));
            }
        }
    }

    protected override IEnumerator DeathAnimation(bool instant)
    {
        //Instant and non-instant death both use the same animation, so the bool is unused for Bowser.

        GameManager.GM.PlaySound("Bowser Die", false);
        CanAct = false; //Prevent fireballs

        HorizSpeed = 0f;
        VertSpeed = 3f;

        Ani.SetTrigger("GotHitInstant");
        transform.position += new Vector3(0, 1f); //Offset enemy so that the death animation plays correctly

        //Fall until offscreen, where OnBecameInvisible will cull the enemy.
        while (true)
        {
            transform.position = new Vector2(transform.position.x, Mathf.Lerp(transform.position.y, transform.position.y + VertSpeed, 3f * Time.unscaledDeltaTime));
            VertSpeed -= 5f * Time.unscaledDeltaTime;

            yield return new WaitForEndOfFrame();
        }
    }
}
