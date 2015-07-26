using System;

public abstract class BaseProjectileControlHandler : IDisposable
{
  #region members
  protected ProjectileController _projectileController;
  #endregion

  #region abstract methods
  public abstract bool Update();
  #endregion

  #region virtual methods
  public virtual void Dispose() { /* can be overridden */}

  /// <summary>
  /// This method is called from the BaseCharacterController control handler stack in order to evaluate whether the
  /// top stack element can be activated or not. By default this method always returns true but can be overridden
  /// for special purposes or chained control handlers.
  /// </summary>
  /// <param name="previousControlHandler">The last active control handler.</param>
  /// <returns>true if activation was successful; false if not.</returns>
  public virtual bool TryActivate(BaseProjectileControlHandler previousControlHandler)
  {
    Logger.Trace("Activated control handler: " + this.ToString());
    return true;
  }
  #endregion

  #region constructors
  public BaseProjectileControlHandler(ProjectileController projectileController)
  {
    _projectileController = projectileController;
  }
  #endregion
}
