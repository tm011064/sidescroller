using UnityEngine;
using System.Collections;
using System;

public class StationaryLaser : MonoBehaviour
{
  private LineRenderer _lineRenderer;

  public Direction direction;
  public LayerMask scanRayCollisionLayers = 0;
  public float beamStartPositionFudgeFactor = 1f;

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
        startPosition += new Vector3(0f, -beamStartPositionFudgeFactor);
        break;
      case Direction.Left:
        vector = -Vector2.right;
        magnitude = Screen.width;
        startPosition += new Vector3(-beamStartPositionFudgeFactor, 0f);
        break;
      case Direction.Right:
        vector = Vector2.right;
        magnitude = Screen.width;
        startPosition += new Vector3(beamStartPositionFudgeFactor, 0f);
        break;
      case Direction.Top:
        vector = Vector2.up;
        magnitude = Screen.height;
        startPosition += new Vector3(0f, beamStartPositionFudgeFactor);
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
