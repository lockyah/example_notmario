using UnityEngine;
using UnityEngine.Tilemaps;

public class BrickTilemap : MonoBehaviour
{
    /*
     * Behaviour for the Bricks Tilemap.
     * 
     * On contact, removes the sprite at the affected tile and plays a breaking effect if Mario can break it.
     * Bricks are kept as one tilemap for performance and because they do not need to be customised like ? Blocks.
     */

    Tilemap t;
    private void Start()
    {
        t = GetComponent<Tilemap>();
    }

    public void HitBrick(Vector3 contactPos, bool breakBrick)
    {
        Vector3Int MapTile = t.WorldToCell(contactPos);

        if (t.GetTile(MapTile) != null && breakBrick)
        {
            bool IsUnderground = t.GetTile(MapTile).name == "underground_3";

            t.SetTile(MapTile, null);
            GameManager.GM.PlaySound("Break", false);
            Instantiate(Resources.Load<GameObject>("Effects/" + (IsUnderground ? "UGBrickBreak" : "Brickbreak")), t.CellToWorld(MapTile) + new Vector3(0.5f, 0.5f), Quaternion.identity);
        }
    }
}
