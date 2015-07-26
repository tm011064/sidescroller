using UnityEngine;
using System.Collections;

public class InstantDeathHazardTrigger : MonoBehaviour
{
  void OnTriggerEnter2D(Collider2D col)
  {
    GameManager.instance.powerUpManager.KillPlayer();
  }
}
