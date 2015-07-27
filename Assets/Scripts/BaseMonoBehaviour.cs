using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class BaseMonoBehaviour : MonoBehaviour
{
  #region events
  /// <summary>
  /// Occurs when [got disabled]. Must be set in Awake() method.
  /// </summary>
  public event Action<BaseMonoBehaviour> GotDisabled;
  /// <summary>
  /// Occurs when [got enabled]. Must be set in Awake() method.
  /// </summary>
  public event Action<BaseMonoBehaviour> GotEnabled;

  /// <summary>
  /// Occurs when [got visible]. Must be set in Awake() method.
  /// </summary>
  public event Action<BaseMonoBehaviour> GotVisible;
  /// <summary>
  /// Occurs when [got hidden]. Must be set in Awake() method.
  /// </summary>
  public event Action<BaseMonoBehaviour> GotHidden;
  #endregion

  #region private members
  protected bool _isVisible;

  private Collider2D _visibilityCheckCollider;
  private Renderer _visibilityCheckRenderer;

  private Func<bool> _testVisibility;
  #endregion

  #region virtual methods
  protected virtual void OnGotVisible() { /* can be overridden */}
  protected virtual void OnGotHidden() { /* can be overridden */}
  #endregion

  #region public fields
  /// <summary>
  /// Can be set at Awake() in scripts
  /// </summary>
  private float _visibiltyCheckInterval = 0f;
  #endregion

  #region Unity Script Methods
  void OnEnable()
  {
    var handler = this.GotEnabled;
    if (handler != null)
      handler(this);
  }

  void OnDisable()
  {
    _isVisible = false; // we have to set this

    var handler = this.GotDisabled;
    if (handler != null)
      handler(this);
  }

  #endregion

  #region methods
  protected void StartVisibilityChecks(float visibiltyCheckInterval, Collider2D collider)
  {
    if (visibiltyCheckInterval > 0f)
    {
      _visibiltyCheckInterval = visibiltyCheckInterval;
      _visibilityCheckCollider = collider;
      _testVisibility = (() => { return _visibilityCheckCollider.IsVisibleFrom(Camera.main); });

      StartCoroutine(CheckVisibility());
    }
  }
  protected void StartVisibilityChecks(float visibiltyCheckInterval, Renderer renderer)
  {
    if (visibiltyCheckInterval > 0f)
    {
      _visibiltyCheckInterval = visibiltyCheckInterval;
      _visibilityCheckRenderer = renderer;
      _testVisibility = (() => { return _visibilityCheckRenderer.IsVisibleFrom(Camera.main); });

      StartCoroutine(CheckVisibility());
    }
  }

  IEnumerator CheckVisibility()
  {
    while (true)
    {
      bool isVisible = _testVisibility();

      if (isVisible && !_isVisible)
      {
        this.OnGotVisible();

        var handler = this.GotVisible;
        if (handler != null)
          handler(this);
      }
      else if (!isVisible && _isVisible)
      {
        this.OnGotHidden();

        var handler = this.GotHidden;
        if (handler != null)
          handler(this);
      }

      _isVisible = isVisible;

      yield return new WaitForSeconds(_visibiltyCheckInterval);
    }
  }
  #endregion
}
