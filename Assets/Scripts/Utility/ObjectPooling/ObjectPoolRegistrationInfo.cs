using UnityEngine;

public class ObjectPoolRegistrationInfo
{
  public GameObject gameObject;
  public int totalInstances;

  public ObjectPoolRegistrationInfo Clone()
  {
    return new ObjectPoolRegistrationInfo(this.gameObject, this.totalInstances);
  }

  public ObjectPoolRegistrationInfo(GameObject gameObject, int totalInstances)
  {
    this.gameObject = gameObject;
    this.totalInstances = totalInstances;
  }
}
