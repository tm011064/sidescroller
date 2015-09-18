using UnityEngine;
using System.Collections;
using System;

public class CheckpointPortal : MonoBehaviour
{
  public bool moveToNextCheckpointIndex = true;
  public int moveToCheckpointIndex = 0;

  void OnTriggerEnter2D(Collider2D col)
  {
    if (moveToNextCheckpointIndex)
    {
      GameManager.instance.SpawnPlayerAtNextCheckpoint(false);
    }
    else
    {
      GameManager.instance.SpawnPlayerAtCheckpoint(moveToCheckpointIndex);
    }
  }

}
