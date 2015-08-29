using UnityEngine;

public class FlyingFollowerEnemyControlHandler : EnemyControlHandler<FlyingFollowerEnemyController>
{
  private PlayerController _playerController;
  private Vector3 _velocity = Vector3.zero;

  public FlyingFollowerEnemyControlHandler(FlyingFollowerEnemyController flyingFollowerEnemyController)
    : base(flyingFollowerEnemyController)
  {
    _playerController = GameManager.instance.player;
  }


  protected override bool DoUpdate()
  {

    /*
    transform.position = new Vector3(
      Mathf.SmoothDamp(transform.position.x, targetPositon.x, ref _horizontalSmoothDampVelocity, _cameraMovementSettings.smoothDampMoveSettings.horizontalSmoothDampTime)
      , Mathf.SmoothDamp(transform.position.y, targetPositon.y, ref _verticalSmoothDampVelocity, verticalSmoothDampTime)
      , targetPositon.z);
    velocity.x = Mathf.Lerp(velocity.x, 0f, _onTrampolineSkidDamping * Time.deltaTime);
    */

    Vector3 direction = _playerController.transform.position - this._enemyController.transform.position;
    direction = direction.normalized * _enemyController.speed * Time.deltaTime;

    _velocity.x = Mathf.Lerp(_velocity.x, direction.x, _enemyController.smoothDampFactor * Time.deltaTime);
    _velocity.y = Mathf.Lerp(_velocity.y, direction.y, _enemyController.smoothDampFactor * Time.deltaTime);

    _enemyController.transform.Translate(_velocity);

    return true;
  }
}
