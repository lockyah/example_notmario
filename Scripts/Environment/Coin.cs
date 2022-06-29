using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : EntityBase
{
    /*
     * Behaviour for coin items.
     * 
     * If in a box (instantiated prefabs), coin is automatically collected.
     * If not (placed prefabs), coin is collected on contact with Mario.
     */

    [SerializeField] bool InBox;

    private void Start()
    {
        Ani = GetComponent<Animator>();
        Coll = GetComponent<Collider2D>();


        //If in a box, collect automatically and skip detecting Mario
        if (InBox)
        {
            Ani.SetTrigger("BoxHit");
            StartCoroutine(Delay());
        }
    }

    IEnumerator Delay()
    {
        GameManager.GM.AddPoints(2, transform.position + new Vector3(0, 1));
        GameManager.GM.AddCoin();
        yield return new WaitForSeconds(0.4f);
        Destroy(gameObject);
    }

    private void Update()
    {
        if (!InBox)
        {
            MarioControl m = CheckForMario();

            if(m != null)
            {
                GameManager.GM.AddPoints(2);
                GameManager.GM.AddCoin();
                Destroy(gameObject);
            }
        }
    }
}
