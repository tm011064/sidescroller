using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlatformSlopeBuilderScript : MonoBehaviour
{
  public GameObject platform;
  public Vector2 platformDimensions;

  public Vector3 spawnPoint;
  public float[] angles;
  
  private Vector2 GetMidPoint(float x, float y, float width, float height, float angle_degrees)
  {
    var angle_rad = angle_degrees * Mathf.PI / 180f;
    var cosa = Mathf.Cos(angle_rad);
    var sina = Mathf.Sin(angle_rad);
    var wp = width / 2f;
    var hp = height / 2f;

    return new Vector2(
      x + wp * cosa + hp * sina
      , y + wp * sina - hp * cosa);
  }


  public void BuildObject()
  {
    Logger.Info("Building slope platform");

    Queue<float> rotations = new Queue<float>(angles);

    Vector2 currentRightTop = new Vector2(spawnPoint.x, spawnPoint.y);
    while (rotations.Count > 0)
    {
      float angle = rotations.Dequeue();

      Vector2 midPoint = GetMidPoint(currentRightTop.x, currentRightTop.y, platformDimensions.x, platformDimensions.y, angle);
      Vector2 normalRightTop = new Vector2(currentRightTop.x + platformDimensions.x, currentRightTop.y);

      var angle_rad = angle * Mathf.PI / 180f;
      Vector2 nextRightTop = new Vector2(
        Mathf.Cos(angle_rad) * (normalRightTop.x - currentRightTop.x) - Mathf.Sin(angle_rad) * (normalRightTop.y - currentRightTop.y) + currentRightTop.x
        , Mathf.Sin(angle_rad) * (normalRightTop.x - currentRightTop.x) - Mathf.Cos(angle_rad) * (normalRightTop.y - currentRightTop.y) + currentRightTop.y
      );

      currentRightTop = nextRightTop;

      //Trace.WriteLine("Rotation: " + angle + ", Pos: (" + midPoint.x + ", " + midPoint.y + ")");

      var obj = Instantiate(platform, new Vector3(midPoint.x, midPoint.y, 0f), Quaternion.Euler(0, 0, angle)) as GameObject;
      GroundPlatformSpriteRenderer groundPlatformSpriteRenderer = obj.GetComponent<GroundPlatformSpriteRenderer>();
      if (groundPlatformSpriteRenderer != null)
      {
        groundPlatformSpriteRenderer.width = Mathf.RoundToInt(platformDimensions.x);
        groundPlatformSpriteRenderer.height = Mathf.RoundToInt(platformDimensions.y);
      }
    }
  }
}
