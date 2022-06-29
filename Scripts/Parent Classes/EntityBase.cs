using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityBase : MonoBehaviour
{
    /*
     * Parent class for moving parts of the game - Mario, enemies, powerups, etc.
     * 
     * Gives base behaviour for activating and deactivating when visible, and for the wander behaviour
     * used by Goombas, Koopas, Mushrooms, and similar grounded entities.
     */

    [Header("Entity Traits")]
    public bool OnScreen = false;
    [SerializeField] protected bool MoveRight, DisappearOffscreen = false;
    [SerializeField] protected float HorizSpeed, VertSpeed, WallDistance;

    protected bool CanMove = true;
    protected float HorizSpeedMultiplier = 1f; //Stops horizontal movement if hitting a wall midair
    protected Animator Ani;
    protected Rigidbody2D RB;
    protected Collider2D Coll;

    private void Start()
    {
        Ani = GetComponent<Animator>();
        RB = GetComponent<Rigidbody2D>();
        Coll = GetComponent<Collider2D>();
    }

    // --- VISIBLE ACTIVATION ---
    private void OnBecameVisible()
    {
        if (!OnScreen)
        {
            OnScreen = true;
        }
    }
    private void OnBecameInvisible()
    {
        if (Camera.main != null)
        {
            if (DisappearOffscreen && (transform.position.x < Camera.main.transform.position.x || transform.position.y < Camera.main.transform.position.y - 10))
            {
                Destroy(gameObject);
            }
        }
    }

    // --- WANDER BEHAVIOUR ---
    protected virtual bool CheckForWall()
    {
        if (OnScreen)
        {
            return Physics2D.Raycast(transform.position + new Vector3(0, 0.25f), new Vector2(WallDistance * (MoveRight ? 1 : -1), 0), WallDistance, LayerMask.GetMask("Ground"));
        } else
        {
            return false;
        }
    }
    private void OnDrawGizmos()
    {
        //Draw ray for the CheckForWall behaviour
        Debug.DrawRay(transform.position + new Vector3(0, 0.25f), new Vector2(WallDistance * (MoveRight ? 1 : -1), 0), Color.red);
    }

    public virtual void ForceJump()
    {
        //Force the entity to jump
        VertSpeed = 6f;
    }

    // --- MARIO COLLISION ---
    protected virtual MarioControl CheckForMario()
    {
        RaycastHit2D r = Physics2D.BoxCast(transform.position + (Vector3)Coll.offset, Coll.bounds.size, 0, Vector2.up, 1f, LayerMask.GetMask("Player"));

        if(r.collider != null)
        {
            return r.collider.gameObject.GetComponent<MarioControl>();
        } else
        {
            return null;
        }
    }

    // --- PHYSICS FUNCTIONS ---

    protected virtual RaycastHit2D[] GetCollidersBelow()
    {
        RaycastHit2D[] r = new RaycastHit2D[3];
        r[0] = Physics2D.Raycast(transform.position, Vector2.down, 1f, LayerMask.GetMask("Ground"));
        r[1] = Physics2D.Raycast(transform.position + new Vector3(Coll.bounds.extents.x, 0f), Vector2.down, 1f, LayerMask.GetMask("Ground"));
        r[2] = Physics2D.Raycast(transform.position + new Vector3(-Coll.bounds.extents.x, 0f), Vector2.down, 1f, LayerMask.GetMask("Ground"));

        return r;
    }

    protected virtual RaycastHit2D[] GetCollidersAbove()
    {
        RaycastHit2D[] r = new RaycastHit2D[3];
        r[0] = Physics2D.Raycast(transform.position + new Vector3(0f, 1f), Vector2.up, 0.05f, LayerMask.GetMask("Ground"));
        r[1] = Physics2D.Raycast(transform.position + new Vector3(Coll.bounds.extents.x, 1f), Vector2.up, 0.05f, LayerMask.GetMask("Ground"));
        r[2] = Physics2D.Raycast(transform.position + new Vector3(-Coll.bounds.extents.x, 1f), Vector2.up, 0.05f, LayerMask.GetMask("Ground"));

        //Check that each RH corresponds to a different object.
        //Prevents hitting the same block twice in one frame.

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

    protected virtual bool IsGrounded()
    {
        Collider2D c;

        foreach (RaycastHit2D r in GetCollidersBelow())
        {
            c = r.collider;

            if (c != null && Coll.IsTouching(c))
            {
                return true;
            }
        }

        return false;
    }

    protected virtual bool IsHittingBlock()
    {
        Collider2D c;

        foreach (RaycastHit2D r in GetCollidersAbove())
        {
            c = r.collider;

            if (c != null && Coll.IsTouching(c))
            {
                return true;
            }
        }

        return false;
    }

}
