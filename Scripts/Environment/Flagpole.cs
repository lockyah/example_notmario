using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flagpole : MonoBehaviour
{
    /*
     * Behaviour for the level-ending flagpoles that allows the animations to play.
     * 
     * On contact, programatically lowers Mario and the flag to the ground, then has him walk toward a second trigger that removes him.
     */

    Transform FlagGO, CastleFlagGO;
    AudioSource a;
    CapsuleCollider2D triggerCol;
    int FireworksBonus;
    bool FlagHit = false;

    [SerializeField] Vector2 DoorPosition;

    private void Start()
    {
        a = GetComponent<AudioSource>(); //Needs a unique AudioSource so that it can be stopped at the right time.
        triggerCol = GetComponent<CapsuleCollider2D>();
        FlagGO = transform.GetChild(0);
        CastleFlagGO = transform.GetChild(1);
    }

    private void OnDrawGizmosSelected()
    {
        Debug.DrawLine(DoorPosition, DoorPosition + new Vector2(0, 3f), Color.red);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.isTrigger && collision.gameObject.layer == 6)
        {
            if (!FlagHit)
            {
                MarioControl m = collision.GetComponent<MarioControl>();
                if (m != null)
                {
                    StartCoroutine(FlagAnimation(m));

                    FlagHit = true;

                    triggerCol.enabled = false; //Disable the trigger
                    GameManager.GM.SetTimerActive(false);
                    FireworksBonus = GameManager.GM.GetFireworksBonus(); //How many fireworks should fire?
                }
            } else
            {
                //Delete Mario to make him "disappear" into the castle.
                Destroy(collision.gameObject);
                StartCoroutine(CastleAnimation());
            }
        }        
    }

    IEnumerator FlagAnimation(MarioControl m)
    {
        //The flagpole is procedurally animated based on how high Mario jumped onto it.

        //Mario hits the flag and pauses temporarily as the score shows.
        Collider2D mColl = m.GetComponent<Collider2D>();
        Animator mAni = mColl.GetComponent<Animator>();
        mColl.isTrigger = true; //Allow Mario to pass through blocks and enemies
        m.AnimatingInput = new Vector2(0.001f, 0.001f); //Stop Mario in place
        mAni.SetBool("OnFlag", true);

        //Award points based on height from the bottom of the flagpole
        float d = Vector2.Distance(transform.position, m.transform.position);
        if(d < 1.5f)
        {
            GameManager.GM.AddPoints(1, m.transform.position); //100 points
        } else if (d < 3f)
        {
            GameManager.GM.AddPoints(3, m.transform.position); //400 points
        } else if(d < 4.5f)
        {
            GameManager.GM.AddPoints(5, m.transform.position); //800 points
        } else if(d < 6)
        {
            GameManager.GM.AddPoints(7, m.transform.position); //2000 points
        } else
        {
            GameManager.GM.AddPoints(9, m.transform.position); //5000 points
        }

        GameManager.GM.PlaySound("Kick", false);
        GameManager.GM.PlaySound("", true); //Stop music
        yield return new WaitForSeconds(1f);

        //Mario slides down the pole. The flag is parented temporarily to him to drop as he does.
        a.Play();
        FlagGO.parent = m.transform;
        m.AnimatingInput = new Vector2(0, -6f);

        while (m.transform.position.y - transform.position.y > 0.5f)
        {
            yield return new WaitForEndOfFrame();
        }
        mAni.speed = 0f;
        FlagGO.parent = transform;
        a.Stop(); //Cut off flagpole sound if hit the end

        //Mario stops in place. The trigger moves to the door to remove him once he reaches it.
        m.AnimatingInput = new Vector2(0.001f, 0.001f); //Stop Mario in place
        triggerCol.offset = DoorPosition - (Vector2)transform.position;
        triggerCol.enabled = true;
        yield return new WaitForSeconds(1f);

        //Mario leaves the flagpole and walks to the castle door.
        GameManager.GM.PlaySound("Flagpole", true);
        mAni.SetBool("OnFlag", false);
        mColl.isTrigger = false;
        mAni.speed = 1f;
        m.AnimatingInput = new Vector2(3f, 0f);
    }

    IEnumerator CastleAnimation()
    {
        StartCoroutine(GameManager.GM.TimeBonus());
        while (GameManager.GM.Timer > 0)
        {
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(0.5f);

        //Raise the tiny flag
        float GoalHeight = CastleFlagGO.position.y + 1.5f;
        while (CastleFlagGO.position.y < GoalHeight)
        {
            CastleFlagGO.position += new Vector3(0, 6f, 0) * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(0.5f);

        if (FireworksBonus == 1 || FireworksBonus == 3 || FireworksBonus == 6)
        {
            while(FireworksBonus > 0)
            {
                Vector2 FireworkPosition = CastleFlagGO.position + new Vector3(Random.Range(-7, 8), Random.Range(1, 6), 0);

                Instantiate(Resources.Load<GameObject>("Effects/Firework"), FireworkPosition, Quaternion.identity);
                GameManager.GM.PlaySound("Thwomp", false);

                FireworksBonus--;
                yield return new WaitForSeconds(0.5f);
            }
        }

        yield return new WaitForSeconds(2f);

        GameManager.GM.LoadLevel(-1);
    }
}
