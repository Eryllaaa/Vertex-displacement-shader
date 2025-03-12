using System;
using UnityEngine;

public class Goal : MonoBehaviour
{
    public event Action ballEnteredHole;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Ball.TAG))
        {
            ballEnteredHole?.Invoke();
        }
    }
}
