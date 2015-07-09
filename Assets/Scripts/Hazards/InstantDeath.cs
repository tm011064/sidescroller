using UnityEngine;
using System.Collections;

public class InstantDeath : MonoBehaviour
{
  void OnTriggerEnter2D(Collider2D col)
  {
    GameManager.instance.powerUpManager.KillPlayer();
  }
}
