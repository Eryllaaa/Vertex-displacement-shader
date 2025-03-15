using System.Collections;
using UnityEngine;

public class TargetFollower : MonoBehaviour
{
    public Vector3 target;
    public float speed;
    public float _range;
    public Curves.EnumAnimations lerpEasing;

    private Curves.Curve _Easing;
    private IEnumerator _followTarget = null;

    private void OnValidate()
    {
        _Easing = Curves.GetCurve(lerpEasing); 
    }

    private IEnumerator FollowTarget()
    {
        while (true)
        {
            LerpTowardTarget(target, SetSpeed(speed));
            yield return 0;
        }
    }

    private float SetSpeed(float pSpeed)
    {
        float lDist = (transform.position - target).sqrMagnitude;
        float lSpeed = pSpeed * (1 - _Easing(Mathf.Clamp(lDist / (_range * _range), 0f, 0.8f)));
        return lSpeed;
    }

    private void LerpTowardTarget(Vector3 pPos, float lSpeed)
    {
        transform.position = Vector3.MoveTowards(transform.position, pPos, lSpeed * Time.deltaTime);
    }

    public void Activate()
    {
        _followTarget.StartSingleInstanceRoutine(() => FollowTarget(), this);
    }

    public void Deactivate()
    {
        StopCoroutine(_followTarget);
    }
}
