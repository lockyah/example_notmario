using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarioControl : EntityBase
{
    /*
     * Behaviour for Mario / the player.
     * 
     * True to the original game, he is able to walk, run, jump at variable heights, and shoot fireballs.
     * A public "AnimatingInput" overrides player input for cutscenes like walking into the castle or entering a pipe.
     */

    public enum MarioState { Idle, Run, Jump, Fall, Swim, Flag }
    public enum MarioPowerUp { None, Mushroom, Flower, Dead }

    [Header("Mario Traits")]
    [SerializeField] MarioState State = MarioState.Idle;
    [SerializeField] MarioPowerUp PowerUp = MarioPowerUp.None;
    public Vector2 AnimatingInput = Vector2.zero; //Used by Warp Pipes, the death animation, and Flagpoles. Overrides the user's actual input.

    public bool HasStar = false;
    float InvincibleTimer, CoyoteTimer = 0f; //Invincible via Star or I-Frames. Coyote Timer is to jump if input was pressed too early.
    float ShootCooldown = 0f; //Only allow fireballs to shoot every so often.
    public int BounceCombo = 0; //Increase points earned for some actions if done without landing.

    // Update is called once per frame
    void Update()
    {
        if(PowerUp != MarioPowerUp.Dead)
        {
            InvincibleTimer = Mathf.Clamp(InvincibleTimer - Time.deltaTime, 0, 99f);
            if (InvincibleTimer == 0 && HasStar)
            {
                //If invincibility was from star, ends here and swap back to normal music
                HasStar = false;
                GameManager.GM.PlaySound("Overworld", true);
            }
            ShootCooldown = Mathf.Clamp(ShootCooldown - Time.deltaTime, 0, 99f);
            CoyoteTimer = Mathf.Clamp(CoyoteTimer - Time.deltaTime, 0, 99); //Coyote Time allows Mario to jump if the input was given just before landing or just after leaving the ground.

            if (AnimatingInput == Vector2.zero)
            {
                HandleMoving();
                HandlePowerUp();
            } else
            {
                HorizSpeed = AnimatingInput.x;

                if(AnimatingInput.y != 0)
                {
                    VertSpeed = AnimatingInput.y;
                } else
                {
                    if (IsGrounded())
                    {
                        VertSpeed = 0;
                    } else
                    {
                        VertSpeed -= 50f * Time.deltaTime;
                        VertSpeed = Mathf.Clamp(VertSpeed, -25f, 13f);
                    }
                }

                //Emulate state switching for walking (i.e. flagpoles)
                if (!IsGrounded() && AnimatingInput.y == 0f)
                {
                    State = MarioState.Fall;
                } else
                {
                    State = Mathf.Abs(HorizSpeed) < 0.2f ? MarioState.Idle : MarioState.Run;
                }
            }

            HandleAnimations();
        }


        RB.velocity = new Vector3(HorizSpeed, VertSpeed, 0f);
    }

    void HandleMoving()
    {
        float HorizInput;
        //If pushing down (crouching) then ignore horizontal input on the ground
        if (Input.GetAxisRaw("Vertical") != -1 || !IsGrounded())
        {
            HorizInput = Input.GetAxisRaw("Horizontal") * 3f;
        } else {
            HorizInput = 0;
        }

        if (!CheckForWall())
        {
            //Momentum change is affected by running and reduced in midair
            //Momentum also will not change while time is paused for animations
            HorizSpeed = Mathf.Lerp(HorizSpeed, HorizInput * (Input.GetButton("Fire1") ? 2.5f : 1.25f), (HorizInput == 0 ? 4: 3) * Time.deltaTime * (IsGrounded() ? 1 : 0.33f) * Time.timeScale);
        }
        else
        {
            //Stop immediately if a wall is ahead of Mario
            HorizSpeed = 0;
        }

        if (State == MarioState.Idle || State == MarioState.Run)
        {
            if(State == MarioState.Run && ((HorizSpeed < 0 && HorizInput > 0) || (HorizSpeed > 0 && HorizInput < 0)) && Time.timeScale != 0)
            {
                if (!Ani.GetBool("Skidding"))
                {
                    GameManager.GM.PlaySound("Skid", false);
                }

                Ani.SetBool("Skidding", true);
            } else
            {
                Ani.SetBool("Skidding", false);
            }

            if(HorizInput != 0 && Time.timeScale != 0)
            {
                MoveRight = HorizInput > 0;
            }

            State = Mathf.Abs(HorizSpeed) < 1f ? MarioState.Idle : MarioState.Run;

            if (IsGrounded())
            {
                if (Input.GetButtonDown("Jump") || (CoyoteTimer > 0 && Input.GetButton("Jump")))
                {
                    CoyoteTimer = 0;
                    VertSpeed = 15f;
                    State = MarioState.Jump;

                    GameManager.GM.PlaySound("Jump", false);
                }
            } else
            {
                CoyoteTimer = 0.1f;
                State = MarioState.Fall;
            }


        } else if (State == MarioState.Jump || State == MarioState.Fall)
        {
            if (Input.GetButtonDown("Jump"))
            {
                CoyoteTimer = 0.1f;
            }

            if (State == MarioState.Jump)
            {
                if (Input.GetButton("Jump"))
                {
                    VertSpeed -= 20f * Time.deltaTime;
                }

                if (VertSpeed < 0f || !Input.GetButton("Jump"))
                {
                    State = MarioState.Fall;
                }
            } else if (State == MarioState.Fall)
            {
                VertSpeed -= 50f * Time.deltaTime;
                VertSpeed = Mathf.Clamp(VertSpeed, -25f, 13f);
            }

            if (IsHittingBlock() && VertSpeed > 0f)
            {
                State = MarioState.Fall;
                VertSpeed = 0f;
                GameManager.GM.PlaySound("Bump", false);

                RaycastHit2D[] r = GetCollidersAbove();

                foreach (RaycastHit2D rH in r)
                {
                    Collider2D c = rH.collider;

                    if (c != null)
                    {
                        if (c.name == "Bricks")
                        {
                            float heightMod = (Input.GetAxisRaw("Vertical") == -1 || PowerUp == MarioPowerUp.None ? 1 : 2);
                            c.GetComponent<BrickTilemap>().HitBrick(transform.position + new Vector3(0, heightMod), PowerUp != MarioPowerUp.None);
                        }
                        else
                        {
                            QuestionBlock b = rH.collider.GetComponent<QuestionBlock>();
                            if (b != null)
                            {
                                //Blocks change behaviour based on Mario's powerup
                                b.ActivateBlock(PowerUp != MarioPowerUp.None);
                            }
                        }
                    }
                }
            }

            if (IsGrounded() && VertSpeed < 0f)
            {
                //Landing from a run or jump
                State = MarioState.Idle;
                VertSpeed = 0f;

                if (!HasStar)
                {
                    BounceCombo = 0;
                }
            }
        }
    }

    void HandlePowerUp()
    {
        if(PowerUp == MarioPowerUp.Flower && Input.GetButtonDown("Fire1") && ShootCooldown <= 0 && Time.timeScale != 0)
        {
            GameManager.GM.PlaySound("Fire Ball", false);
            Ani.SetTrigger("Shoot");
            ShootCooldown = 0.175f;

            GameObject f = Instantiate(Resources.Load<GameObject>("Items/Fireball"), transform.position + new Vector3(0.5f * (MoveRight ? 1 : -1), 1f), Quaternion.identity);
            f.GetComponent<Fireball>().SetMovingRight(MoveRight);
        }
    }

    void HandleAnimations()
    {
        Ani.SetFloat("HorizSpeed", Mathf.Abs(HorizSpeed)/3f);
        Ani.SetInteger("CurrentPowerUp", (int)PowerUp);
        Ani.SetInteger("CurrentState", (int)State);
        Ani.SetBool("FacingRight", MoveRight);
        Ani.SetBool("HasStar", HasStar);
        Ani.SetFloat("ShootCooldown", ShootCooldown);

        //Only update crouching if on the ground
        if (IsGrounded() && AnimatingInput == Vector2.zero)
        {
            Ani.SetBool("Crouching", Input.GetAxisRaw("Vertical") == -1);
        } else if (AnimatingInput != Vector2.zero)
        {
            Ani.SetBool("Crouching", false);
        }
    }

    public void GivePowerUp(int p, bool discrete)
    {
        //p = 0 = mushroom, 1 = flower, 2 = star
        //discrete is whether or not to play the effect and sound

        if (!discrete)
        {
            switch (p)
            {
                case 0:
                    if(PowerUp != MarioPowerUp.Mushroom)
                    {
                        PowerUp = MarioPowerUp.Mushroom;
                        GameManager.GM.PlaySound("Powerup", false);
                        Ani.SetTrigger("GotPowerUp");
                        StartCoroutine(TimeStop(1f));
                    } else
                    {
                        //Don't pause time if Mario already has the flower
                        GameManager.GM.PlaySound("Powerup", false);
                    }

                    
                    break;
                case 1:
                    if (PowerUp != MarioPowerUp.Flower)
                    {
                        PowerUp = MarioPowerUp.Flower;
                        GameManager.GM.PlaySound("Powerup", false);
                        Ani.SetTrigger("GotPowerUp");
                        StartCoroutine(TimeStop(1f));
                    }
                    else
                    {
                        //Don't pause time if Mario already has the flower
                        GameManager.GM.PlaySound("Powerup", false);
                    }

                    break;
                case 2:
                    HasStar = true;
                    InvincibleTimer = 10f;
                    GameManager.GM.PlaySound("Star", true);
                    break;
            }
        } else
        {
            //Parse the number given and directly set Mario to it.
            //Only used for the start of the level.

            PowerUp = (MarioPowerUp)p;
        }
        
    }

    public void TakeDamage()
    {
        if(InvincibleTimer <= 0)
        {
            PowerUp -= 1;
            InvincibleTimer = 2f;

            if (PowerUp < 0)
            {
                StartCoroutine(DeathAnimation(false));
            }
            else
            {
                Ani.SetTrigger("GotHit");
                GameManager.GM.PlaySound("Warp", false);
                StartCoroutine(TimeStop(1f));
            }
        }
        
    }

    public void InstantDeath(bool instant)
    {
        //Called from endless pits only, skips TakeDamage and instantly kills Mario
        StartCoroutine(DeathAnimation(instant));
    }

    IEnumerator DeathAnimation(bool instant)
    {
        PowerUp = MarioPowerUp.Dead;

        StartCoroutine(TimeStop(5f));

        GameManager.GM.PlaySound("", true); //Stop music
        GameManager.GM.PlaySound("Die", false);
        float timer = 5f;

        if (!instant)
        {
            VertSpeed = 4f;
            Ani.SetTrigger("GotHit");
            Ani.SetInteger("CurrentPowerUp", 3);
        }

        while(timer > 0)
        {
            //Wait for a second before falling if on-screen
            if (timer <= 4.5f && !instant)
            {
                transform.position = new Vector2(transform.position.x, Mathf.Lerp(transform.position.y, transform.position.y + VertSpeed, 3f * Time.unscaledDeltaTime));
                VertSpeed -= 5f * Time.unscaledDeltaTime;
            }
            
            timer -= Time.unscaledDeltaTime;

            yield return new WaitForEndOfFrame();
        }

        GameManager.GM.LoseLife();
    }

    IEnumerator TimeStop(float time)
    {
        //Used when gaining/losing a powerup or losing a life.
        //Pauses all movement for a short time.
        Time.timeScale = 0f;
        Ani.SetBool("TimeStop", true);

        yield return new WaitForSecondsRealtime(time);

        Time.timeScale = 1f;
        Ani.SetBool("TimeStop", false);
    }

    public int GetPowerUp()
    {
        return (int)PowerUp;
    }

    public MarioState GetMarioState()
    {
        return State;
    }

    // --- OVERRIDE FUNCTIONS ---

    protected override RaycastHit2D[] GetCollidersAbove()
    {
        //Adjusts the boxcast according to Mario's height.

        float heightMod = ((Input.GetAxisRaw("Vertical") == -1 && PowerUp != MarioPowerUp.None) || PowerUp == MarioPowerUp.None ? 0.25f : 1.25f);

        RaycastHit2D[] r = new RaycastHit2D[3];
        r[0] = Physics2D.Raycast(transform.position + new Vector3(0f, heightMod), Vector2.up, 1f, LayerMask.GetMask("Ground"));
        r[1] = Physics2D.Raycast(transform.position + new Vector3(Coll.bounds.extents.x, heightMod), Vector2.up, 1f, LayerMask.GetMask("Ground"));
        r[2] = Physics2D.Raycast(transform.position + new Vector3(-Coll.bounds.extents.x, heightMod), Vector2.up, 1f, LayerMask.GetMask("Ground"));

        //Check that each RH corresponds to a different object.
        List<Collider2D> l = new List<Collider2D>();
        for (int i = 0; i < r.Length; i++)
        {
            Collider2D c = r[i].collider;
            if (c != null)
            {
                if (!l.Contains(c))
                {
                    //Item is unique!
                    l.Add(c);
                }
                else
                {
                    //Remove the second link to the same object
                    r[i] = new RaycastHit2D();
                }
            }
        }

        return r;
    }

    protected override RaycastHit2D[] GetCollidersBelow()
    {
        //Mario has a shorter ground raycast to prevent jumps directly under Ground layers behaving strangely
        
        RaycastHit2D[] r = new RaycastHit2D[3];
        r[0] = Physics2D.Raycast(transform.position, Vector2.down, 0.25f, LayerMask.GetMask("Ground"));
        r[1] = Physics2D.Raycast(transform.position + new Vector3(Coll.bounds.extents.x, 0f), Vector2.down, 0.25f, LayerMask.GetMask("Ground"));
        r[2] = Physics2D.Raycast(transform.position + new Vector3(-Coll.bounds.extents.x, 0f), Vector2.down, 0.25f, LayerMask.GetMask("Ground"));

        return r;
    }

    protected override bool IsGrounded()
    {
        //Checks that at least one contact is under Mario to prevent him hovering in corners of the ground tilemap.

        Collider2D c;
        ContactPoint2D[] conts = new ContactPoint2D[10];
        Coll.GetContacts(conts);

        foreach (RaycastHit2D r in GetCollidersBelow())
        {
            c = r.collider;

            if (c != null && Coll.IsTouching(c))
            {
                if (Coll.isTrigger || c.transform == transform.parent)
                {
                    //If Mario is animating through pipes or attached to a platform, consider him grounded.
                    return true;
                } else
                {
                    foreach (ContactPoint2D cP in conts)
                    {
                        if (cP.point.y <= transform.position.y)
                        {
                            return true;
                        }
                    }
                }
                
            }
        }

        return false;
    }

    public override void ForceJump()
    {
        //Used when landing on an enemy - forces Mario into a jump that can be held as normal
        //Increases the bounce combo counter for higher points before landing

        VertSpeed = 15f;
        State = MarioState.Jump;
        BounceCombo++;
    }

    void OnBecameInvisible()
    {
        if (OnScreen)
        {
            OnScreen = false;
        }
    }
}
