using System;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour
{

    [SerializeField] public List<Transform> targets;
    [Tooltip("the time in seconds it will take for the camera to get to the target position")]
    [SerializeField] public float followSpeed = 0.1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    void Update()
    {
        if (targets.Count == 0)
        {
            Debug.LogWarning("Target is not set for GameCamera. Make sure this is intentional");
        }

        Vector3 resultingPosition = Vector3.zero;

        if (targets.Count > 0)
        {
            foreach (Transform t in targets)
            {
                if (t == null)
                {
                    Debug.LogWarning("One of the targets is null. Skipping this target.");
                    continue;
                }
                resultingPosition += t.position;
            }
            // get midpoint
            resultingPosition /= targets.Count;
        }

        transform.position = Vector3.Lerp(transform.position, resultingPosition, Time.deltaTime * 2f);
    }
}