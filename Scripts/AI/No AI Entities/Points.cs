using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Points : MonoBehaviour
{
    /*
     * Behaviour for the points marker that appears when some actions are performed.
     * 
     * Using the same index as point values in GameManager, finds the correct sprite and then slowly rises and disappears.
     */
    public void Setup(int points)
    {
        SpriteRenderer s = GetComponent<SpriteRenderer>();
        Sprite[] sp = Resources.LoadAll<Sprite>("Effects/PointImages");

        s.sprite = sp[points];
        StartCoroutine(Rise());
    }

    IEnumerator Rise()
    {
        float timer = 1f;

        while(timer > 0)
        {
            transform.position += new Vector3(0, 0.005f) * Time.timeScale;
            timer -= Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        Destroy(gameObject);
    }
}
