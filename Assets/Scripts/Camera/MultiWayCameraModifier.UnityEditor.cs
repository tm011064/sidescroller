#if UNITY_EDITOR

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public partial class MultiWayCameraModifier : MonoBehaviour
{
  private struct BoxVertices
  {
    public Vector2 leftTop;
    public Vector2 rightTop;
    public Vector2 leftBottom;
    public Vector2 rightBottom;
  }

  private bool _areGizmosInitialized = false;
  private BoxVertices _greenBox;
  private BoxVertices _redBox;
  
  private void SetColoredBoxVertices()
  {
    EdgeCollider2D edgeCollider = this.GetComponent<EdgeCollider2D>();

    Vector2 normal = new Vector2(-edgeCollider.points[1].y, edgeCollider.points[1].x).normalized;

    float boxWidth = 128f;

    _greenBox.leftTop = edgeCollider.points[1];
    _greenBox.rightTop = edgeCollider.points[1] - normal * boxWidth;
    _greenBox.rightBottom = edgeCollider.points[0] - normal * boxWidth;
    _greenBox.leftBottom = edgeCollider.points[0];

    _redBox.rightTop = edgeCollider.points[1];
    _redBox.leftTop = edgeCollider.points[1] + normal * boxWidth;
    _redBox.leftBottom = edgeCollider.points[0] + normal * boxWidth;
    _redBox.rightBottom = edgeCollider.points[0];
  }

  void OnDrawGizmos()
  {
    if (!_areGizmosInitialized)
    {
      SetColoredBoxVertices();
      _areGizmosInitialized = true;
    }

    Gizmos.color = Color.green;
    Gizmos.DrawLine(this.transform.TransformPoint(_greenBox.leftTop), this.transform.TransformPoint(_greenBox.rightTop));
    Gizmos.DrawLine(this.transform.TransformPoint(_greenBox.rightTop), this.transform.TransformPoint(_greenBox.rightBottom));
    Gizmos.DrawLine(this.transform.TransformPoint(_greenBox.rightBottom), this.transform.TransformPoint(_greenBox.leftBottom));

    Gizmos.color = Color.red;
    Gizmos.DrawLine(this.transform.TransformPoint(_redBox.rightTop), this.transform.TransformPoint(_redBox.leftTop));
    Gizmos.DrawLine(this.transform.TransformPoint(_redBox.leftTop), this.transform.TransformPoint(_redBox.leftBottom));
    Gizmos.DrawLine(this.transform.TransformPoint(_redBox.leftBottom), this.transform.TransformPoint(_redBox.rightBottom));

    Gizmos.color = Color.white;
    Gizmos.DrawLine(this.transform.TransformPoint(_greenBox.leftBottom), this.transform.TransformPoint(_greenBox.leftTop));
  }

  public void BuildObject()
  {
    EdgeCollider2D edgeCollider = this.GetComponent<EdgeCollider2D>();

    edgeCollider.hideFlags = HideFlags.NotEditable;

    List<Vector2> points = new List<Vector2>();
    points.Add(Vector2.zero);

    float x = Mathf.Sin(edgeColliderAngle * Mathf.Deg2Rad) * edgeColliderLength;
    float y = Mathf.Cos(edgeColliderAngle * Mathf.Deg2Rad) * edgeColliderLength;
    points.Add(new Vector2(x, y));

    edgeCollider.points = points.ToArray();

    SetColoredBoxVertices();
  }
}
#endif