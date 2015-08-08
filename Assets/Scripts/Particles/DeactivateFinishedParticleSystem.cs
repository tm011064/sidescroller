using System.Collections;
using UnityEngine;

public class DeactivateFinishedParticleSystem : MonoBehaviour
{
  private ParticleSystem _particleSystem;

  void Start()
  {
    _particleSystem = this.GetComponent<ParticleSystem>();
    StartCoroutine("DeactivationTriggerCheck");
  }

  IEnumerator DeactivationTriggerCheck()
  {
    bool doBreak = false;
    while (!doBreak)
    {
      if (!_particleSystem.isPlaying)
      {
        ObjectPoolingManager.Instance.Deactivate(_particleSystem.gameObject);
        doBreak = true;
      }

      yield return new WaitForSeconds(.2f);
    }
  }
}