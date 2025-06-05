using UnityEngine;
using System;

[Serializable]
public struct CinemaKeyFrameData
{

	public Vector3 KeyPosition;
	public Vector3 InTangent;
	public Vector3 OutTangent;

	public Vector3 targetCameraPosition;
	public Quaternion targetCamerarotation;


	public CinemaKeyFrameData(Vector3 KeyPosition, Vector3 InTangent, Vector3 OutTangent, Vector3 targetCameraPosition, Quaternion targetCamerarotation)
	{
		this.KeyPosition = KeyPosition;
		this.InTangent = InTangent;
		this.OutTangent = OutTangent;
		this.targetCameraPosition = targetCameraPosition;
		this.targetCamerarotation = targetCamerarotation;
	}
}