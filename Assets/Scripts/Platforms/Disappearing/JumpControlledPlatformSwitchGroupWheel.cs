using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class JumpControlledPlatformSwitchGroupWheel : SpawnBucketItemBehaviour, IObjectPoolBehaviour
{
  #region nested
  public class GameObjectContainer
  {
    public GameObject GameObject;
    public float Angle;
  }

  [Serializable]
  public class JumpSwitchGroup
  {
    public GameObject enabledGameObject;
    public GameObject disabledGameObject;

    [HideInInspector]
    public List<GameObjectContainer> gameObjects;
  }
  #endregion

  public int totalPlatforms = 4;
  public float width = 256f;
  public float height = 128f;
  public float speed = 35f;

  public List<JumpSwitchGroup> platformGroups = new List<JumpSwitchGroup>();
  public int platformGroupStartIndex = 0;

  private ObjectPoolingManager _objectPoolingManager;
  private int _currentEnabledGroupIndex = 0;
  protected PlayerController _playerController;
  private List<Vector3> _worldSpacePlatformCoordinates = new List<Vector3>();

  void OnEnable()
  {
    if (platformGroups.Count < 1)
      throw new ArgumentOutOfRangeException("There must be at least two platform position groups.");

    _playerController = GameManager.instance.player;
    _objectPoolingManager = ObjectPoolingManager.Instance;
   
    for (int i = 0; i < platformGroups.Count; i++)
    {
      platformGroups[i].gameObjects = new List<GameObjectContainer>();
    }

    int index = 0;
    float twoPi = Mathf.PI * 2f;
    for (float angle = 0f; angle < twoPi; angle += twoPi / totalPlatforms)
    {

      Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);

      Vector3 rotated = new Vector3(width * Mathf.Cos(angle), height * Mathf.Sin(angle), 0.0f);
      rotated = q * rotated + this.transform.position;

      GameObject platform = null; // note: we do allow null game objects in case it is a wheel with a single platform switch group
      if (platformGroups[index].disabledGameObject != null)
      {
        platform = _objectPoolingManager.GetObject(platformGroups[index].disabledGameObject.name);
        platform.transform.position = rotated;
      }
      platformGroups[index].gameObjects.Add(new GameObjectContainer() { GameObject = platform, Angle = angle });

      index = index < platformGroups.Count - 1 ? index + 1 : 0;
    }

    if (platformGroupStartIndex < 0 || platformGroupStartIndex >= platformGroups.Count)
      SwitchGroups(0);
    else
      SwitchGroups(platformGroupStartIndex);

    _playerController.OnJumpedThisFrame += _playerController_OnJumpedThisFrame;
  }

  void Update()
  {
    float angleToRotate = speed * Mathf.Deg2Rad * Time.deltaTime;

    for (int i = 0; i < platformGroups.Count; i++)
    {
      for (int j = 0; j < platformGroups[i].gameObjects.Count; j++)
      {
        if (platformGroups[i].gameObjects[j].GameObject != null)
        {
          platformGroups[i].gameObjects[j].Angle += angleToRotate;
          Quaternion q = Quaternion.AngleAxis(platformGroups[i].gameObjects[j].Angle, Vector3.forward);

          Vector3 rotated = new Vector3(width * Mathf.Cos(platformGroups[i].gameObjects[j].Angle), height * Mathf.Sin(platformGroups[i].gameObjects[j].Angle), 0.0f);
          rotated = q * rotated + this.transform.position;

          platformGroups[i].gameObjects[j].GameObject.transform.position = rotated;
        }
      }
    }
  }

  void OnDisable()
  {
    for (int i = 0; i < platformGroups.Count; i++)
    {
      for (int j = 0; j < platformGroups[i].gameObjects.Count; j++)
      {
        if (platformGroups[i].gameObjects[j].GameObject != null)
        {
          _objectPoolingManager.Deactivate(platformGroups[i].gameObjects[j].GameObject);
        }
        platformGroups[i].gameObjects[j] = null;
      }
    }

    _playerController.OnJumpedThisFrame -= _playerController_OnJumpedThisFrame;
  }

  private void SwitchGroups(int enabledIndex)
  {
    _currentEnabledGroupIndex = enabledIndex;
    for (int i = 0; i < platformGroups.Count; i++)
    {
      for (int j = 0; j < platformGroups[i].gameObjects.Count; j++)
      {
        if (platformGroups[i].gameObjects[j].GameObject != null)
        {
          Vector3 position = platformGroups[i].gameObjects[j].GameObject.transform.position;

          _objectPoolingManager.Deactivate(platformGroups[i].gameObjects[j].GameObject);

          platformGroups[i].gameObjects[j].GameObject = _objectPoolingManager.GetObject(
            _currentEnabledGroupIndex == i
              ? platformGroups[i].enabledGameObject.name
              : platformGroups[i].disabledGameObject.name
            , position);
        }
      }
    }
  }

  void _playerController_OnJumpedThisFrame()
  {
    SwitchGroups(_currentEnabledGroupIndex + 1 >= platformGroups.Count ? 0 : _currentEnabledGroupIndex + 1);
  }

  #region IObjectPoolBehaviour Members

  public List<ObjectPoolRegistrationInfo> GetObjectPoolRegistrationInfos()
  {
    List<ObjectPoolRegistrationInfo> list = new List<ObjectPoolRegistrationInfo>();

    for (int i = 0; i < platformGroups.Count; i++)
    {
      if (platformGroups[i].enabledGameObject != null)
      {
        list.Add(new ObjectPoolRegistrationInfo(platformGroups[i].enabledGameObject, ((totalPlatforms + 1) / platformGroups.Count)));
      }
      if (platformGroups[i].disabledGameObject != null)
      {
        list.Add(new ObjectPoolRegistrationInfo(platformGroups[i].disabledGameObject, ((totalPlatforms + 1) / platformGroups.Count)));
      }
    }

    return list;
  }

  #endregion
}