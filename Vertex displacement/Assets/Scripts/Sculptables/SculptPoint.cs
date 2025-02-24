
using UnityEngine;

public enum SculptDirection { up, down }

public struct SculptPoint
{
    public Vector3 position;
    public float radius;
    public float direction; // --> 1 or -1 in order to change the direction of the displacement
    public float ratio;

    public SculptPoint(Vector3 pPosition, float pRadius, SculptDirection pDirection, float pRatio = 0f)
    {
        position = pPosition;
        radius = pRadius;
        switch (pDirection)
        {
            case SculptDirection.up:
                direction = 1f;
                break;
            case SculptDirection.down:
                direction = -1f;
                break;
            default:
                direction = 0f;
                break;
        }
        ratio = pRatio;
    }

    public SculptPoint(SculptPoint pOther)
    {
        position = pOther.position;
        radius = pOther.radius;
        direction = pOther.direction;
        ratio = pOther.ratio;
    }
}
