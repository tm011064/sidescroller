using UnityEngine;
using System.Collections;
using System;

public class StationarySentryLaser : SpawnBucketItemBehaviour
{
  private LineRenderer _lineRenderer;

  [Tooltip("All layers that the scan rays can collide with. Should include platforms and player.")]
  public LayerMask scanRayCollisionLayers = 0;
  [Tooltip("Total scan rays to use.")]
  public int totalScanRays = 36;
  [Tooltip("Scan rays are sent out in all directions. The start angle defines the start boundary of the line of sight in degrees. 0 degrees means right direction (1,0), going counter clockwise. Example: 0 = (1, 0), 90 = (0, 1), 180 = (-1, 0)")]
  public float scanRayStartAngle = 0f;
  [Tooltip("Scan rays are sent out in all directions. The end angle defines the end boundary of the line of sight in degrees. 0 degrees means right direction (1,0), going counter clockwise. Example: 0 = (1, 0), 90 = (0, 1), 180 = (-1, 0)")]
  public float scanRayEndAngle = 360f;
  [Tooltip("The length of the scan rays emitted from the position of this gameobject.")]
  public float scanRayLength = 1280;

  [Tooltip("Once the sentry laser detects the player, he has some time to get cover before being killed by the laser. This is the variable indicating the time.")]
  public float timeNeededToKillPlayer = .1f;

  private float _startAngleRad;
  private float _endAngleRad;
  private float _step;
  private float _rateOfFireInterval;

  private float _playerInSightDuration;

  private ObjectPoolingManager _objectPoolingManager;
  private PlayerController _playerController;

  [System.Diagnostics.Conditional("DEBUG")]
  private void DrawRay(Vector3 start, Vector3 dir, Color color)
  {
    Debug.DrawRay(start, dir, color);
  }

  void Awake()
  {
    _lineRenderer = this.GetComponent<LineRenderer>();

    _startAngleRad = scanRayStartAngle * Mathf.Deg2Rad;
    _endAngleRad = scanRayEndAngle * Mathf.Deg2Rad;
    _step = (_endAngleRad - _startAngleRad) / (float)totalScanRays;
  }

  void OnEnable()
  {
    _playerController = GameManager.instance.player;
    Logger.Info("Enabled stationary sentry laser " + this.GetHashCode());
  }

  void Update()
  {
    #region now check whether we can see the player
    bool isSeeingPlayer = false;
    for (float theta = _endAngleRad; theta > _startAngleRad - _step / 2; theta -= _step)
    {
      Vector2 vector = new Vector2(Mathf.Cos(theta), Mathf.Sin(theta));
      RaycastHit2D raycastHit2D = Physics2D.Raycast(this.gameObject.transform.position, vector.normalized, scanRayLength, scanRayCollisionLayers);
      if (raycastHit2D)
      {
        if (raycastHit2D.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
          _playerInSightDuration += Time.deltaTime;
          isSeeingPlayer = true;
          DrawRay(this.gameObject.transform.position, _playerController.transform.position - this.gameObject.transform.position, Color.red);
          break;
        }
        else
        {
          DrawRay(this.gameObject.transform.position, raycastHit2D.point.ToVector3() - this.gameObject.transform.position, Color.grey);
        }
      }
      else
      {
        DrawRay(this.gameObject.transform.position, vector * scanRayLength, Color.grey);
      }
    }

    if (!isSeeingPlayer)
      _playerInSightDuration = 0f;

    if (isSeeingPlayer)
    {
      if (!_lineRenderer.enabled)
        _lineRenderer.enabled = true;

      if (_lineRenderer.useWorldSpace)
      {
        _lineRenderer.SetPosition(0, this.transform.position);
        _lineRenderer.SetPosition(1, _playerController.transform.position);
      }
      else
      {
        _lineRenderer.SetPosition(0, Vector3.zero);
        _lineRenderer.SetPosition(1, _playerController.transform.position - this.transform.position);
      }
    }
    else
    {
      _lineRenderer.enabled = false;
    }

    if (_playerInSightDuration >= timeNeededToKillPlayer)
    {
      GameManager.instance.powerUpManager.KillPlayer();
    }

    #endregion
  }
}
