using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class JumpControlledPlatformSwitchGroup : SpawnBucketItemBehaviour
{
  #region nested
  [Serializable]
  public class JumpSwitchGroup
  {
#if UNITY_EDITOR
    public Color outlineGizmoColor = Color.yellow;
    public bool showGizmoOutline = true;
#endif

    public GameObject enabledGameObject;
    public GameObject disabledGameObject;
    public List<Vector3> positions = new List<Vector3>();

    [HideInInspector]
    public Vector3[] worldSpaceCoordinates;
    [HideInInspector]
    public GameObject[] gameObjects;
  }
  #endregion

  public List<JumpSwitchGroup> platformGroupPositions = new List<JumpSwitchGroup>();
  public int platformGroupStartIndex = 0;

  private ObjectPoolingManager _objectPoolingManager;

  private int _currentEnabledGroupIndex = 0;
  protected PlayerController _playerController;

  private List<Vector3> _worldSpacePlatformCoordinates = new List<Vector3>();

  void OnEnable()
  {
    _playerController = GameManager.instance.player;
    if (_objectPoolingManager == null)
    {
      _objectPoolingManager = ObjectPoolingManager.Instance;
      // TODO (Roman): this should be done at camera script
      for (int i = 0; i < platformGroupPositions.Count; i++)
      {
        _objectPoolingManager.RegisterPool(platformGroupPositions[i].enabledGameObject, platformGroupPositions[i].positions.Count, int.MaxValue);
        _objectPoolingManager.RegisterPool(platformGroupPositions[i].disabledGameObject, platformGroupPositions[i].positions.Count, int.MaxValue);

        platformGroupPositions[i].gameObjects = new GameObject[platformGroupPositions[i].positions.Count];
        platformGroupPositions[i].worldSpaceCoordinates = new Vector3[platformGroupPositions[i].positions.Count];
        for (int j = 0; j < platformGroupPositions[i].positions.Count; j++)
          platformGroupPositions[i].worldSpaceCoordinates[j] = this.transform.TransformPoint(platformGroupPositions[i].positions[j]);
      }
    }

    if (platformGroupPositions.Count < 2)
      throw new ArgumentOutOfRangeException("There must be at least two platform position groups.");

    if (platformGroupStartIndex < 0 || platformGroupStartIndex >= platformGroupPositions.Count)
      SwitchGroups(0);
    else
      SwitchGroups(platformGroupStartIndex);

    _playerController.OnJumpedThisFrame += _playerController_OnJumpedThisFrame;
  }

  void OnDisable()
  {
    _playerController.OnJumpedThisFrame -= _playerController_OnJumpedThisFrame;
  }

  private void SwitchGroups(int enabledIndex)
  {
    _currentEnabledGroupIndex = enabledIndex;
    for (int i = 0; i < platformGroupPositions.Count; i++)
    {
      for (int j = 0; j < platformGroupPositions[i].worldSpaceCoordinates.Length; j++)
      {
        if (platformGroupPositions[i].gameObjects[j] != null)
        {
          _objectPoolingManager.Deactivate(platformGroupPositions[i].gameObjects[j]);
          platformGroupPositions[i].gameObjects[j] = null;
        }

        platformGroupPositions[i].gameObjects[j] = _objectPoolingManager.GetObject(
          _currentEnabledGroupIndex == i
            ? platformGroupPositions[i].enabledGameObject.name
            : platformGroupPositions[i].disabledGameObject.name
          , platformGroupPositions[i].worldSpaceCoordinates[j]);
      }
    }
  }

  void _playerController_OnJumpedThisFrame()
  {
    SwitchGroups(_currentEnabledGroupIndex + 1 >= platformGroupPositions.Count ? 0 : _currentEnabledGroupIndex + 1);
  }
}