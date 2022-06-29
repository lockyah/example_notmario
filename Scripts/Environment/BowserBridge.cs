using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowserBridge : MonoBehaviour
{
    /*
     * Behaviour for the bridge Bowser stands on.
     * 
     * Procedurally handles the Axe animation and the level-ending cutscene for the Castle level.
     */

    Animator ToadAni;
    GameObject AxePiece;
    List<GameObject> BridgePieces;
    AI_Bowser Bowser;
    MarioControl Mario;

    BoxCollider2D AxeColl, BridgeColl;

    void Start()
    {
        AxePiece = transform.GetChild(0).gameObject;
        AxeColl = AxePiece.GetComponent<BoxCollider2D>();
        BridgeColl = GetComponent<BoxCollider2D>();

        Bowser = GameObject.Find("Bowser").GetComponent<AI_Bowser>();
        Mario = GameObject.Find("Mario").GetComponent<MarioControl>();
        ToadAni = transform.GetChild(2).GetComponent<Animator>();

        BridgePieces = new List<GameObject>();
        foreach(Transform t in transform.GetChild(1))
        {
            BridgePieces.Add(t.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == 6)
        {
            if(AxeColl.offset.x == 0f)
            {
                //Start Mario moving towards "Toad".
                StartCoroutine(BridgeAnimation());

                GameManager.GM.SetTimerActive(false);
                Camera.main.GetComponent<CameraFollow>().FollowMario = true;

                AxeColl.offset = new Vector2(23f, 0f);
                AxeColl.size = new Vector2(0.5f, 15f);
            } else
            {
                StartCoroutine(EndAnimation());
            }
        }
    }

    IEnumerator BridgeAnimation()
    {
        //Break the bridge piece by piece, then start Mario walking toward "Toad"
        Time.timeScale = 0f;

        while(BridgePieces.Count > 0)
        {
            GameObject g = BridgePieces[BridgePieces.Count - 1];
            BridgePieces.Remove(g);
            Destroy(g);

            GameManager.GM.PlaySound("Break", false);
            yield return new WaitForSecondsRealtime(0.1f);
        }

        BridgeColl.enabled = false;
        Bowser.TakeDamage(false, 0);

        yield return new WaitForSecondsRealtime(1.5f);

        Time.timeScale = 1f;
        GameManager.GM.PlaySound("WorldClear", true);
        Mario.AnimatingInput = new Vector2(4f, 0);
    }

    IEnumerator EndAnimation()
    {
        //"Toad" handles showing the text in time, so this just handles the bonus and level end.
        ToadAni.SetTrigger("Continue");
        Mario.AnimatingInput = new Vector2(0.0001f, 0.0001f);

        yield return new WaitForSeconds(8f);

        StartCoroutine(GameManager.GM.TimeBonus());
        while (GameManager.GM.Timer > 0)
        {
            yield return new WaitForEndOfFrame();
        }

        StartCoroutine(GameManager.GM.CastleFinish());
    }
}
