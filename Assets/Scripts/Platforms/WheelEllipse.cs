using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public partial class WheelEllipse : SpawnBucketItemBehaviour
{
  #region nested
  private class GameObjectContainer
  {
    public GameObject GameObject;
    public float Angle;
  }
  #endregion

  public GameObject floatingAttachedPlatform;
  public float totalPlatforms = 4;
  public float width = 256f;
  public float height = 128f;
  public float speed = 35f;

  private List<GameObjectContainer> _platforms = new List<GameObjectContainer>();
  private bool _isPlayerAttached;
  private BoxCollider2D _visibilityCollider;
  private ObjectPoolingManager _objectPoolingManager;

  void Update()
  {
    if (_platforms.Count > 0)
    {
      float angleToRotate = speed * Mathf.Deg2Rad * Time.deltaTime;
      for (int i = 0; i < _platforms.Count; i++)
      {
        _platforms[i].Angle += angleToRotate;
        Quaternion q = Quaternion.AngleAxis(_platforms[i].Angle, Vector3.forward);

        Vector3 rotated = new Vector3(width * Mathf.Cos(_platforms[i].Angle), height * Mathf.Sin(_platforms[i].Angle), 0.0f);
        rotated = q * rotated + this.transform.position;
                
        _platforms[i].GameObject.transform.position = rotated;
      }
    }
  }

  void OnEnable()
  {
    _objectPoolingManager = ObjectPoolingManager.Instance;
    // TODO (Roman): this should be done at global scene load
    _objectPoolingManager.RegisterPool(floatingAttachedPlatform, (int)totalPlatforms, int.MaxValue);

    Logger.Info("Enabling wheel " + this.name);
    List<GameObjectContainer> platforms = new List<GameObjectContainer>();

    for (float angle = 0f; angle < 360 * Mathf.Deg2Rad; angle += 360 * Mathf.Deg2Rad / totalPlatforms)
    {
      GameObject platform = _objectPoolingManager.GetObject(floatingAttachedPlatform.name);
      
      Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);

      Vector3 rotated = new Vector3(width * Mathf.Cos(angle), height * Mathf.Sin(angle), 0.0f);
      rotated = q * rotated + this.transform.position;

      platform.transform.position = rotated;
      platforms.Add(new GameObjectContainer() { GameObject = platform, Angle = angle });
    }

    _platforms = platforms;
  }

  void OnDisable()
  {
    Logger.Info("Disabling wheel " + this.name);

    for (int i = _platforms.Count - 1; i >= 0; i--)
    {
      _objectPoolingManager.Deactivate(_platforms[i].GameObject);
    }
    _platforms.Clear();
  }
}

