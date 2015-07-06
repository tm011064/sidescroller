using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public partial class Wheel : BaseMonoBehaviour
{
  #region nested
  private class GameObjectContainer
  {
    public GameObject GameObject;
    public float Angle;
  }
  #endregion

  public GameObject floatingAttachedPlatform;
  public float totalPlatforms = 3;
  public float radius = 256f;
  public float speed = 200f;
  public float visibiltyCheckInterval = .1f;

  public int platformWidth = 128;
  public int platformHeight = 16;
  private float _visibilityCheckRadiusMultiplier = 4f;
  private List<GameObjectContainer> _platforms;
  private bool _isPlayerAttached;
  private BoxCollider2D _visibilityCollider;
  
  void Update()
  {
    if (_platforms != null)
    {
      float angleToRotate = speed * Mathf.Deg2Rad * Time.deltaTime;
      for (int i = 0; i < _platforms.Count; i++)
      {
        _platforms[i].Angle += angleToRotate;

        Vector3 initial = new Vector3(transform.position.x + radius, transform.position.y, transform.position.z);
        Vector3 rotated = new Vector3(
          Mathf.Cos(_platforms[i].Angle) * (initial.x - transform.position.x) - Mathf.Sin(_platforms[i].Angle) * (initial.y - transform.position.y) + transform.position.x
          , Mathf.Sin(_platforms[i].Angle) * (initial.x - transform.position.x) - Mathf.Cos(_platforms[i].Angle) * (initial.y - transform.position.y) + transform.position.y
          , transform.position.z);

        _platforms[i].GameObject.transform.position = rotated;
      }
    }
  }

  protected override void OnGotVisible()
  {
    Logger.Info("Wheel " + this.name + " got visible");
    _platforms = new List<GameObjectContainer>();

    for (float angle = 0f; angle < 360 * Mathf.Deg2Rad; angle += 360 * Mathf.Deg2Rad / totalPlatforms)
    {
      GameObject platform = ObjectPoolingManager.Instance.GetObject(floatingAttachedPlatform.name);

      var groundPlatformSpriteRenderer = platform.GetComponent<GroundPlatformSpriteRenderer>();
      groundPlatformSpriteRenderer.SetBoundaries(platformWidth/2, platformHeight/2, platformWidth, platformHeight);

      Vector3 initial = new Vector3(transform.position.x + radius, transform.position.y, transform.position.z);

      Vector3 rotated = new Vector3(
        Mathf.Cos(angle) * (initial.x - transform.position.x) - Mathf.Sin(angle) * (initial.y - transform.position.y) + transform.position.x
        , Mathf.Sin(angle) * (initial.x - transform.position.x) - Mathf.Cos(angle) * (initial.y - transform.position.y) + transform.position.y
        , transform.position.z);

      platform.transform.position = rotated;
      platform.SetActive(true);

      _platforms.Add(new GameObjectContainer() { GameObject = platform, Angle = angle });
    }
  }
  protected override void OnGotHidden()
  {
    Logger.Info("Wheel " + this.name + " got hidden");
    for (int i = 0; i < _platforms.Count; i++)
    {
      _platforms[i].GameObject.SetActive(false);
    }
    _platforms = null;
  }

  void Start()
  {
    // we wanna do this in start as we know that the player has been added to the game context
    ObjectPoolingManager.Instance.RegisterPool(floatingAttachedPlatform, (int)totalPlatforms, int.MaxValue);

    _visibilityCollider = this.gameObject.AddComponent<BoxCollider2D>();
    _visibilityCollider.isTrigger = true;
        
    // note: multiplying the radius by _visibilityCheckRadiusMultiplier is not ideal as we would really want to get the radius + floatingplatform size.
    _visibilityCollider.size = new Vector2(
      (radius + platformWidth) / (this.gameObject.transform.localScale.x == 0f ? 1f : this.gameObject.transform.localScale.x)
      , (radius + platformHeight) / (this.gameObject.transform.localScale.y == 0f ? 1f : this.gameObject.transform.localScale.y)
    );

    StartVisibilityChecks(visibiltyCheckInterval, _visibilityCollider);
  }
}

