using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class OneWayPlatform : MonoBehaviour
{
  #region fields
  Collider2D _collider;
  #endregion

  #region methods

  #region private
  private void ResetLayer()
  {
    this._collider.enabled = true;
  }
  #endregion

  #region public
  public void TriggerFall()
  {
    this._collider.enabled = false;
    Invoke("ResetLayer", .2f); // TODO (Roman): hard coded value
  }
  #endregion
  #endregion

  #region awake/start
  void Awake()
  {
    _collider = this.GetComponent<Collider2D>();
  }
  #endregion
}
