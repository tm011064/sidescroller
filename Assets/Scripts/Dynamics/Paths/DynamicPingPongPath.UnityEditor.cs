#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class DynamicPingPongPath : SpawnBucketItemBehaviour
{
  void OnDrawGizmos()
  {
    if (showGizmoOutline)
    {
      Gizmos.color = outlineGizmoColor;
      for (int i = 1; i < nodes.Count; i++)
      {
        Gizmos.DrawLine(this.gameObject.transform.TransformPoint(nodes[i - 1]), this.gameObject.transform.TransformPoint(nodes[i]));
      }
    }
  }
}
#endif