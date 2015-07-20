using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class JumpControlledDisappearingPlatformGroup : MonoBehaviour
{
  #region nested
  public enum JumpControlledDisappearingPlatformMode
  {
    DisappearWhenLandingOnNextPlatform,
    DisappearWhenLostGround
  }
  #endregion

  public GameObject platformPrefab;
  public List<Vector3> platformPositions = new List<Vector3>();
  public int totalVisiblePlatforms = 2;
  public int totalInitialVisiblePlatforms = 1;
  public JumpControlledDisappearingPlatformMode jumpControlledDisappearingPlatformMode = JumpControlledDisappearingPlatformMode.DisappearWhenLostGround;

  private ObjectPoolingManager _objectPoolingManager;

  private Queue<GameObject> _currentPlatforms = new Queue<GameObject>();
  private int _currentIndex = 0;
  private GameObject _currentPlatform;

  protected PlayerController _playerController;
  private bool _hasLandedOnPlatform = false;
  private bool _isOnPlatform = false;
  private List<Vector3> _worldSpacePlatformCoordinates = new List<Vector3>();

  void OnEnable()
  {
    switch (jumpControlledDisappearingPlatformMode)
    {
      case JumpControlledDisappearingPlatformMode.DisappearWhenLostGround:
        _playerController.OnGroundedPlatformChanged += _playerController_DisappearWhenLostGround_OnGroundedPlatformChanged;
        break;
      case JumpControlledDisappearingPlatformMode.DisappearWhenLandingOnNextPlatform:
        _playerController.OnGroundedPlatformChanged += _playerController_DisappearWhenLandingOnNextPlatform_OnGroundedPlatformChanged;
        break;
    }
  }

  void OnDisable()
  {
    switch (jumpControlledDisappearingPlatformMode)
    {
      case JumpControlledDisappearingPlatformMode.DisappearWhenLostGround:
        _playerController.OnGroundedPlatformChanged -= _playerController_DisappearWhenLostGround_OnGroundedPlatformChanged;
        break;
      case JumpControlledDisappearingPlatformMode.DisappearWhenLandingOnNextPlatform:
        _playerController.OnGroundedPlatformChanged -= _playerController_DisappearWhenLandingOnNextPlatform_OnGroundedPlatformChanged;
        break;
    }
  }

  void _playerController_DisappearWhenLostGround_OnGroundedPlatformChanged(object sender, PlayerController.GroundedPlatformChangedEventArgs e)
  {
    if (e.currentPlatform == null)
    {// lost ground
      if (_hasLandedOnPlatform)
      {
        if (_isOnPlatform)
        {
          switch (jumpControlledDisappearingPlatformMode)
          {
            case JumpControlledDisappearingPlatformMode.DisappearWhenLostGround:
              if (_currentPlatforms.Count >= totalVisiblePlatforms)
              {
                GameObject platformToRemove = _currentPlatforms.Dequeue();
                StartCoroutine(FadeOutPlatform(platformToRemove, .2f));
              }
              break;
          }
        }
      }

      _isOnPlatform = false;
      _currentPlatform = null;
    }
    else
    {
      if (e.currentPlatform != _currentPlatform)
      {
        if (_currentPlatforms.Contains(e.currentPlatform))
        {
          _currentPlatform = e.currentPlatform;

          _hasLandedOnPlatform = true;
          _isOnPlatform = true;

          if (e.currentPlatform.transform.position == _worldSpacePlatformCoordinates[_currentIndex])
          {
            // we are on last platform. Make sure we have the correct count
            while (_currentPlatforms.Count >= totalVisiblePlatforms)
            {
              GameObject platformToRemove = _currentPlatforms.Dequeue();
              StartCoroutine(FadeOutPlatform(platformToRemove, .2f));
            }

            while (_currentPlatforms.Count < totalVisiblePlatforms)
            {
              _currentIndex++;
              if (_currentIndex >= _worldSpacePlatformCoordinates.Count)
                _currentIndex = 0;

              GameObject platform = _objectPoolingManager.GetObject(platformPrefab.name);
              platform.transform.position = _worldSpacePlatformCoordinates[_currentIndex];
              platform.SetActive(true);

              _currentPlatforms.Enqueue(platform);
            }
          }
        }
        else
        {
          _currentPlatform = null;
        }
      }
    }
  }

  void _playerController_DisappearWhenLandingOnNextPlatform_OnGroundedPlatformChanged(object sender, PlayerController.GroundedPlatformChangedEventArgs e)
  {
    if (_currentPlatforms.Contains(e.currentPlatform))
    {
      if (e.currentPlatform.transform.position == _worldSpacePlatformCoordinates[_currentIndex])
      {
        // we are on last platform
        while (_currentPlatforms.Count < totalVisiblePlatforms + 1)
        {
          _currentIndex++;
          if (_currentIndex >= _worldSpacePlatformCoordinates.Count)
            _currentIndex = 0;

          GameObject platform = _objectPoolingManager.GetObject(platformPrefab.name);
          platform.transform.position = _worldSpacePlatformCoordinates[_currentIndex];
          platform.SetActive(true);

          _currentPlatforms.Enqueue(platform);
        }

        GameObject platformToRemove = _currentPlatforms.Dequeue();
        ObjectPoolingManager.Instance.Deactivate(platformToRemove);
        //platformToRemove.SetActive(false); // TODO (Roman): notify and run fade animation
      }
    }
  }

  IEnumerator FadeOutPlatform(GameObject platform, float delayTime)
  {
    yield return new WaitForSeconds(delayTime);

    ObjectPoolingManager.Instance.Deactivate(platform);
    //platform.SetActive(false); // TODO (Roman): notify and run fade animation
  }

  void Awake()
  {
    _playerController = GameManager.instance.player;
  }

  void Start()
  {
    _objectPoolingManager = ObjectPoolingManager.Instance;
    // TODO (Roman): this should be done at camera script
    _objectPoolingManager.RegisterPool(platformPrefab, totalVisiblePlatforms, int.MaxValue);

    if (totalVisiblePlatforms >= platformPositions.Count)
      throw new ArgumentOutOfRangeException("Total visible platforms must be less or equal the number of platform positions.");
    if (totalInitialVisiblePlatforms >= platformPositions.Count)
      throw new ArgumentOutOfRangeException("Total initial visible platforms must be less or equal the number of platform positions.");

    for (int i = 0; i < platformPositions.Count; i++)
      _worldSpacePlatformCoordinates.Add(this.transform.TransformPoint(platformPositions[i]));

    for (int i = 0; i < totalInitialVisiblePlatforms; i++)
    {
      GameObject platform = _objectPoolingManager.GetObject(platformPrefab.name);
      platform.transform.position = _worldSpacePlatformCoordinates[i];
      platform.SetActive(true);

      _currentPlatforms.Enqueue(platform);
      _currentIndex = i;
    }
  }
}