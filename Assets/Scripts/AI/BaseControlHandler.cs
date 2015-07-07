using UnityEngine;
using System.Collections;
using System;

public class BaseControlHandler : IDisposable
{
  protected float _overrideEndTime;
  protected CharacterPhysicsManager _characterPhysicsManager;

  protected Color debugBoundingBoxColor = Color.green;
  protected bool doDrawDebugBoundingBox = false;

  public virtual void DrawGizmos() { /* can be overridden */ }
  public virtual void OnEnemyMoveMaskCollision(Collider2D col) { /* can be overridden */ }
  protected virtual void OnBeforeHandlerPop() { /* can be overridden */ }
  protected virtual void OnAfterUpdate() { /* can be overridden */ }
  public virtual void Dispose() { /* can be overridden */}

  /// <summary>
  /// Updates this instance.
  /// </summary>
  /// <returns>true if update succeeded, false if handler should be removed...</returns>
  public bool Update()
  {
    if (this._overrideEndTime < Time.time)
    {
      OnBeforeHandlerPop();
      return false;
    }

    bool doUpdate = DoUpdate();

    OnAfterUpdate();

    if (!doUpdate)
      OnBeforeHandlerPop();

    return doUpdate;
  }

  protected virtual bool DoUpdate() { return true; }

  public BaseControlHandler(CharacterPhysicsManager characterPhysicsManager, float overrideEndTime)
  {
    _characterPhysicsManager = characterPhysicsManager;
    _overrideEndTime = overrideEndTime;
  }
}
