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
    : base(projectileController)
  {
    _ballisticTrajectorySettings = ballisticTrajectorySettings;

    _velocity = DynamicsUtility.GetBallisticVelocity(
      _projectileController.gameObject.transform.position + new Vector3(ballisticTrajectorySettings.endPosition.x, ballisticTrajectorySettings.endPosition.y, _projectileController.gameObject.transform.position.z)
      , _projectileController.gameObject.transform.position
      , ballisticTrajectorySettings.angle
      , ballisticTrajectorySettings.projectileGravity);
  }
}