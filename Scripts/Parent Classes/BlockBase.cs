using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockBase : MonoBehaviour
{
    /*
     * Inherited by QuestionBlock. Handles what happens when Mario jumps into it.
     * Previously used by Brick blocks, but lead to performance issues with high numbers of bricks. Replaced by BrickTilemap
     */
    public enum Items { None, PowerUp, Coin, Star, ExtraLife }
    [SerializeField] protected Items Contains = Items.None;
    
    protected bool BlockHit;
    protected Animator Ani;

    private void Start()
    {
        Ani = GetComponent<Animator>();
    }

    virtual public void ActivateBlock(bool IsPoweredUp)
    {
        //Some blocks have differing behaviours based on if Mario is small or not.
    }
}
