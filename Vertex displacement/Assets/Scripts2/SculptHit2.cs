using UnityEngine;

public struct SculptHit2
{
    public Vector3 position;
    public Vector3 previousPos;
    public int direction;
    public float radius;
    public float speed;

    public SculptHit2(Vector3 pPosition, Vector3 pPreviousPos, SculptDirection pDirection, float pRadius, float pSpeed)
    {
        position = pPosition;
        previousPos = pPreviousPos;
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
        speed = pSpeed;
    }

    public SculptHit2(SculptHit2 pOther)
    {
        position = pOther.position;
        previousPos = pOther.previousPos;
        direction = pOther.direction;
        radius = pOther.radius;
        speed = pOther.speed;
    }

    public static SculptHit2 none = new SculptHit2(Vector3.zero, Vector3.zero, SculptDirection.none, 0f, 0f);
}
