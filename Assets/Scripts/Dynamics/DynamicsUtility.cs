using System;
using UnityEngine;

/// <summary>
/// Good sources:
/// 
/// Ballistic trajectories -> http://hyperphysics.phy-astr.gsu.edu/hbase/traj.html
/// </summary>
public static class DynamicsUtility
{
  public static Vector3 GetBallisticVelocity(Vector3 targetPosition, Vector3 startPosition, float angle, float gravity)
  {
    if (angle == 0f)
      return GetBallisticVelocityForHorizontalLaunch(targetPosition, startPosition, gravity);

    var dir = targetPosition - startPosition;  // get target direction
    var h = dir.y;  // get height difference
    dir.y = 0;  // retain only the horizontal direction
    var dist = dir.magnitude;  // get horizontal distance
    var a = angle * Mathf.Deg2Rad;  // convert angle to radians
    dir.y = dist * Mathf.Tan(a);  // set dir to the elevation angle
    dist += h / Mathf.Tan(a);  // correct for small height differences
    // calculate the velocity magnitude
    var vel = Mathf.Sqrt(dist * Math.Abs(gravity) / Mathf.Sin(2 * a));
    return vel * dir.normalized;
  }

  public static Vector3 GetBallisticVelocityForHorizontalLaunch(Vector3 targetPosition, Vector3 startPosition, float gravity)
  {
    var dir = targetPosition - startPosition;  // get target direction
    var vel = dir.x * Mathf.Sqrt(Mathf.Abs(gravity / (2f * dir.y)));
    return new Vector3(vel, 0f, 0f);
  }
}

