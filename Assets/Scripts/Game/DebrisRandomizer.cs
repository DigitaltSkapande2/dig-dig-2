using System;
using System.Collections.Generic;

using UnityEngine;

using Random = UnityEngine.Random;

namespace DigDig2.Game
{
	public class DebrisRandomizer : MonoBehaviour
	{
		[SerializeField] private float linearVelocityStrength = 1f;
		[SerializeField] private float upExtraVelocityStrength = 2f;
		[SerializeField] private float angularVelocityStrength = 1f;
		[SerializeField] private List<Rigidbody> pieces;
		
		private void Start( )
		{
			foreach ( Rigidbody piece in pieces )
			{
				piece.transform.rotation = Quaternion.Euler( Random.Range( -180f, 180f ), Random.Range( -180f, 180f ), Random.Range( -180f, 180f ) );
			}
			
			foreach ( Rigidbody piece in pieces )
			{
				Vector3 direction = ( piece.transform.position ).normalized;
				piece.linearVelocity = direction * linearVelocityStrength + Vector3.up * upExtraVelocityStrength;
				piece.angularVelocity = new Vector3( Random.Range( -1f, 1f ), Random.Range( -1f, 1f ), Random.Range( -1f, 1f ) ) * angularVelocityStrength;
			}
		}
	}
}
