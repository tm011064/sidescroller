using UnityEngine;
using System.Collections;
using System;

public class StationaryLaser : SpawnBucketItemBehaviour
{
  private LineRenderer _lineRenderer;

  [Tooltip("All layers that the scan rays can collide with. Should include platforms and player.")]
  public LayerMask scanRayCollisionLayers = 0;
  [Tooltip("The direction of the laser emitted from the game object's position.")]
  public Direction direction;
  [Tooltip("The offset of the point where the laser gets emitted.")]
  public Vector3 scanRayEmissionPositionOffset = Vector3.zero;

  void Awake()
  {
    _lineRenderer = this.GetComponent<LineRenderer>();
  }
  
  void Update()
  {
    Vector2 vector;
    float magnitude;
    Vector3 startPosition = this.transform.position;
    switch (direction)
    {
      case Direction.Bottom:
        vector = -Vector2.up;
        magnitude = Screen.height;
        startPosition += scanRayEmissionPositionOffset;
        break;
      case Direction.Left:
        vector = -Vector2.right;
        magnitude = Screen.width;
        startPosition += scanRayEmissionPositionOffset;
        break;
      case Direction.Right:
        vector = Vector2.right;
        magnitude = Screen.width;
        startPosition += scanRayEmissionPositionOffset;
        break;
      case Direction.Top:
        vector = Vector2.up;
        magnitude = Screen.height;
        startPosition += scanRayEmissionPositionOffset;
        break;

      default: throw new ArgumentException("Direction " + direction + " not supported.");
    }

    RaycastHit2D raycastHit2D = Physics2D.Raycast(startPosition, vector.normalized, magnitude, scanRayCollisionLayers);
    bool hasHit = raycastHit2D == true;
    if (hasHit && raycastHit2D.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
    {
      GameManager.instance.powerUpManager.KillPlayer();
      hasHit = false;
    }

    if (_lineRenderer.useWorldSpace)
    {
      if (hasHit)
      {
        _lineRenderer.SetPosition(0, this.transform.position);
        _lineRenderer.SetPosition(1, raycastHit2D.point);
      }
      else
      {
        _lineRenderer.SetPosition(0, this.transform.position);
        switch (direction)
        {
          case Direction.Bottom:
            _lineRenderer.SetPosition(1, this.transform.position - new Vector3(0f, -Screen.height));
            break;
          case Direction.Left:
            _lineRenderer.SetPosition(1, this.transform.position - new Vector3(-Screen.width, 0));
            break;
          case Direction.Right:
            _lineRenderer.SetPosition(1, this.transform.position - new Vector3(Screen.width, 0f));
            break;
          case Direction.Top:
            _lineRenderer.SetPosition(1, this.transform.position - new Vector3(0f, Screen.height));
            break;

          default: throw new ArgumentException("Direction " + direction + " not supported.");
        }
      }
    }
    else
    {
      if (hasHit)
      {
        _lineRenderer.SetPosition(0, Vector3.zero);
        _lineRenderer.SetPosition(1, raycastHit2D.point.ToVector3() - this.transform.position);
      }
      else
      {
        _lineRenderer.SetPosition(0, Vector3.zero);
        switch (direction)
        {
          case Direction.Bottom:
            _lineRenderer.SetPosition(1, new Vector3(0f, -Screen.height));
            break;
          case Direction.Left:
            _lineRenderer.SetPosition(1, new Vector3(-Screen.width, 0));
            break;
          case Direction.Right:
            _lineRenderer.SetPosition(1, new Vector3(Screen.width, 0f));
            break;
          case Direction.Top:
            _lineRenderer.SetPosition(1, new Vector3(0f, Screen.height));
            break;

          default: throw new ArgumentException("Direction " + direction + " not supported.");
        }
      }
    }
  }
}
