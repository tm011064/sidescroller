#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public partial class SpawnBucket : BaseMonoBehaviour
{
  private Vector3 _gizmoCenter = Vector3.zero;
  private Vector3 _gizmoExtents = new Vector3(16, 16, 0);
  private bool _areGizmosInitialized = false;

  public Color outlineGizmoColor = Color.yellow;
  public bool showGizmoOutline = true;

  void OnDrawGizmos()
  {
    if (showGizmoOutline)
    {
      BoxCollider2D boxCollider2D = this.GetComponent<BoxCollider2D>();
      if (boxCollider2D != null)
      {
        _gizmoCenter = boxCollider2D.offset;
        _gizmoExtents = boxCollider2D.size / 2;
      }

      _areGizmosInitialized = true;

      GizmoUtility.DrawBoundingBox(this.transform.TransformPoint(_gizmoCenter), _gizmoExtents, outlineGizmoColor);
    }
  }

  void Awake()
  {
    // if in editor, auto register to facilitate
    RegisterChildObjects();
  }

  public void RegisterChildObjects()
  {
    _children = this.gameObject.GetComponentsInChildren<SpawnBucketItemBehaviour>();
  }
}
#endif
