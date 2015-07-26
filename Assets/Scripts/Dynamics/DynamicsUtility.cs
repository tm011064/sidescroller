using System;
using UnityEngine;

public static class DynamicsUtility
{
  public static Vector3 GetBallisticVelocity(Vector3 targetPosition, Vector3 startPosition, float angle, float gravity)
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
    var vel = Mathf.Sqrt(dist * Math.Abs(gravity) / Mathf.Sin(2 * a));
    return vel * dir.normalized;
  }
}

