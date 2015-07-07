using UnityEngine;

public class TopBounceableEnemyController : EnemyController
{
  public float dropPowerUpChance = .8f;
  [Tooltip("If the player does not press the jump button when hitting the enemy, this multiplier is used to calculate the bounce height.")]
  public float bounceJumpMultiplier = .5f;
  [Tooltip("Specifies how much time the player has to perform a jump after colliding with this enemy.")]
  public float allowJumpFromTopThresholdInSeconds = .3f;
  
  [Tooltip("-45 is the top left corner")]
  public float allowedTopCollisionAngleFrom = -38f;
  [Tooltip("-135 is the top right corner")]
  public float allowedTopCollisionAngleTo = -142f;
  
  private Collider2D _collider;

  void Awake()
  {
    _collider = GetComponent<Collider2D>();    
    PushControlHandler(new PathAttachedEnemyControlHandler());
  }
  
  #region Event Listeners
  
  public override void onPlayerCollide(PlayerController playerController)
  {
    Vector3 dir = transform.position - playerController.transform.position;
    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

    Logger.Info("Player collided with TopBounceableEnemyController enemy. Angle: " + angle);
    if (angle < allowedTopCollisionAngleFrom && angle > allowedTopCollisionAngleTo) 
    {
      playerController.PushControlHandler(new TopBounceableControlHandler(playerController, Time.time + allowJumpFromTopThresholdInSeconds, bounceJumpMultiplier));

      // TODO (Roman): power up drop code
      if (dropPowerUpChance >= Random.Range(.0f, 1.0f))
      {
        GameObject obj = ObjectPoolingManager.Instance.GetObject(GameManager.instance.gameSettings.pooledObjects.basicPowerUpPrefab.prefab.name);
        obj.transform.position = this.transform.position;
        obj.SetActive(true);
      }

      // TODO (Roman): run death animation...
      this.gameObject.SetActive(false);
    }
    else
    {
      // TODO (Roman): 
      //playerController.PushControlHandler(new TrooperHitControlHandler(playerController));
      GameManager.instance.powerUpManager.AddDamage();
    }
  }

  #endregion
}

