using System;
using System.Collections.Generic;

using UnityEngine;

namespace DigDig2.EffectSystem {
	//  T is the InstanceData Class

	public class Effect<T> : MonoBehaviour where T : ICloneable {
		[SerializeField] internal bool useUnscaledTime = true;
		internal List<T> effectInstances = new( );

		public void Update( ) {
			for ( int i = effectInstances.Count - 1; i > -1; i-- ) { UpdateEffect( effectInstances[ i ] ); }
		}

		public virtual void PlayEffectInstance( T effectInstance ) {
			// Store a copy of the provided instance so callers can reuse the same template
			// without mutating the serialized asset/state.
			var copy = (T)effectInstance.Clone( );
			effectInstances.Add( copy );
			OnEffectStart( copy );
		}

		internal virtual void OnEffectStart( T effect ) { }

		internal virtual void UpdateEffect( T effect ) { }

		internal virtual void OnEffectEnd( T effect ) { }
	}
}
