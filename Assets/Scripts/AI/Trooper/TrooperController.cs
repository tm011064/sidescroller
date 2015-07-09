using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrooperController : EnemyController //MonoBehaviour
{
  public float gravity = -25f;
  public float runSpeed = 8f;
  public float groundDamping = 20f; // how fast do we change direction? higher means faster
  public float inAirDamping = 5f;

  public float dropPowerUpChance = .8f;

  public bool canFallOffPlatform = false;

  private Animator _anim;
  private Collider2D _collider;

  void Awake()
  {
    characterPhysicsManager = GetComponent<CharacterPhysicsManager>();
    _anim = GetComponent<Animator>();
    _collider = GetComponent<Collider2D>();

    // listen to some events for illustration purposes
    characterPhysicsManager.onTriggerEnterEvent += onTriggerEnterEvent;
    characterPhysicsManager.onControllerCollidedEvent += characterPhysicsManager_onControllerCollidedEvent;

    PushControlHandler(new DefaultTrooperControlHandler(this));

    _anim.Play("Idle");
  }
  
  void characterPhysicsManager_onControllerCollidedEvent(RaycastHit2D hit)
  {
    // bail out on plain old ground hits cause they arent very interesting
    if (hit.normal.y == 1f)
      return;
  }

  #region Event Listeners

  void onTriggerEnterEvent(Collider2D col)
  {
    if (col.gameObject.layer == LayerMask.NameToLayer("EnemyMoveMask"))
    {
      this.CurrentControlHandler.OnEnemyMoveMaskCollision(col);
    }
  }

  public override void onPlayerCollide(PlayerController playerController)
  {
    Vector3 dir = transform.position - playerController.transform.position;
    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

    if (angle < -45f && angle > -135f)
    {
      playerController.PushControlHandler(new TrooperBounceControlHandler(playerController));

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
      playerController.PushControlHandler(new TrooperHitControlHandler(playerController));

      GameManager.instance.powerUpManager.AddDamage();
    }
  }

  #endregion

}
