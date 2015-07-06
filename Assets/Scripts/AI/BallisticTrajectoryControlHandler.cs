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
    : base(characterPhysicsManager, float.MaxValue)
  {
    base.doDrawDebugBoundingBox = true;
    base.debugBoundingBoxColor = Color.cyan;

    characterPhysicsManager.onControllerBecameGrounded += characterPhysicsManager_onControllerBecameGrounded;

    _gravity = gravity;
    _velocity = GetBallisticVel(endPosition, startPosition, angle);

    Logger.Trace("Ballistic start velocity: " + _velocity);
  }

  void characterPhysicsManager_onControllerBecameGrounded(GameObject obj)
  {
    _keepAlive = false;
  }

  private Vector3 GetBallisticVel(Vector3 targetPosition, Vector3 startPosition, float angle)
  {
    var dir = targetPosition - startPosition;  // get target direction
    var h = dir.y;  // get height difference
    dir.y = 0;  // retain only the horizontal direction
    var dist = dir.magnitude;  // get horizontal distance
    var a = angle * Mathf.Deg2Rad;  // convert angle to radians
    dir.y = dist * Mathf.Tan(a);  // set dir to the elevation angle
    dist += h / Mathf.Tan(a);  // correct for small height differences
    // calculate the velocity magnitude
    //var vel = Mathf.Sqrt(dist * _gravity / Mathf.Sin(2 * a));
    var vel = Mathf.Sqrt(dist * Math.Abs(_gravity) / Mathf.Sin(2 * a));
    return vel * dir.normalized;
  }

  protected override bool DoUpdate()
  {
    _velocity.y += _gravity * Time.deltaTime;
    _characterPhysicsManager.move(_velocity * Time.deltaTime);

    return _keepAlive;
  }
}

