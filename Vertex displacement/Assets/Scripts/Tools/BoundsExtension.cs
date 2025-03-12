using UnityEngine;

public static class BoundsExtensions
{
    public static Bounds GetRendererBounds(this GameObject gameObject)
    {
        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0) return new Bounds(gameObject.transform.position, Vector3.zero);

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }
}