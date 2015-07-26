using System;
using UnityEngine;

[Serializable]
public class BallisticTrajectorySettings
{
  public bool isEnabled = false;

  public float angle = 2f;
  public float projectileGravity = -9.81f;
  public Vector2 endPosition = Vector2.zero;
}