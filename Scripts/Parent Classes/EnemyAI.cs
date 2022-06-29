using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : EntityBase
{
    /*
     * Parent class for enemies.
     * 
     * Grandparent class, EntityBase, gives basic entity behaviour. This includes enemy-specific details
     * and functions that items, Mario, projectiles, etc. would not need.
     */

    public enum EnemyType { Overworld, Underground, Underwater, Castle, Red, Flying };

    [Header("Enemy Traits")]
    [SerializeField] protected EnemyType type;
    protected int Health = 1;
    [SerializeField] protected bool CanBeStomped = false; //Not all enemies use this, but prevents Mario from jumping onto the enemy.

    private void Start()
    {
        Ani = GetComponent<Animator>();
        RB = GetComponent<Rigidbody2D>();
        Coll = GetComponent<Collider2D>();

        Ani.SetInteger("EnemyType", (int)type);
    }

    public virtual void TakeDamage(bool HitByTool, int comboModifier)
    {
        //React to being stomped on or hit by a shell, fire flower, etc.
        //Empty as this differs by enemy.
    }

    protected virtual IEnumerator DeathAnimation(bool instant)
    {
        if (instant || !IsGrounded())
        {
            GameManager.GM.PlaySound("Kick", false);
            HorizSpeed = 0f;
            VertSpeed = 3f;

            Ani.SetTrigger("GotHitInstant");
            transform.position += new Vector3(0, 1f); //Offset enemy so that the death animation plays correctly

            //Fall until offscreen, where OnBecameInvisible will cull the enemy.
            while (true)
            {
                transform.position = new Vector2(transform.position.x, Mathf.Lerp(transform.position.y, transform.position.y + VertSpeed, 3f * Time.deltaTime));
                VertSpeed -= 5f * Time.deltaTime;

                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            //Play normal death animation. Particle effect deletes the enemy when it's done.
            GameManager.GM.PlaySound("Squish", false);
            Ani.SetTrigger("GotHit");
            HorizSpeed = 0f;
        }
    }
}
