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
      if (!_areGizmosInitialized)
      {
        BoxCollider2D boxCollider2D = this.GetComponent<BoxCollider2D>();
        if (boxCollider2D != null)
        {
          _gizmoCenter = boxCollider2D.offset;
          _gizmoExtents = boxCollider2D.size / 2;
        }

        _areGizmosInitialized = true;
      }

      GizmoUtility.DrawBoundingBox(this.transform.TransformPoint(_gizmoCenter), _gizmoExtents, outlineGizmoColor);
    }
  }

  public void RegisterChildObjects()
  {
    _children = this.gameObject.GetComponentsInChildren<SpawnBucketItemBehaviour>();
  }

  //private void AddComponentsRecursively(List<SpawnBucketItemBehaviour> children, GameObject gameObject)
  //{
  //  foreach (SpawnBucketItemBehaviour spawnBucketItemBehaviour in gameObject.GetComponents<SpawnBucketItemBehaviour>())
  //  {
  //    Type type = spawnBucketItemBehaviour.GetType();

  //    foreach (FieldInfo fieldInfo in type.GetFields())
  //    {
  //      if (fieldInfo.IsDefined(typeof(SpawnableItemAttribute), true))
  //      {
  //        GameObject field = fieldInfo.GetValue(spawnBucketItemBehaviour) as GameObject;
  //        if (field != null)
  //        {
  //          AddComponentsRecursively(children, field);
  //        }
  //      }
  //    }

  //    children.Add(spawnBucketItemBehaviour);
  //  }

  //  for (int i = 0; i < gameObject.transform.childCount; i++)
  //  {
  //    GameObject child = gameObject.transform.GetChild(i).gameObject;
  //    AddComponentsRecursively(children, child);
  //  }
  //}

  //public void RegisterChildObjects()
  //{
  //  List<SpawnBucketItemBehaviour> children = new List<SpawnBucketItemBehaviour>();
  //  AddComponentsRecursively(children, this.gameObject);
  //  _children = children;
  //}
}
#endif
