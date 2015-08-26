using UnityEngine;
using System.Collections;

public class EnemyControlHandler<TEnemyController>
  : BaseControlHandler
  where TEnemyController : EnemyController
{
  protected enum PlatformEdgeMoveMode
  {
    TurnAround,
    FallOff
  }

  #region members
  protected TEnemyController _enemyController;
  #endregion

  [System.Diagnostics.Conditional("DEBUG")]
  protected void DrawRay(Vector3 start, Vector3 dir, Color color)
  {
    Debug.DrawRay(start, dir, color);
  }

  protected float? _pauseAtEdgeEndTime = null;

  protected void MoveHorizontally(ref float moveDirectionFactor, float speed, float gravity, PlatformEdgeMoveMode platformEdgeMoveMode, float edgeTurnAroundPause = 0f
    , float? jumpVelocityY = null)
  {
    Vector3 velocity = _characterPhysicsManager.velocity;

    if (_pauseAtEdgeEndTime.HasValue)
    {
      if (_pauseAtEdgeEndTime.Value > Time.time)
      {
        velocity.x = 0f;
        if (jumpVelocityY.HasValue)
          velocity.y = jumpVelocityY.Value;
        velocity.y += gravity * Time.deltaTime;

        _characterPhysicsManager.Move(velocity * Time.deltaTime);
        return;
      }
      else
      {
        // would go over edge, so change direction
        moveDirectionFactor *= -1;

        _pauseAtEdgeEndTime = null;
      }
    }

    // move with constant speed
    velocity.x = moveDirectionFactor * speed;

    if (jumpVelocityY.HasValue)
    {
      velocity.y = jumpVelocityY.Value;
      Debug.Log("JUMP: " + velocity.y);
    }
    else if (_characterPhysicsManager.lastMoveCalculationResult.collisionState.below)
      velocity.y = 0;

    // apply gravity before moving
    velocity.y += gravity * Time.deltaTime;

    MoveCalculationResult moveCalculationResult = _characterPhysicsManager.CalculateMove(velocity * Time.deltaTime);

    switch (platformEdgeMoveMode)
    {
      case PlatformEdgeMoveMode.TurnAround:
        bool isOnEdge = (moveCalculationResult.collisionState.wasGroundedLastFrame && moveCalculationResult.collisionState.below && !moveCalculationResult.collisionState.isFullyGrounded); // we are on edge
        if (
              isOnEdge
              || (moveDirectionFactor < 0f && moveCalculationResult.collisionState.left)
              || (moveDirectionFactor > 0f && moveCalculationResult.collisionState.right)
           )
        {

          if (isOnEdge && edgeTurnAroundPause > 0f)
          {
            velocity.x = 0f;
            _characterPhysicsManager.Move(velocity * Time.deltaTime);
            _pauseAtEdgeEndTime = Time.time + edgeTurnAroundPause;
          }
          else
          {
            // would go over edge, so change direction
            moveDirectionFactor *= -1;

            velocity.x = moveDirectionFactor * speed;
            _characterPhysicsManager.Move(velocity * Time.deltaTime);
          }
        }
        else
        {
          _characterPhysicsManager.PerformMove(moveCalculationResult);
        }
        break;

      case PlatformEdgeMoveMode.FallOff:
        if (
              (moveDirectionFactor < 0f && moveCalculationResult.collisionState.left)
              || (moveDirectionFactor > 0f && moveCalculationResult.collisionState.right)
           )
        {
          // would go over edge, so change direction
          moveDirectionFactor *= -1;

          velocity.x = moveDirectionFactor * speed;
          _characterPhysicsManager.Move(velocity * Time.deltaTime);
        }
        else
        {
          _characterPhysicsManager.PerformMove(moveCalculationResult);
        }
        break;
    }
  }

  #region constructors
  public EnemyControlHandler(TEnemyController enemyController)
    : this(enemyController, -1f) { }
  public EnemyControlHandler(TEnemyController enemyController, float duration)
    : base(enemyController.characterPhysicsManager, duration)
  {
    _enemyController = enemyController;
  }
  #endregion
}
