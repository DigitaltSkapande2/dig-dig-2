using System.Collections.Generic;

using DigDig2.Debugging.Menu;

using UnityEngine;

namespace DigDig2.Debugging.Modules
{
	[Debug]
	public class DisplayCollisionBoxes : MonoBehaviour
	{
		private readonly List<Collider2D> colliders = new( );

		// LineRenderer for drawing the colliders
		private readonly List<LineRenderer> lineRenderers = new( );

		private void Update( )
		{
			UpdateColliders( );

			// Update the line renderers to draw the colliders
			for ( int i = 0; i < colliders.Count; i++ )
			{
				if ( !colliders[ i ] || !colliders[ i ].enabled ) continue;

				switch ( colliders[ i ] )
				{
					case BoxCollider2D boxCollider: DrawBoxCollider( boxCollider, lineRenderers[ i ] ); break;
					case CircleCollider2D circleCollider: DrawCircleCollider( circleCollider, lineRenderers[ i ] ); break;
					case PolygonCollider2D polygonCollider: DrawPolygonCollider( polygonCollider, lineRenderers[ i ] ); break;
					case EdgeCollider2D edgeCollider: DrawEdgeCollider( edgeCollider, lineRenderers[ i ] ); break;
				}
			}
		}

		private void OnDisable( ) { ClearLineRenderers( ); }

		private void UpdateColliders( )
		{
			colliders.Clear( );
			ClearLineRenderers( );

			// Collect all 2D colliders in the scene
			colliders.AddRange( FindObjectsByType<Collider2D>( FindObjectsSortMode.None ) );

			// Initialize line renderers for each collider
			for ( int index = 0; index < colliders.Count; index++ )
			{
				var lineObj = new GameObject( "ColliderVisualizer" );
				lineObj.transform.SetParent( transform );
				LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>( );
				lineRenderer.positionCount = 0;
				lineRenderer.startWidth = 0.05f;
				lineRenderer.endWidth = 0.05f;
				lineRenderer.useWorldSpace = true;
				lineRenderer.material = new( Shader.Find( "Sprites/Default" ) ); // Set a simple material
				lineRenderer.startColor = Color.green;
				lineRenderer.endColor = Color.green;
				lineRenderers.Add( lineRenderer );
			}
		}

		private void ClearLineRenderers( )
		{
			foreach ( LineRenderer lineRenderer in lineRenderers ) { Destroy( lineRenderer.gameObject ); }

			lineRenderers.Clear( );
		}

		private void DrawBoxCollider( BoxCollider2D boxCollider, LineRenderer lineRenderer )
		{
			var corners = new Vector3[ 5 ];
			Vector2 center = boxCollider.transform.TransformPoint( boxCollider.offset );
			Vector2 size = boxCollider.size * boxCollider.transform.lossyScale;

			corners[ 0 ] = center + new Vector2( -size.x, -size.y ) * 0.5f;
			corners[ 1 ] = center + new Vector2( size.x, -size.y ) * 0.5f;
			corners[ 2 ] = center + new Vector2( size.x, size.y ) * 0.5f;
			corners[ 3 ] = center + new Vector2( -size.x, size.y ) * 0.5f;
			corners[ 4 ] = corners[ 0 ]; // Close the box

			lineRenderer.positionCount = corners.Length;
			lineRenderer.SetPositions( corners );
		}

		private void DrawCircleCollider( CircleCollider2D circleCollider, LineRenderer lineRenderer )
		{
			const int SEGMENTS = 30;
			var points = new Vector3[ SEGMENTS + 1 ];

			Vector2 center = circleCollider.transform.TransformPoint( circleCollider.offset );
			float radius = circleCollider.radius * Mathf.Max( circleCollider.transform.lossyScale.x, circleCollider.transform.lossyScale.y );

			for ( int i = 0; i <= SEGMENTS; i++ )
			{
				float angle = (float)i / SEGMENTS * 2 * Mathf.PI;
				points[ i ] = center + new Vector2( Mathf.Cos( angle ) * radius, Mathf.Sin( angle ) * radius );
			}

			lineRenderer.positionCount = points.Length;
			lineRenderer.SetPositions( points );
		}

		private void DrawPolygonCollider( PolygonCollider2D polygonCollider, LineRenderer lineRenderer )
		{
			Vector2[ ] points = polygonCollider.points;
			var worldPoints = new Vector3[ points.Length + 1 ];

			for ( int i = 0; i < points.Length; i++ ) { worldPoints[ i ] = polygonCollider.transform.TransformPoint( points[ i ] ); }

			worldPoints[ points.Length ] = worldPoints[ 0 ]; // Close the polygon

			lineRenderer.positionCount = worldPoints.Length;
			lineRenderer.SetPositions( worldPoints );
		}

		private void DrawEdgeCollider( EdgeCollider2D edgeCollider, LineRenderer lineRenderer )
		{
			Vector2[ ] points = edgeCollider.points;
			var worldPoints = new Vector3[ points.Length ];

			for ( int i = 0; i < points.Length; i++ ) { worldPoints[ i ] = edgeCollider.transform.TransformPoint( points[ i ] ); }

			lineRenderer.positionCount = worldPoints.Length;
			lineRenderer.SetPositions( worldPoints );
		}
	}
}
