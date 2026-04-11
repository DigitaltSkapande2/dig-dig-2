using System.Collections.Generic;

using UnityEngine;

namespace DigDig2.Game
{
    public class BrokenCrystalFade : MonoBehaviour
	{
		[SerializeField] private float decaySpeed = 10f;
		[SerializeField] [ColorUsage(true, true)] private Color targetColor = new(0.32232666f,0f,1.0592736f,1f);
		[SerializeField] [ColorUsage(true, true)] private Color targetCracksColor = new(0.32232666f,0f,1.0592736f,1f);
		[SerializeField] private List<MeshRenderer> pieces;
		
		private static readonly int color = Shader.PropertyToID( "_Color" );
		private static readonly int cracksColor = Shader.PropertyToID( "_CracksColors" );

		private void Update( )
		{
			foreach ( MeshRenderer pieceMeshRenderer in pieces )
			{
				pieceMeshRenderer.material.SetColor( color, Color.Lerp( pieceMeshRenderer.material.GetColor( color ), targetColor, Time.deltaTime * decaySpeed ) );
				pieceMeshRenderer.material.SetColor( cracksColor, Color.Lerp( pieceMeshRenderer.material.GetColor( cracksColor ), targetCracksColor, Time.deltaTime * decaySpeed ) );
			}
		}
	}
}
