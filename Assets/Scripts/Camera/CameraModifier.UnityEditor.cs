#if UNITY_EDITOR

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public partial class CameraModifier : MonoBehaviour
{
  public ImportCameraSettings importCameraSettings;

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
        case ImportCameraSettingsMode.FromGreenToRed:

          this.verticalLockSettings = multiWayCameraModifier.greenCameraModificationSettings.verticalLockSettings.Clone();
          this.horizontalLockSettings = multiWayCameraModifier.greenCameraModificationSettings.horizontalLockSettings.Clone();
          this.zoomSettings = multiWayCameraModifier.greenCameraModificationSettings.zoomSettings.Clone();
          this.smoothDampMoveSettings = multiWayCameraModifier.greenCameraModificationSettings.smoothDampMoveSettings.Clone();

          this.offset = multiWayCameraModifier.greenCameraModificationSettings.offset;
          this.verticalCameraFollowMode = multiWayCameraModifier.greenCameraModificationSettings.verticalCameraFollowMode;
          this.horizontalOffsetDeltaMovementFactor = multiWayCameraModifier.greenCameraModificationSettings.horizontalOffsetDeltaMovementFactor;

          Debug.Log("Successfully imported green settings from " + importCameraSettings.importSource.name + " and applied them to greenCameraModificationSettings");
          break;

        case ImportCameraSettingsMode.FromRedToGreen:
        case ImportCameraSettingsMode.FromRedToRed:
          this.verticalLockSettings = multiWayCameraModifier.redCameraModificationSettings.verticalLockSettings.Clone();
          this.horizontalLockSettings = multiWayCameraModifier.redCameraModificationSettings.horizontalLockSettings.Clone();
          this.zoomSettings = multiWayCameraModifier.redCameraModificationSettings.zoomSettings.Clone();
          this.smoothDampMoveSettings = multiWayCameraModifier.redCameraModificationSettings.smoothDampMoveSettings.Clone();

          this.offset = multiWayCameraModifier.redCameraModificationSettings.offset;
          this.verticalCameraFollowMode = multiWayCameraModifier.redCameraModificationSettings.verticalCameraFollowMode;
          this.horizontalOffsetDeltaMovementFactor = multiWayCameraModifier.redCameraModificationSettings.horizontalOffsetDeltaMovementFactor;

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
      this.verticalLockSettings = cameraModifier.verticalLockSettings.Clone();
      this.horizontalLockSettings = cameraModifier.horizontalLockSettings.Clone();
      this.zoomSettings = cameraModifier.zoomSettings.Clone();
      this.smoothDampMoveSettings = cameraModifier.smoothDampMoveSettings.Clone();

      this.offset = cameraModifier.offset;
      this.verticalCameraFollowMode = cameraModifier.verticalCameraFollowMode;
      this.horizontalOffsetDeltaMovementFactor = cameraModifier.horizontalOffsetDeltaMovementFactor;
    }
    else
    {
      Debug.LogError("Unable to import settings because object does not contain MultiWayCameraModifier nor CameraModifier component.");
      return;
    }
  }
}
#endif