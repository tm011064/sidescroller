using UnityEngine;
using UnityEditor;

public class CreateSmoothCameraModifierTransitionTool : EditorWindow
{
  public enum CameraModifierTransitionDirection
  {
    FromGreenToRed,
    FromRedToGreen
  }

  private Vector3 prevPosition;

  private Vector2 distanceBetweenModifiers = new Vector2(256, 0);
  private float horizontalSmoothDampTime = .6f;
  private float verticalSmoothDampTime = .6f;
  private CameraModifierTransitionDirection cameraModifierTransitionDirection = CameraModifierTransitionDirection.FromGreenToRed;

  [MenuItem("Tools/Create Smooth CameraModifier Transition")]
  static void Init()
  {
    var window = (CreateSmoothCameraModifierTransitionTool)EditorWindow.GetWindow(typeof(CreateSmoothCameraModifierTransitionTool), true);
    window.maxSize = new Vector2(512, 140);
  }
  
  public void OnGUI()
  {
    horizontalSmoothDampTime = EditorGUILayout.FloatField("horizontalSmoothDampTime", horizontalSmoothDampTime);
    verticalSmoothDampTime = EditorGUILayout.FloatField("verticalSmoothDampTime", verticalSmoothDampTime);
    distanceBetweenModifiers = EditorGUILayout.Vector2Field("distanceBetweenModifiers", distanceBetweenModifiers);
    cameraModifierTransitionDirection = (CameraModifierTransitionDirection)EditorGUILayout.EnumPopup(cameraModifierTransitionDirection);

    Debug.Log(Selection.activeGameObject);
    if (Selection.activeGameObject != null)
    {
      MultiWayCameraModifier originalMultiWayCameraModifier
        , transitionMultiWayCameraModifier = null
        , finalMultiWayCameraModifier = null;

      originalMultiWayCameraModifier = Selection.activeGameObject.GetComponent<MultiWayCameraModifier>();
      if (originalMultiWayCameraModifier != null)
      {
        var pressed = GUILayout.Button("Create", new GUIStyle(GUI.skin.GetStyle("Button")) { alignment = TextAnchor.MiddleLeft });
        if (pressed)
        {
          // 180 -> green left
          // 0 -> green right
          SceneView.lastActiveSceneView.Focus();
          EditorWindow.focusedWindow.SendEvent(EditorGUIUtility.CommandEvent("Duplicate"));

          if (Selection.activeGameObject != null)
          {
            Selection.activeGameObject.transform.Translate(distanceBetweenModifiers, Space.World);
            transitionMultiWayCameraModifier = Selection.activeGameObject.GetComponent<MultiWayCameraModifier>();
            
            SceneView.lastActiveSceneView.Focus();
            EditorWindow.focusedWindow.SendEvent(EditorGUIUtility.CommandEvent("Duplicate"));
            if (Selection.activeGameObject != null)
            {
              Selection.activeGameObject.transform.Translate(distanceBetweenModifiers, Space.World);
              finalMultiWayCameraModifier = Selection.activeGameObject.GetComponent<MultiWayCameraModifier>();
            }
          }
        }
      }

      if (originalMultiWayCameraModifier != null && transitionMultiWayCameraModifier != null && finalMultiWayCameraModifier != null)
      {
        switch (cameraModifierTransitionDirection)
        {
          case CameraModifierTransitionDirection.FromGreenToRed:
            Debug.Log(finalMultiWayCameraModifier.redCameraModificationSettings.smoothDampMoveSettings.horizontalSmoothDampTime);
            originalMultiWayCameraModifier.redCameraModificationSettings = originalMultiWayCameraModifier.greenCameraModificationSettings.Clone();
            originalMultiWayCameraModifier.redCameraModificationSettings.smoothDampMoveSettings.horizontalSmoothDampTime = horizontalSmoothDampTime;
            originalMultiWayCameraModifier.redCameraModificationSettings.smoothDampMoveSettings.verticalSmoothDampTime = verticalSmoothDampTime;

            transitionMultiWayCameraModifier.greenCameraModificationSettings.smoothDampMoveSettings.horizontalSmoothDampTime = horizontalSmoothDampTime;
            transitionMultiWayCameraModifier.greenCameraModificationSettings.smoothDampMoveSettings.verticalSmoothDampTime = verticalSmoothDampTime;
            transitionMultiWayCameraModifier.redCameraModificationSettings.smoothDampMoveSettings.horizontalSmoothDampTime = horizontalSmoothDampTime;
            transitionMultiWayCameraModifier.redCameraModificationSettings.smoothDampMoveSettings.verticalSmoothDampTime = verticalSmoothDampTime;

            Debug.Log(finalMultiWayCameraModifier.redCameraModificationSettings.smoothDampMoveSettings.horizontalSmoothDampTime);
            finalMultiWayCameraModifier.greenCameraModificationSettings = finalMultiWayCameraModifier.redCameraModificationSettings.Clone();
            finalMultiWayCameraModifier.greenCameraModificationSettings.smoothDampMoveSettings.horizontalSmoothDampTime = horizontalSmoothDampTime;
            finalMultiWayCameraModifier.greenCameraModificationSettings.smoothDampMoveSettings.verticalSmoothDampTime = verticalSmoothDampTime;
            break;

          case CameraModifierTransitionDirection.FromRedToGreen:
            originalMultiWayCameraModifier.greenCameraModificationSettings = originalMultiWayCameraModifier.redCameraModificationSettings.Clone();
            originalMultiWayCameraModifier.greenCameraModificationSettings.smoothDampMoveSettings.horizontalSmoothDampTime = horizontalSmoothDampTime;
            originalMultiWayCameraModifier.greenCameraModificationSettings.smoothDampMoveSettings.verticalSmoothDampTime = verticalSmoothDampTime;

            transitionMultiWayCameraModifier.greenCameraModificationSettings.smoothDampMoveSettings.horizontalSmoothDampTime = horizontalSmoothDampTime;
            transitionMultiWayCameraModifier.greenCameraModificationSettings.smoothDampMoveSettings.verticalSmoothDampTime = verticalSmoothDampTime;
            transitionMultiWayCameraModifier.redCameraModificationSettings.smoothDampMoveSettings.horizontalSmoothDampTime = horizontalSmoothDampTime;
            transitionMultiWayCameraModifier.redCameraModificationSettings.smoothDampMoveSettings.verticalSmoothDampTime = verticalSmoothDampTime;

            finalMultiWayCameraModifier.redCameraModificationSettings = finalMultiWayCameraModifier.greenCameraModificationSettings.Clone();
            finalMultiWayCameraModifier.redCameraModificationSettings.smoothDampMoveSettings.horizontalSmoothDampTime = horizontalSmoothDampTime;
            finalMultiWayCameraModifier.redCameraModificationSettings.smoothDampMoveSettings.verticalSmoothDampTime = verticalSmoothDampTime;
            break;
        }


      }
    }
  }

}