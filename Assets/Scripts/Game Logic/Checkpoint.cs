using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour
{
  public int index = 0;

  void OnTriggerEnter2D(Collider2D collider)
  {
    GameManager.instance.player.spawnLocation = this.transform.position;
  }
}
