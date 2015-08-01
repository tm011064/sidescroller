#if UNITY_EDITOR

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[Serializable]
public class ImportCameraSettings
{
  [Tooltip("Must contain MultiWayCameraModifier or CameraModifier component")]
  public GameObject importSource;
  public ImportCameraSettingsMode importCameraSettingsMode;
}

public enum ImportCameraSettingsMode
{
  FromGreenToGreen,
  FromGreenToRed,
  FromRedToGreen,
  FromRedToRed,
  FromModifierToGreen,
  FromModifierToRed
}

public partial class MultiWayCameraModifier : MonoBehaviour
{
  public ImportCameraSettings importCameraSettings;

  private struct BoxVertices
  {
    public Vector2 leftTop;
    public Vector2 rightTop;
    public Vector2 leftBottom;
    public Vector2 rightBottom;
  }

  private bool _areGizmosInitialized = false;
  private BoxVertices _greenBox;
  private BoxVertices _redBox;

  private void SetColoredBoxVertices()
  {
    EdgeCollider2D edgeCollider = this.GetComponent<EdgeCollider2D>();

    Vector2 normal = new Vector2(-edgeCollider.points[1].y, edgeCollider.points[1].x).normalized;

    float boxWidth = 128f;

    _greenBox.leftTop = edgeCollider.points[1];
    _greenBox.rightTop = edgeCollider.points[1] - normal * boxWidth;
    _greenBox.rightBottom = edgeCollider.points[0] - normal * boxWidth;
    _greenBox.leftBottom = edgeCollider.points[0];

    _redBox.rightTop = edgeCollider.points[1];
    _redBox.leftTop = edgeCollider.points[1] + normal * boxWidth;
    _redBox.leftBottom = edgeCollider.points[0] + normal * boxWidth;
    _redBox.rightBottom = edgeCollider.points[0];
  }

  void OnDrawGizmosSelected()
  {
    if (greenCameraModificationSettings.horizontalLockSettings.enabled
      && greenCameraModificationSettings.horizontalLockSettings.enableRightHorizontalLock)
    {
      Gizmos.color = Color.green;

      Vector3 v1 = new Vector3(parentPositionObject.transform.TransformPoint(greenCameraModificationSettings.horizontalLockSettings.rightHorizontalLockPosition, 0f, 0f).x, this.transform.position.y - 256f);
      Vector3 v2 = new Vector3(parentPositionObject.transform.TransformPoint(greenCameraModificationSettings.horizontalLockSettings.rightHorizontalLockPosition, 0f, 0f).x, this.transform.position.y + 0f);

      Gizmos.DrawLine(v1, v2);
    }
    if (greenCameraModificationSettings.horizontalLockSettings.enabled
      && greenCameraModificationSettings.horizontalLockSettings.enableLeftHorizontalLock)
    {
      Gizmos.color = Color.green;

      Vector3 v1 = new Vector3(parentPositionObject.transform.TransformPoint(greenCameraModificationSettings.horizontalLockSettings.leftHorizontalLockPosition, 0f, 0f).x, this.transform.position.y - 256f);
      Vector3 v2 = new Vector3(parentPositionObject.transform.TransformPoint(greenCameraModificationSettings.horizontalLockSettings.leftHorizontalLockPosition, 0f, 0f).x, this.transform.position.y + 0f);

      Gizmos.DrawLine(v1, v2);
    }
    if (greenCameraModificationSettings.verticalLockSettings.enabled
      && greenCameraModificationSettings.verticalLockSettings.enableTopVerticalLock)
    {
      Gizmos.color = Color.green;

      Vector3 v1 = new Vector3(this.transform.position.x - 256f, parentPositionObject.transform.TransformPoint(0f, greenCameraModificationSettings.verticalLockSettings.topVerticalLockPosition, 0f).y);
      Vector3 v2 = new Vector3(this.transform.position.x + 0f, parentPositionObject.transform.TransformPoint(0f, greenCameraModificationSettings.verticalLockSettings.topVerticalLockPosition, 0f).y);

      Gizmos.DrawLine(v1, v2);
    }
    if (greenCameraModificationSettings.verticalLockSettings.enabled
      && greenCameraModificationSettings.verticalLockSettings.enableBottomVerticalLock)
    {
      Gizmos.color = Color.green;

      Vector3 v1 = new Vector3(this.transform.position.x - 256f, parentPositionObject.transform.TransformPoint(0f, greenCameraModificationSettings.verticalLockSettings.bottomVerticalLockPosition, 0f).y);
      Vector3 v2 = new Vector3(this.transform.position.x + 0, parentPositionObject.transform.TransformPoint(0f, greenCameraModificationSettings.verticalLockSettings.bottomVerticalLockPosition, 0f).y);

      Gizmos.DrawLine(v1, v2);
    }
    if (greenCameraModificationSettings.verticalLockSettings.enabled
      && greenCameraModificationSettings.verticalLockSettings.enableDefaultVerticalLockPosition)
    {
      Gizmos.color = Color.green;

      Vector3 v1 = new Vector3(this.transform.position.x - 512f, parentPositionObject.transform.TransformPoint(0f, greenCameraModificationSettings.verticalLockSettings.defaultVerticalLockPosition, 0f).y);
      Vector3 v2 = new Vector3(this.transform.position.x + 0f, parentPositionObject.transform.TransformPoint(0f, greenCameraModificationSettings.verticalLockSettings.defaultVerticalLockPosition, 0f).y);

      Gizmos.DrawLine(v1, v2);
    }
    

    if (redCameraModificationSettings.horizontalLockSettings.enabled
      && redCameraModificationSettings.horizontalLockSettings.enableRightHorizontalLock)
    {
      Gizmos.color = Color.red;

      Vector3 v1 = new Vector3(parentPositionObject.transform.TransformPoint(redCameraModificationSettings.horizontalLockSettings.rightHorizontalLockPosition, 0f, 0f).x, this.transform.position.y - 0f);
      Vector3 v2 = new Vector3(parentPositionObject.transform.TransformPoint(redCameraModificationSettings.horizontalLockSettings.rightHorizontalLockPosition, 0f, 0f).x, this.transform.position.y + 256f);

      Gizmos.DrawLine(v1, v2);
    }
    if (redCameraModificationSettings.horizontalLockSettings.enabled
      && redCameraModificationSettings.horizontalLockSettings.enableLeftHorizontalLock)
    {
      Gizmos.color = Color.red;

      Vector3 v1 = new Vector3(parentPositionObject.transform.TransformPoint(redCameraModificationSettings.horizontalLockSettings.leftHorizontalLockPosition, 0f, 0f).x, this.transform.position.y - 0f);
      Vector3 v2 = new Vector3(parentPositionObject.transform.TransformPoint(redCameraModificationSettings.horizontalLockSettings.leftHorizontalLockPosition, 0f, 0f).x, this.transform.position.y + 256f);

      Gizmos.DrawLine(v1, v2);
    }
    if (redCameraModificationSettings.verticalLockSettings.enabled
      && redCameraModificationSettings.verticalLockSettings.enableTopVerticalLock)
    {
      Gizmos.color = Color.red;

      Vector3 v1 = new Vector3(this.transform.position.x - 0f, parentPositionObject.transform.TransformPoint(0f, redCameraModificationSettings.verticalLockSettings.topVerticalLockPosition, 0f).y);
      Vector3 v2 = new Vector3(this.transform.position.x + 256f, parentPositionObject.transform.TransformPoint(0f, redCameraModificationSettings.verticalLockSettings.topVerticalLockPosition, 0f).y);

      Gizmos.DrawLine(v1, v2);
    }
    if (redCameraModificationSettings.verticalLockSettings.enabled
      && redCameraModificationSettings.verticalLockSettings.enableBottomVerticalLock)
    {
      Gizmos.color = Color.red;

      Vector3 v1 = new Vector3(this.transform.position.x - 0f, parentPositionObject.transform.TransformPoint(0f, redCameraModificationSettings.verticalLockSettings.bottomVerticalLockPosition, 0f).y);
      Vector3 v2 = new Vector3(this.transform.position.x + 256f, parentPositionObject.transform.TransformPoint(0f, redCameraModificationSettings.verticalLockSettings.bottomVerticalLockPosition, 0f).y);

      Gizmos.DrawLine(v1, v2);
    }
    if (redCameraModificationSettings.verticalLockSettings.enabled
      && redCameraModificationSettings.verticalLockSettings.enableDefaultVerticalLockPosition)
    {
      Gizmos.color = Color.red;

      Vector3 v1 = new Vector3(this.transform.position.x - 0f, parentPositionObject.transform.TransformPoint(0f, redCameraModificationSettings.verticalLockSettings.defaultVerticalLockPosition, 0f).y);
      Vector3 v2 = new Vector3(this.transform.position.x + 512f, parentPositionObject.transform.TransformPoint(0f, redCameraModificationSettings.verticalLockSettings.defaultVerticalLockPosition, 0f).y);

      Gizmos.DrawLine(v1, v2);
    }

  }

  void OnDrawGizmos()
  {
    if (!_areGizmosInitialized)
    {
      SetColoredBoxVertices();
      _areGizmosInitialized = true;
    }

    Gizmos.color = Color.green;
    Gizmos.DrawLine(this.transform.TransformPoint(_greenBox.leftTop), this.transform.TransformPoint(_greenBox.rightTop));
    Gizmos.DrawLine(this.transform.TransformPoint(_greenBox.rightTop), this.transform.TransformPoint(_greenBox.rightBottom));
    Gizmos.DrawLine(this.transform.TransformPoint(_greenBox.rightBottom), this.transform.TransformPoint(_greenBox.leftBottom));

    Gizmos.color = Color.red;
    Gizmos.DrawLine(this.transform.TransformPoint(_redBox.rightTop), this.transform.TransformPoint(_redBox.leftTop));
    Gizmos.DrawLine(this.transform.TransformPoint(_redBox.leftTop), this.transform.TransformPoint(_redBox.leftBottom));
    Gizmos.DrawLine(this.transform.TransformPoint(_redBox.leftBottom), this.transform.TransformPoint(_redBox.rightBottom));

    Gizmos.color = Color.white;
    Gizmos.DrawLine(this.transform.TransformPoint(_greenBox.leftBottom), this.transform.TransformPoint(_greenBox.leftTop));
  }

  public void ImportSettings()
  {
    if (importCameraSettings.importSource == null)
    {
      Debug.LogWarning("Unable to import settings because no object was selected");
      return;
    }

    MultiWayCameraModifier multiWayCameraModifier = importCameraSettings.importSource.GetComponent<MultiWayCameraModifier>();
    CameraModifier cameraModifier = importCameraSettings.importSource.GetComponent<CameraModifier>();
    if (multiWayCameraModifier != null)
    {
      switch (importCameraSettings.importCameraSettingsMode)
      {
        case ImportCameraSettingsMode.FromGreenToGreen:
          this.greenCameraModificationSettings = multiWayCameraModifier.greenCameraModificationSettings.Clone();
          Debug.Log("Successfully imported green settings from " + importCameraSettings.importSource.name + " and applied them to greenCameraModificationSettings");
          break;
        case ImportCameraSettingsMode.FromGreenToRed:
          this.redCameraModificationSettings = multiWayCameraModifier.greenCameraModificationSettings.Clone();
          Debug.Log("Successfully imported green settings from " + importCameraSettings.importSource.name + " and applied them to redCameraModificationSettings");
          break;
        case ImportCameraSettingsMode.FromRedToGreen:
          this.greenCameraModificationSettings = multiWayCameraModifier.redCameraModificationSettings.Clone();
          Debug.Log("Successfully imported red settings from " + importCameraSettings.importSource.name + " and applied them to greenCameraModificationSettings");
          break;
        case ImportCameraSettingsMode.FromRedToRed:
          this.redCameraModificationSettings = multiWayCameraModifier.redCameraModificationSettings.Clone();
          Debug.Log("Successfully imported red settings from " + importCameraSettings.importSource.name + " and applied them to redCameraModificationSettings");
          break;

        case ImportCameraSettingsMode.FromModifierToGreen:
        case ImportCameraSettingsMode.FromModifierToRed:
          Debug.LogError("Unable to import modifer settings because the source is of type MultiWayCameraModifier");
          break;
      }
    }
    else if (cameraModifier != null)
    {
      MultiWayCameraModificationSetting multiWayCameraModificationSetting = new MultiWayCameraModificationSetting();

      multiWayCameraModificationSetting.verticalLockSettings = cameraModifier.verticalLockSettings.Clone();
      multiWayCameraModificationSetting.horizontalLockSettings = cameraModifier.horizontalLockSettings.Clone();
      multiWayCameraModificationSetting.zoomSettings = cameraModifier.zoomSettings.Clone();
      multiWayCameraModificationSetting.smoothDampMoveSettings = cameraModifier.smoothDampMoveSettings.Clone();

      multiWayCameraModificationSetting.offset = cameraModifier.offset;
      multiWayCameraModificationSetting.verticalCameraFollowMode = cameraModifier.verticalCameraFollowMode;

      switch (importCameraSettings.importCameraSettingsMode)
      {
        case ImportCameraSettingsMode.FromGreenToGreen:
        case ImportCameraSettingsMode.FromRedToGreen:
        case ImportCameraSettingsMode.FromModifierToGreen:
          this.greenCameraModificationSettings = multiWayCameraModificationSetting;
          Debug.Log("Successfully imported settings from " + importCameraSettings.importSource.name + " and applied them to greenCameraModificationSettings");
          break;

        case ImportCameraSettingsMode.FromGreenToRed:
        case ImportCameraSettingsMode.FromRedToRed:
        case ImportCameraSettingsMode.FromModifierToRed:
          this.redCameraModificationSettings = multiWayCameraModificationSetting;
          Debug.Log("Successfully imported settings from " + importCameraSettings.importSource.name + " and applied them to redCameraModificationSettings");
          break;
      }
    }
    else
    {
      Debug.LogError("Unable to import settings because object does not contain MultiWayCameraModifier nor CameraModifier component.");
      return;
    }
  }

  public void BuildObject()
  {
    EdgeCollider2D edgeCollider = this.GetComponent<EdgeCollider2D>();

    edgeCollider.hideFlags = HideFlags.NotEditable;

    List<Vector2> points = new List<Vector2>();
    points.Add(Vector2.zero);

    float x = Mathf.Sin(edgeColliderAngle * Mathf.Deg2Rad) * edgeColliderLength;
    float y = Mathf.Cos(edgeColliderAngle * Mathf.Deg2Rad) * edgeColliderLength;
    points.Add(new Vector2(x, y));

    edgeCollider.points = points.ToArray();

    SetColoredBoxVertices();
  }
}
#endif