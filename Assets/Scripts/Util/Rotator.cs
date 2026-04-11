using System;

using UnityEngine;

namespace DigDig2.Util
{
    public class Rotator : MonoBehaviour
	{
		[SerializeField] public bool handledExternally = false;
		[SerializeField] private float rotationSpeed = 1f;
		
		private void Update( )
		{
			if (!handledExternally) Rotate( );
		}

		public void Rotate( float multiplier = 1f, float deltaTime = -1f )
		{
			if ( deltaTime < 0 ) deltaTime = Time.deltaTime;
			transform.rotation *= Quaternion.AngleAxis( rotationSpeed * deltaTime * multiplier, Vector3.forward );
		}
	}
}
