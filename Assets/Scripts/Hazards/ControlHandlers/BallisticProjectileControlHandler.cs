using UnityEngine;

public class BallisticProjectileControlHandler : BaseProjectileControlHandler
{
  private Vector2 _velocity;
  private BallisticTrajectorySettings _ballisticTrajectorySettings;

  public override bool Update()
  {
    _velocity.y += _ballisticTrajectorySettings.projectileGravity * Time.deltaTime;
    _projectileController.gameObject.transform.Translate(_velocity * Time.deltaTime, Space.World);

    return true;
  }

  public BallisticProjectileControlHandler(ProjectileController projectileController, BallisticTrajectorySettings ballisticTrajectorySettings)
    : this(projectileController, ballisticTrajectorySettings, null) { }
  public BallisticProjectileControlHandler(ProjectileController projectileController, BallisticTrajectorySettings ballisticTrajectorySettings, Vector3? maxVelocity)
    : base(projectileController)
  {
    _ballisticTrajectorySettings = ballisticTrajectorySettings;

    _velocity = DynamicsUtility.GetBallisticVelocity(
      _projectileController.gameObject.transform.position + new Vector3(ballisticTrajectorySettings.endPosition.x, ballisticTrajectorySettings.endPosition.y, _projectileController.gameObject.transform.position.z)
      , _projectileController.gameObject.transform.position
      , ballisticTrajectorySettings.angle
      , ballisticTrajectorySettings.projectileGravity);

    if (maxVelocity.HasValue)
    {
      if (Mathf.Abs(_velocity.x) > maxVelocity.Value.x)
        _velocity.x = maxVelocity.Value.x * Mathf.Sign(_velocity.x);

      if (Mathf.Abs(_velocity.y) > maxVelocity.Value.y)
        _velocity.y = maxVelocity.Value.y * Mathf.Sign(_velocity.y);
    }
  }
}