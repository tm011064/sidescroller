﻿using UnityEngine;

public class JumpingRunnerEnemyController : TopBounceableEnemyController
{
  public float speed = 200f;
  public float gravity = -3960f;

  public float jumpHeight = 256f;
  public float jumpInterval = 2f;

  protected override BaseControlHandler ApplyDamageControlHandler
  {
    get { return new DamageTakenPlayerControlHandler(); }
  }

  void Start()
  {
    // we insert in case there is already a control handler attached when spawned by another manager
    InsertControlHandler(0, new JumpingRunnerEnemyControlHandler(this, startDirection));
  }
}

