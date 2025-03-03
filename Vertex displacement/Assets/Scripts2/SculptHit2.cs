using UnityEngine;

public struct SculptHit2
{
    public Vector3 position;
    public int direction;
    public float radius;

    public SculptHit2(Vector3 pPosition, SculptDirection pDirection, float pRadius)
    {
        position = pPosition;
        switch (pDirection)
        {
            case SculptDirection.up:
                direction = 1;
                break;
            case SculptDirection.down:
                direction = -1;
                break;
            case SculptDirection.none:
                direction = 0;
                break;
            default:
                direction = 0;
                break;
        }
        radius = pRadius;
    }

    public SculptHit2(SculptHit2 pOther)
    {
        position = pOther.position;
        direction = pOther.direction;
        radius = pOther.radius;
    }

    public static SculptHit2 none = new SculptHit2(Vector3.zero, SculptDirection.none, 0);
}
