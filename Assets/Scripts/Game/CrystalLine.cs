using UnityEngine;

namespace DigDig2.Game
{
	public class CrystalLine : MonoBehaviour
	{
		[SerializeField] private Vector3 crystal;
		[SerializeField] private Vector3 enemy;

		[SerializeField] private float controlPointDefaultPosition;
		[SerializeField] private float controlPointResetSpeed;
		[SerializeField] private int lineSegmentsPerUnit;
		[SerializeField] private float randomOffset;
		[SerializeField] private float shieldDistance;

		[SerializeField] private LineRenderer line;

		private Vector3 controlPoint;

		private int segments;

		private void Start( ) { controlPoint = ( crystal + enemy ) / 2; }

		private void Update( )
		{
			if ( Vector3.Distance( crystal, enemy ) > 1 )
				segments = (int)Vector3.Distance( crystal, enemy ) * lineSegmentsPerUnit;
			else
				segments = lineSegmentsPerUnit;

			line.positionCount = segments + 1;

			controlPoint = Vector3.Lerp( controlPoint, Vector3.Lerp( crystal, enemy, controlPointDefaultPosition ), Time.deltaTime * controlPointResetSpeed );

			for ( int index = 0; index <= segments; index++ )
			{
				float value = 1f / segments * index;

				var p1 = Vector3.Lerp( crystal, controlPoint, value );
				var p2 = Vector3.Lerp( controlPoint, enemy, value );
				var p3 = Vector3.Lerp( p1, p2, value );

				p3 += Vector3.up * ( ( Mathf.PerlinNoise1D( value + Time.time ) - 0.5f ) * randomOffset * ( 1 - Mathf.Pow( value, 2 ) ) );

				if ( Vector3.Distance( crystal, p3 ) < shieldDistance )
				{
					line.positionCount -= 1;
					continue;
				}

				if ( index == 0 )
				{
					Vector3 offset = p3 - crystal;
					p3 = crystal + offset.normalized * shieldDistance;

					continue;
				}

				line.SetPosition( index, p3 );
			}
		}

		public void SetPositions( Vector3 crystal, Vector3 enemy )
		{
			this.crystal = crystal;
			this.enemy = enemy;
		}
	}
}
