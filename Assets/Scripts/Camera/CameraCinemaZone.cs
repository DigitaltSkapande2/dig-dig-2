
using UnityEngine;
using System.Collections.Generic;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

#region Component impl

public class CameraCinemaZone : MonoBehaviour
{
	[SerializeField] private List<CinemaKeyFrameData> keyframes = new List<CinemaKeyFrameData>();
	[SerializeField] private bool drawGizmos = true;
	[SerializeField] private int amountOfLinesPerSpline = 40;

	Vector3 GetClosestPositionOnCurve(Vector3 position, float tolerance)
	{
		return Vector3.back;
	}

	Vector3 GetPositionBetweenKeyframes(CinemaKeyFrameData inKey, CinemaKeyFrameData outKey, float t)
	{
		Vector3 p0 = inKey.KeyPosition;
		Vector3 p1 = inKey.KeyPosition + inKey.OutTangent;
		Vector3 p2 = outKey.KeyPosition + outKey.InTangent;
		Vector3 p3 = outKey.KeyPosition;

		Vector3 outPosition =
			Vector3.Lerp(
				Vector3.Lerp(p0, p1, t),
				Vector3.Lerp(p2, p3, t),
				t
			)
		;

		return outPosition;

	}

	void OnDrawGizmos()
	{
		if (!drawGizmos) return;

		Vector3 previousPosition = Vector3.zero;
		for (int i = 0; i < keyframes.Count - 1; i++)
		{
			CinemaKeyFrameData keyframe = keyframes[i];

			Gizmos.color = Color.green;
			Gizmos.DrawSphere(keyframe.KeyPosition, 0.1f);

			for (int j = 0; j < amountOfLinesPerSpline; j++)
			{
				float t = (float)j / (amountOfLinesPerSpline - 1);
				Vector3 position = GetPositionBetweenKeyframes(keyframe, keyframes[i + 1], t);
				Gizmos.color = Color.yellow;
				if (j == 0)
				{
					previousPosition = keyframe.KeyPosition;
				}

				Gizmos.DrawLine(previousPosition, position);
				previousPosition = position;
			}
		}
	}

}

#endregion

#region Editor impl

#if UNITY_EDITOR

public class CameraCinemaZoneEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
	}

	void OnSceneGUI()
	{

	}
}

#endif

#endregion