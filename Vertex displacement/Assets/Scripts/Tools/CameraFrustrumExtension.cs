using UnityEngine;

public static class CameraFrustrumExtension
{
    public static bool IsInCameraFrustum(this Camera camera, Bounds objectBounds)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, objectBounds);
    }
}
