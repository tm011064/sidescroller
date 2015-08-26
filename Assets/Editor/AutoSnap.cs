using UnityEngine;
using UnityEditor;

public class AutoSnap : EditorWindow
{
  private Vector3 prevPosition;
  private bool doSnap = true;
  private float snapValue = 16;

  [MenuItem("Tools/Auto Snap %_l")]
  static void Init()
  {
    var window = (AutoSnap)EditorWindow.GetWindow(typeof(AutoSnap));
    window.maxSize = new Vector2(200, 100);
  }

  void Awake()
  {
    // Remove delegate listener if it has previously
    // been assigned.
    SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
    // Add (or re-add) the delegate.
    SceneView.onSceneGUIDelegate += this.OnSceneGUI;
  }

  public void OnGUI()
  {

    doSnap = EditorGUILayout.Toggle("Auto Snap", doSnap);
    snapValue = EditorGUILayout.FloatField("Snap Value", snapValue);
  }
    
  // Window has been selected
  void OnFocus()
  {
    // Remove delegate listener if it has previously
    // been assigned.
    SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
    // Add (or re-add) the delegate.
    SceneView.onSceneGUIDelegate += this.OnSceneGUI;
  }

  void OnDestroy()
  {
    // When the window is destroyed, remove the delegate
    // so that it will no longer do any drawing.
    SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
  }

  void OnSceneGUI(SceneView sceneView)
  {
    if (doSnap
      && !EditorApplication.isPlaying
      && Selection.transforms.Length > 0
      && Selection.transforms[0].position != prevPosition)
    {
      Snap();
      prevPosition = Selection.transforms[0].position;
    }
  }

  private void Snap()
  {
    foreach (var transform in Selection.transforms)
    {
      var t = transform.transform.position;
      t.x = Round(t.x);
      t.y = Round(t.y);
      t.z = Round(t.z);
      transform.transform.position = t;
    }
  }

  private float Round(float input)
  {
    return snapValue * Mathf.Round((input / snapValue));
  }
}