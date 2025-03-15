using System.Collections;
using UnityEngine;

public static class CoroutineExtension
{
    public delegate IEnumerator CoroutineMethod();
    public static IEnumerator StartSingleInstanceRoutine(this IEnumerator pRoutine, CoroutineMethod pCoroutineMethod, MonoBehaviour pObject)
    {
        if (pRoutine != null) pRoutine = null;
        pObject.StartCoroutine(pRoutine = pCoroutineMethod());
        return pRoutine;
    }
}
