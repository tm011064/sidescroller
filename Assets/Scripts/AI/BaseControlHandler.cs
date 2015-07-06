using UnityEngine;
using System.Collections;
using System;

public class BaseControlHandler : IDisposable
{
  protected float _overrideEndTime;
  protected CharacterPhysicsManager _characterPhysicsManager;

  protected Color debugBoundingBoxColor = Color.green;
  protected bool doDrawDebugBoundingBox = false;

  public virtual void DrawGizmos()
  {
    if (doDrawDebugBoundingBox)
    {
      GizmoUtility.DrawBoundingBox(_characterPhysicsManager.transform.position
       , _characterPhysicsManager.boxCollider.bounds.extents, debugBoundingBoxColor);
    }
  }

  public virtual void OnEnemyMoveMaskCollision(Collider2D col)
  {

  }

  protected virtual void SetAnimation()
  {

  }

  protected virtual void BeforeHandlerPop()
  {
  }

  public virtual void Dispose() { /* can be overridden */}

  /// <summary>
  /// Updates this instance.
  /// </summary>
  /// <returns>true if update succeeded, false if handler should be removed...</returns>
  public bool Update()
  {
    if (this._overrideEndTime < Time.time)
    {
      BeforeHandlerPop();
      return false;
    }

    bool doUpdate = DoUpdate();

    SetAnimation();

    if (!doUpdate)
      BeforeHandlerPop();

    return doUpdate;
  }

  protected virtual bool DoUpdate() { return true; }

  public BaseControlHandler(CharacterPhysicsManager characterPhysicsManager, float overrideEndTime)
  {
    _characterPhysicsManager = characterPhysicsManager;
    _overrideEndTime = overrideEndTime;
  }
}
