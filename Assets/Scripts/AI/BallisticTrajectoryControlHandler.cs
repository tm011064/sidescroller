using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class BallisticTrajectoryControlHandler : BaseControlHandler
{
  private float _gravity;
  private Vector3 _velocity;
  private bool _keepAlive = true;

  public BallisticTrajectoryControlHandler(CharacterPhysicsManager characterPhysicsManager, Vector3 startPosition, Vector3 endPosition, float gravity
    , float angle)
    : base(characterPhysicsManager)
  {
    base.doDrawDebugBoundingBox = true;
    base.debugBoundingBoxColor = Color.cyan;

    if (characterPhysicsManager != null)
    {
      characterPhysicsManager.onControllerBecameGrounded += (GameObject obj) =>
      {
        _keepAlive = false;
      };
    }

    _gravity = gravity;
    _velocity = DynamicsUtility.GetBallisticVelocity(endPosition, startPosition, angle, _gravity);

    Logger.Trace("Ballistic start velocity: " + _velocity + ", (startPosition: " + startPosition + ", endPosition: " + endPosition + ", gravity: " + gravity + ", angle: " + angle + ")");
  }
  
  protected override bool DoUpdate()
  {
    _velocity.y += _gravity * Time.deltaTime;
    _characterPhysicsManager.Move(_velocity * Time.deltaTime);

    return _keepAlive;
  }
}

