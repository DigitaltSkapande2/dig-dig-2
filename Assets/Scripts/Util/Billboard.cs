using UnityEngine;

namespace DigDig2.Util {
	public class Billboard : MonoBehaviour {
		[SerializeField] private bool onlyYAxis;

		private void Awake( ) { UpdateRotation( ); }

		private void Update( ) { UpdateRotation( ); }

		private void UpdateRotation( ) {
			if ( onlyYAxis && Camera.main ) {
				transform.rotation = Quaternion.Euler(
					new(
						transform.rotation.x,
						Camera.main.transform.rotation.y,
						transform.rotation.z
					)
				);
			} else if ( Camera.main ) transform.rotation = Camera.main.transform.rotation;
		}
	}
}
