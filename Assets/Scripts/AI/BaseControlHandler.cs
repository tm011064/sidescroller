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
  /// This method is called from the BaseCharacterController control handler stack in order to evaluate whether the
  /// top stack element can be activated or not. By default this method always returns true but can be overridden
  /// for special purposes or chained control handlers.
  /// </summary>
  /// <param name="previousControlHandler">The last active control handler.</param>
  /// <returns>true if activation was successful; false if not.</returns>
  public virtual bool TryActivate(BaseControlHandler previousControlHandler)
  {
    Logger.Trace("Activated control handler: " + this.ToString());
    return true;
  }

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
