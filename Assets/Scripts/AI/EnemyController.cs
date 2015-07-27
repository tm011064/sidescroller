using UnityEngine;

public class EnemyController : BaseCharacterController
{
  public Direction startDirection = Direction.Right;  

  void Awake()
  {
    characterPhysicsManager = GetComponent<CharacterPhysicsManager>();
  }

  public virtual void onPlayerCollide(PlayerController playerController)
  {

  }
}
