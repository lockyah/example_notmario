using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpPipe : MonoBehaviour
{
    /*
     * Behaviour for Warp Pipes.
     * 
     * Separate to the actual sprite, this trigger covers the entrance.
     * If Mario is inputting or animating in the pipe direction, he loses collision and animates into the pipe.
     * He and the camera teleport to the other side, animates again, and the player regains control.
     */

    public Vector3 WarpPosition, CameraPosition;
    public bool CameraShouldFollow = true; //Should the Camera move with Mario in the next area?
    public string NewZoneMusic; //Which song should play on the other side?

    public enum PipeDirection { Left, Right, Up, Down }
    [SerializeField] PipeDirection InDirection;
    [SerializeField] PipeDirection OutDirection;

    Vector2 DetermineDirection(PipeDirection p)
    {
        //Determine the direction to move Mario in based on the side of the pipe he's using.

        switch (p)
        {
            case PipeDirection.Left:
                return new Vector2(-3f, 0);
            case PipeDirection.Right:
                return new Vector2(3f, 0);
            case PipeDirection.Up:
                return new Vector2(0, 3f);
            case PipeDirection.Down:
                return new Vector2(0, -3f);
        }

        //Failsafe, but shouldn't be possible to reach
        return Vector2.zero;
    }

    private void OnDrawGizmosSelected()
    {
        Debug.DrawLine(WarpPosition, WarpPosition + ((Vector3)DetermineDirection(OutDirection)), Color.green);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.gameObject.layer == 6)
        {
            MarioControl m = collision.gameObject.GetComponent<MarioControl>();

            //Allow Mario to enter manually or as a "cutscene" like in 1-2
            float HorizInput = m.AnimatingInput.x == 0 ? Input.GetAxisRaw("Horizontal") : m.AnimatingInput.x;
            float VertInput = m.AnimatingInput.y == 0 ? Input.GetAxisRaw("Vertical") : m.AnimatingInput.y;

            if ((InDirection == PipeDirection.Down && VertInput < 0f) ||
                (InDirection == PipeDirection.Up && VertInput > 0f) ||
                (InDirection == PipeDirection.Left && HorizInput < 0f) ||
                (InDirection == PipeDirection.Right && HorizInput > 0f))
            {
                m.AnimatingInput = DetermineDirection(InDirection);

                StartCoroutine(AnimateInDirection(m));

                GetComponent<Collider2D>().enabled = false; //Disable the trigger
            }
        }
    }

    IEnumerator AnimateInDirection(MarioControl m)
    {
        GameManager.GM.PlaySound("", true); //Stop music while warping
        GameManager.GM.PlaySound("Warp", false);

        Collider2D mColl = m.GetComponent<Collider2D>();
        mColl.isTrigger = true; //Allow Mario to pass through blocks

        yield return new WaitForSeconds(m.AnimatingInput.x != 0 ? 0.325f : 0.7f);

        m.AnimatingInput = new Vector2(0.0001f, 0.0001f); //Nearly-stop Mario so that animation continues to play.

        if(Camera.main.transform.position != CameraPosition)
        {
            //Still on the first side. Call another AnimateInDirection to finish the animation.
            yield return new WaitForSeconds(1f);

            //Adjust Warp Position for pipe animation
            switch (OutDirection)
            {
                case PipeDirection.Left:
                    WarpPosition.x += 2.1f;
                    break;
                case PipeDirection.Right:
                    WarpPosition.x -= 2.1f;
                    break;
                case PipeDirection.Up:
                    WarpPosition.y -= 2.5f;
                    break;
                case PipeDirection.Down:
                    WarpPosition.y += 2.5f;
                    break;
            }
            m.gameObject.transform.position = WarpPosition;
            m.AnimatingInput = DetermineDirection(OutDirection);

            RaycastHit2D[] r = Physics2D.BoxCastAll(WarpPosition, new Vector2(3, 3), 0f, Vector2.up);
            foreach(RaycastHit2D rH in r)
            {
                AI_Plant OutPlant = rH.collider.GetComponent<AI_Plant>();
                if (OutPlant != null)
                {
                    //If a plant is on the other side, force it down so it doesn't hit Mario.
                    OutPlant.ForceDown();
                }
            }
            

            Camera.main.transform.position = CameraPosition;
            GameManager.GM.ChangeBackground(NewZoneMusic);
            StartCoroutine(AnimateInDirection(m));
        }
        else
        {
            //On the far side and animation is finished! Re-enable movement, and let's-a go!
            Camera.main.GetComponent<CameraFollow>().FollowMario = CameraShouldFollow;
            GameManager.GM.PlaySound(NewZoneMusic, true);

            m.AnimatingInput = Vector2.zero; //Re-enable movement
            mColl.isTrigger = false;
        }
    }
}
