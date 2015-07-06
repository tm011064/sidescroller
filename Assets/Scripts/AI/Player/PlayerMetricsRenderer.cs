using UnityEngine;
using System.Collections;

public partial class PlayerMetricsRenderer : MonoBehaviour
{
#if UNITY_EDITOR
  private int _jumpRadiusResolution = 32;
  private Vector3[] _walkingJumpRadiusPositions = null;
  private Vector3[] _runningJumpRadiusPositions = null;
  private BoxCollider2D _collider = null;

  public float walkingJumpDistance = 512f;
  public float runningJumpDistance = 1024f;
  public float jumpHeight = 360f;
  public float maxJumpHeight = 304f;
  public float comfortableJumpHeight = 272f;
  public float walkingJumpHeightWidth = 332f;

  public Vector2 playerDimensions = new Vector2(86, 86);


  void OnReset()
  {
    _walkingJumpRadiusPositions = null;
    _runningJumpRadiusPositions = null;
  }

  void Start()
  {
    _collider = GetComponent<BoxCollider2D>();

    Logger.Info("Player Dimensions: " + playerDimensions);
    if (_collider != null)
    {
      playerDimensions = new Vector2(_collider.bounds.extents.x * 2f, _collider.bounds.extents.y * 2f);
      Logger.Info("Player Dimensions: " + playerDimensions);
    }
  }

  void OnDrawGizmos()
  {
    if (_collider != null
      || (Application.isEditor && !Application.isPlaying))
    {
      _walkingJumpRadiusPositions = null;
      _runningJumpRadiusPositions = null;

      Gizmos.color = Color.gray;
      if (_walkingJumpRadiusPositions == null)
      {
        _walkingJumpRadiusPositions = GizmoUtility.CreateEllipse(
          walkingJumpDistance
          , jumpHeight
          , 0, 0//-playerDimensions.y * .5f
          , 0f
          , _jumpRadiusResolution);
      }
      if (_runningJumpRadiusPositions == null)
      {
        _runningJumpRadiusPositions = GizmoUtility.CreateEllipse(
          runningJumpDistance
          , jumpHeight
          , 0, 0//-playerDimensions.y * .5f
          , 0f
          , _jumpRadiusResolution);
      }

      for (int i = 1; i < _walkingJumpRadiusPositions.Length; i++)
        Gizmos.DrawLine(_walkingJumpRadiusPositions[i - 1] + this.transform.position, _walkingJumpRadiusPositions[i] + this.transform.position);
      Gizmos.DrawLine(_walkingJumpRadiusPositions[_walkingJumpRadiusPositions.Length - 1] + this.transform.position, _walkingJumpRadiusPositions[0] + this.transform.position);

      for (int i = 1; i < _runningJumpRadiusPositions.Length; i++)
        Gizmos.DrawLine(_runningJumpRadiusPositions[i - 1] + this.transform.position, _runningJumpRadiusPositions[i] + this.transform.position);
      Gizmos.DrawLine(_runningJumpRadiusPositions[_runningJumpRadiusPositions.Length - 1] + this.transform.position, _runningJumpRadiusPositions[0] + this.transform.position);


      float comfortableJumpHeightYPos = this.transform.position.y + comfortableJumpHeight;// -playerDimensions.y * .5f;
      float maxJumpHeightYPos = this.transform.position.y + maxJumpHeight;//  - playerDimensions.y * .5f;

      Gizmos.color = Color.gray;
      Gizmos.DrawLine(
        new Vector3(this.transform.position.x - walkingJumpHeightWidth * .4f, maxJumpHeightYPos, this.transform.position.z)
        , new Vector3(this.transform.position.x + walkingJumpHeightWidth * .4f, maxJumpHeightYPos, this.transform.position.z));
      Gizmos.color = Color.white;
      Gizmos.DrawLine(
        new Vector3(this.transform.position.x - walkingJumpHeightWidth * .7f, comfortableJumpHeightYPos, this.transform.position.z)
        , new Vector3(this.transform.position.x + walkingJumpHeightWidth * .7f, comfortableJumpHeightYPos, this.transform.position.z));
      
      Gizmos.DrawLine(new Vector3(transform.position.x - playerDimensions.x, transform.position.y - playerDimensions.y * .5f, transform.position.z)
      , new Vector3(transform.position.x + playerDimensions.x, transform.position.y - playerDimensions.y * .5f, transform.position.z));

      // draw player visible rect
      GizmoUtility.DrawBoundingBox(transform.position, new Vector3(950f, 540f, 0f), Color.blue);

      // draw default camera locked
      // draw player visible rect 
      GizmoUtility.DrawBoundingBox(new Vector3(transform.position.x, 540f, transform.position.z), new Vector3(950f, 540f, 0f), Color.cyan);
      Gizmos.color = Color.cyan;
      Gizmos.DrawLine(
        new Vector3(this.transform.position.x - 1000f, 540f, this.transform.position.z)
        , new Vector3(this.transform.position.x + 1000f, 540f, this.transform.position.z));
      Gizmos.DrawLine(
        new Vector3(this.transform.position.x - 1000f, 720f, this.transform.position.z)
        , new Vector3(this.transform.position.x + 1000f, 720f, this.transform.position.z));
      Gizmos.DrawLine(
        new Vector3(this.transform.position.x - 1000f, 360f, this.transform.position.z)
        , new Vector3(this.transform.position.x + 1000f, 360f, this.transform.position.z));

    }
  }
#endif
}
