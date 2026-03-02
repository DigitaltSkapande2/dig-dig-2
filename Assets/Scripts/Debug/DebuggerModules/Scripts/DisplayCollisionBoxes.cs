using System.Collections.Generic;
using UnityEngine;


namespace DigDig2.Debugging
{
    [DigDig2.Debugging.Debug]
    public class DisplayCollisionBoxes : MonoBehaviour
    {
        private List<Collider2D> colliders = new List<Collider2D>();

        [DebugSerialized] public float Hejjj = 4f;

        // LineRenderer for drawing the colliders
        private List<LineRenderer> lineRenderers = new List<LineRenderer>();

        private void Awake()
        {

        }

        private void Update()
        {
            UpdateColiders();

            // Update the line renderers to draw the colliders
            for (int i = 0; i < colliders.Count; i++)
            {
                if (colliders[i] == null || !colliders[i].enabled) continue;

                if (colliders[i] is BoxCollider2D boxCollider)
                {
                    DrawBoxCollider(boxCollider, lineRenderers[i]);
                }
                else if (colliders[i] is CircleCollider2D circleCollider)
                {
                    DrawCircleCollider(circleCollider, lineRenderers[i]);
                }
                else if (colliders[i] is PolygonCollider2D polygonCollider)
                {
                    DrawPolygonCollider(polygonCollider, lineRenderers[i]);
                }
                else if (colliders[i] is EdgeCollider2D edgeCollider)
                {
                    DrawEdgeCollider(edgeCollider, lineRenderers[i]);
                }
            }
        }

        private void OnEnable()
        {
            
        }
        private void OnDisable()
        {
            ClearLineRenderers();
        }

        private void UpdateColiders()
        {
            colliders.Clear();
            ClearLineRenderers();

            // Collect all 2D colliders in the scene
            colliders.AddRange(FindObjectsByType<Collider2D>(FindObjectsSortMode.None));

            // Initialize line renderers for each collider
            foreach (Collider2D collider in colliders)
            {
                GameObject lineObj = new GameObject("ColliderVisualizer");
                lineObj.transform.SetParent(transform);
                LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();
                lineRenderer.positionCount = 0;
                lineRenderer.startWidth = 0.05f;
                lineRenderer.endWidth = 0.05f;
                lineRenderer.useWorldSpace = true;
                lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Set a simple material
                lineRenderer.startColor = Color.green;
                lineRenderer.endColor = Color.green;
                lineRenderers.Add(lineRenderer);
            }
        }

        private void ClearLineRenderers()
        {
            foreach (LineRenderer lineRenderer in lineRenderers)
            {
                Destroy(lineRenderer.gameObject);
            }
            lineRenderers.Clear();
        }

        private void DrawBoxCollider(BoxCollider2D boxCollider, LineRenderer lineRenderer)
        {
            Vector3[] corners = new Vector3[5];
            Vector2 center = boxCollider.transform.TransformPoint(boxCollider.offset);
            Vector2 size = boxCollider.size * boxCollider.transform.lossyScale;

            corners[0] = center + (new Vector2(-size.x, -size.y) * 0.5f);
            corners[1] = center + (new Vector2(size.x, -size.y) * 0.5f);
            corners[2] = center + (new Vector2(size.x, size.y) * 0.5f);
            corners[3] = center + (new Vector2(-size.x, size.y) * 0.5f);
            corners[4] = corners[0]; // Close the box

            lineRenderer.positionCount = corners.Length;
            lineRenderer.SetPositions(corners);
        }

        private void DrawCircleCollider(CircleCollider2D circleCollider, LineRenderer lineRenderer)
        {
            int segments = 30;
            Vector3[] points = new Vector3[segments + 1];

            Vector2 center = circleCollider.transform.TransformPoint(circleCollider.offset);
            float radius = circleCollider.radius * Mathf.Max(circleCollider.transform.lossyScale.x, circleCollider.transform.lossyScale.y);

            for (int i = 0; i <= segments; i++)
            {
                float angle = (float)i / segments * 2 * Mathf.PI;
                points[i] = center + new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
            }

            lineRenderer.positionCount = points.Length;
            lineRenderer.SetPositions(points);
        }

        private void DrawPolygonCollider(PolygonCollider2D polygonCollider, LineRenderer lineRenderer)
        {
            Vector2[] points = polygonCollider.points;
            Vector3[] worldPoints = new Vector3[points.Length + 1];

            for (int i = 0; i < points.Length; i++)
            {
                worldPoints[i] = polygonCollider.transform.TransformPoint(points[i]);
            }
            worldPoints[points.Length] = worldPoints[0]; // Close the polygon

            lineRenderer.positionCount = worldPoints.Length;
            lineRenderer.SetPositions(worldPoints);
        }

        private void DrawEdgeCollider(EdgeCollider2D edgeCollider, LineRenderer lineRenderer)
        {
            Vector2[] points = edgeCollider.points;
            Vector3[] worldPoints = new Vector3[points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                worldPoints[i] = edgeCollider.transform.TransformPoint(points[i]);
            }

            lineRenderer.positionCount = worldPoints.Length;
            lineRenderer.SetPositions(worldPoints);
        }
    }
}

