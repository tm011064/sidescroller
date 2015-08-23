using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public partial class CameraTrolley : MonoBehaviour
{
  public List<Vector3> nodes = new List<Vector3>() { Vector3.zero, Vector3.zero };
  public int nodeCount;

  public Color outlineGizmoColor = Color.white;
  public bool showGizmoOutline = true;

  [HideInInspector]
  public bool isPlayerWithinBoundingBox;

  void OnTriggerEnter2D(Collider2D col)
  {
    isPlayerWithinBoundingBox = true;
  }
  void OnTriggerExit2D(Collider2D col)
  {
    isPlayerWithinBoundingBox = false;
  }

  public float? GetPositionY(float posX)
  {
    for (int i = 1; i < nodeCount; i++)
    {
      if (nodes[i - 1].x <= posX && nodes[i].x >= posX)
      {
        Logger.Assert(nodes[i - 1].x != nodes[i].x, "Vertical lines not supported");

        var m = (nodes[i - 1].y - nodes[i].y) / (nodes[i - 1].x - nodes[i].x);

        Vector3 point = this.transform.TransformPoint(0f, m * posX - m * nodes[i - 1].x + nodes[i - 1].y, 0f);
        return point.y;
      }
    }

    return null;
  }

  void OnEnable()
  {

  }

}
