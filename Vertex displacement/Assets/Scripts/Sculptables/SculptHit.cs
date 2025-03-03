using UnityEngine;

public enum SculptDirection { up, down, none }

public struct SculptHit
{
    public Vector3 position;
    public int direction; // --> 1 or -1 in order to change the direction of the displacement
    public float radius;

    public SculptHit(Vector3 pPosition, SculptDirection pDirection, float pRadius)
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

    public SculptHit(SculptHit pOther)
    {
        position = pOther.position;
        direction = pOther.direction;
        radius = pOther.radius;
    }

    public static SculptHit none = new SculptHit(Vector3.zero, SculptDirection.none, 0f);
}
