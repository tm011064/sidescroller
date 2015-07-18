using UnityEngine;
using System.Collections;

public class DeathHazard : MonoBehaviour
{
  void OnTriggerEnter2D(Collider2D col)
  {
    GameManager.instance.powerUpManager.KillPlayer();
  }
}
