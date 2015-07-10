using UnityEngine;
using System.Collections;
using System;

public class BaseControlHandler : IDisposable
{
  protected GameManager _gameManager;

  protected float? _overrideEndTime;
  protected CharacterPhysicsManager _characterPhysicsManager;

  protected Color debugBoundingBoxColor = Color.green;
  protected bool doDrawDebugBoundingBox = false;

  public virtual void DrawGizmos() { /* can be overridden */ }
  public virtual void OnEnemyMoveMaskCollision(Collider2D col) { /* can be overridden */ }
  protected virtual void OnAfterUpdate() { /* can be overridden */ }
  public virtual void Dispose() { /* can be overridden */}

  /// <summary>
  /// Updates this instance.
  /// </summary>
  /// <returns>true if update succeeded, false if handler should be removed...</returns>
  public bool Update()
  {
    if (this._overrideEndTime.HasValue && this._overrideEndTime < Time.time)
    {
      return false;
    }

    bool doUpdate = DoUpdate();

    OnAfterUpdate();
    
    return doUpdate;
  }

  protected virtual bool DoUpdate() { return true; }

  public BaseControlHandler(CharacterPhysicsManager characterPhysicsManager)
    : this(characterPhysicsManager, -1) { }
  public BaseControlHandler(CharacterPhysicsManager characterPhysicsManager, float duration)
  {
    _gameManager = GameManager.instance;
    _characterPhysicsManager = characterPhysicsManager;
    _overrideEndTime = duration >= 0f ? (float?)(Time.time + duration) : null;
  }
}
