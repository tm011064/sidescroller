using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public partial class CameraTrolley : MonoBehaviour
{
  public int nodeCount;
  [HideInInspector]
  public List<Vector3> nodes = new List<Vector3>() { Vector3.zero, Vector3.zero };
  [HideInInspector]
  public bool isPlayerWithinBoundingBox;

  private Vector3[] _worldCoordinateNodes = null;

  void OnTriggerEnter2D(Collider2D col)
  {
    isPlayerWithinBoundingBox = true;
  }
  void OnTriggerExit2D(Collider2D col)
  {
    isPlayerWithinBoundingBox = false;
  }

  void OnEnable()
  {
    if (_worldCoordinateNodes == null)
    {
      _worldCoordinateNodes = new Vector3[nodes.Count];
      for (int i = 0; i < nodes.Count; i++)
      {
        _worldCoordinateNodes[i] = this.transform.TransformPoint(nodes[i]);

        if (i > 0 && _worldCoordinateNodes[i - 1].x == _worldCoordinateNodes[i].x)
          throw new ArgumentOutOfRangeException("Vertical lines not supported.");
      }
    }
  }

  public float? GetPositionY(float posX)
  {
    for (int i = 1; i < nodeCount; i++)
    {
      if (_worldCoordinateNodes[i - 1].x <= posX && _worldCoordinateNodes[i].x >= posX)
      {
        float m = (_worldCoordinateNodes[i - 1].y - _worldCoordinateNodes[i].y) / (_worldCoordinateNodes[i - 1].x - _worldCoordinateNodes[i].x);
        return m * posX - m * _worldCoordinateNodes[i - 1].x + _worldCoordinateNodes[i - 1].y;
      }
    }

    return null;
  }
}
