using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestionBlock : MonoBehaviour
{
    /*
     * Behaviour for ? Blocks.
     * 
     * Contents set with BlockBase (Empty, PowerUp, Coin, Star). When hit from below or by a shell, the box activates.
     * CurrentPowerUp determines which item is spawned for PowerUp - Mushroom for Small Mario, Fire Flower otherwise.
     */

    Transform ItemMount;

    public enum Items { None, PowerUp, Coin, Star, ExtraLife }
    [SerializeField] protected Items Contains = Items.None;

    [SerializeField] bool MultiHit; //Can this block be hit multiple times?
    [SerializeField] bool IsUnderground; //Which sprite to use?
    float MultiHitTimer = 5f;
    protected bool BlockHit;
    protected Animator Ani;

    private void Start()
    {
        Ani = GetComponent<Animator>();
        Ani.SetBool("IsUnderground", IsUnderground);
        ItemMount = transform.GetChild(1);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Used only by Hidden ? Blocks, which can only be activated by Mario instead of shells.
        //Instead of Mario hitting them by collision, this checks for his velocity and position when he enters the collision.
        if(collision.gameObject.layer == 6)
        {
            MarioControl m = collision.gameObject.GetComponent<MarioControl>();
            if(m != null)
            {
                if (collision.gameObject.transform.position.y < transform.position.y && m.GetMarioState() == MarioControl.MarioState.Jump)
                {
                    int p = m.GetPowerUp();
                    ActivateBlock(p != 0 && p != 3);
                    GetComponent<Collider2D>().isTrigger = false;
                }
            }
            
        }
    }

    private void Update()
    {
        if(MultiHitTimer < 5f && MultiHitTimer > 0f)
        {
            //5 seconds to keep hitting the block. BlockHit becomes true on the last hit.
            MultiHitTimer -= Time.deltaTime;
        }
    }

    public void ActivateBlock(bool IsPoweredUp)
    {
        //When activated, PowerUp needs to give a mushroom or flower depending on what Mario already has. Otherwise, it's unused.

        if (!BlockHit)
        {
            //If there are entities above the block, either make them jump or give them damage.
            RaycastHit2D[] r = Physics2D.BoxCastAll(transform.position, new Vector3(1, 1), 0f, Vector2.up, LayerMask.GetMask("Enemies", "Items"));
            foreach(RaycastHit2D rh in r)
            {
                if(rh.collider != null)
                {
                    EntityBase e = rh.collider.GetComponent<EntityBase>();
                    if (e != null)
                    {
                        try
                        {
                            ((EnemyAI)e).TakeDamage(true, 0);
                        }
                        catch
                        {
                            e.ForceJump();
                        }
                    }
                }
            }


            if (MultiHit)
            {
                if (MultiHitTimer <= 0f)
                {
                    BlockHit = true;
                } else if(MultiHitTimer == 5f)
                {
                    //Start the timer from the first hit
                    MultiHitTimer -= Time.deltaTime;
                }
            }
            else
            {
                BlockHit = true;
            };

            Ani.SetTrigger("BlockHit");
            Ani.SetBool("BlockEmpty", BlockHit);

            switch (Contains)
            {
                case Items.PowerUp:
                    if (!IsPoweredUp)
                    {
                        //Spawn Mushroom - delays for animation, then patrols.
                        Instantiate(Resources.Load<GameObject>("Items/Mushroom"), ItemMount.position, Quaternion.identity, ItemMount);
                    }
                    else
                    {
                        //Spawn Flower - does not move.
                        Instantiate(Resources.Load<GameObject>("Items/Flower"), ItemMount.position, Quaternion.identity, ItemMount);
                    }

                    GameManager.GM.PlaySound("Item", false);
                    break;
                case Items.ExtraLife:
                    //Spawn 1-Up - uses same script as Mushroom.
                    Instantiate(Resources.Load<GameObject>("Items/ExtraLife"), ItemMount.position, Quaternion.identity, ItemMount);

                    GameManager.GM.PlaySound("Item", false);
                    break;
                case Items.Coin:
                    //Spawn coin - collected automatically.
                    Instantiate(Resources.Load<GameObject>("Items/Coin"), ItemMount.position, Quaternion.identity, ItemMount);
                    break;
                case Items.Star:
                    //Spawn star - delays for animation, then bounces back and forth.
                    Instantiate(Resources.Load<GameObject>("Items/Star"), ItemMount.position, Quaternion.identity, ItemMount);

                    GameManager.GM.PlaySound("Item", false);
                    break;
            }
        }
    }
}
