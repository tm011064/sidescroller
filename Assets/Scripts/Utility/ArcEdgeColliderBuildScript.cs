using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ArcEdgeColliderBuildScript : MonoBehaviour
{
  public int totalSegments = 12;
  public float radius = 256f;
  public float startAngle = 30f;
  public float endAngle = 120f;

  private EdgeCollider2D _edgeCollider;

  private LineRenderer _lineRenderer;

  void Start()
  {
    _lineRenderer = this.GetComponent<LineRenderer>();
  }

  public void BuildObject()
  {
    Logger.Info("Start building edge collider.");

    _edgeCollider = this.GetComponent<EdgeCollider2D>();
    if (_edgeCollider == null)
    {
      Logger.Info("Adding edge collider component.");
      _edgeCollider = this.gameObject.AddComponent<EdgeCollider2D>();
    }
    List<Vector2> vectors = new List<Vector2>();

    float startAngleRad = startAngle * Mathf.Deg2Rad;
    float endAngleRad = endAngle * Mathf.Deg2Rad;
    float totalAngle = Mathf.Abs(startAngleRad - endAngleRad);

    float max = totalAngle;// 2.0f * Mathf.PI;
    float step = max / (totalSegments);

    if (_lineRenderer == null)
      _lineRenderer = this.GetComponent<LineRenderer>();

    if (_lineRenderer != null)
      _lineRenderer.SetVertexCount(totalSegments + 1);

    int index = 0;
    for (float theta = startAngleRad; theta < endAngleRad + step / 2; theta += step)
    {
      Vector2 vector = new Vector2((float)(radius * Mathf.Cos(theta)), (float)(radius * Mathf.Sin(theta)));
      vectors.Add(vector);
      if (_lineRenderer != null)
        _lineRenderer.SetPosition(index++, new Vector3(vector.x, vector.y));
    }

    _edgeCollider.points = vectors.ToArray();
  }
}
