using UnityEngine;

public partial class DeactivatePooledObjectTrigger : MonoBehaviour
{
  void OnTriggerEnter2D(Collider2D col)
  {
    Logger.Trace("Game object " + col.gameObject.name + " [" + col.gameObject.GetHashCode() + "] triggered destroy at hit point " + col.gameObject.transform.position + ". This object will be deactivated.");
    
    ObjectPoolingManager.Instance.Deactivate(col.gameObject);
  }
}
