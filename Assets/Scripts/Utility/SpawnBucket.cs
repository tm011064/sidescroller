using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;

public partial class SpawnBucket : BaseMonoBehaviour
{
  public float visibiltyCheckInterval = .1f;

  [SerializeField]
  [InspectorReadOnlyAttribute]
  [Tooltip("Don't edit this, use the 'Register Child Objects' button instead")]
  private SpawnBucketItemBehaviour[] _children = new SpawnBucketItemBehaviour[0];

  protected override void OnGotVisible()
  {
    for (int i = 0; i < _children.Length; i++)
    {
      _children[i].gameObject.SetActive(true);
    }
  }
  protected override void OnGotHidden()
  {
    for (int i = 0; i < _children.Length; i++)
    {
      _children[i].gameObject.SetActive(false);
    }
  }

  void OnEnable()
  {
    for (int i = 0; i < _children.Length; i++)
    {
      if (_children[i].gameObject.activeSelf)
        _children[i].gameObject.SetActive(false);
    }
  }

  public void Reload()
  {
    for (int i = 0; i < _children.Length; i++)
    {
      if (_children[i].gameObject.activeSelf)
        _children[i].gameObject.SetActive(false);
    }
    _isVisible = false;
  }

  void Start()
  {
    this.StartVisibilityChecks(visibiltyCheckInterval, this.GetComponent<Collider2D>());
  }
}
