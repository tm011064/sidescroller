using UnityEngine;
using System.Collections;

public class DeathHazard : MonoBehaviour
{
  void OnTriggerEnter2D(Collider2D col)
  {
    // TODO (Roman): do we need this check? should be covered by layer...
    if (col.gameObject.layer == LayerMask.NameToLayer("Player"))
    {
      GameManager.instance.powerUpManager.KillPlayer();
    }
  }
}
