using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// SceneSwitchWindow class.
/// </summary>
public class SceneSwitchWindow : EditorWindow
{
  /// <summary>
  /// Tracks scroll position.
  /// </summary>
  private Vector2 scrollPos;

  /// <summary>
  /// Initialize window state.
  /// </summary>
  [MenuItem("Tools/Switch Scene")]
  internal static void Init()
  {
    // EditorWindow.GetWindow() will return the open instance of the specified window or create a new
    // instance if it can't find one. The second parameter is a flag for creating the window as a
    // Utility window; Utility windows cannot be docked like the Scene and Game view windows.
    var window = (SceneSwitchWindow)GetWindow(typeof(SceneSwitchWindow), false, "Scene Switch");
    window.position = new Rect(window.position.xMin + 100f, window.position.yMin + 100f, 768f, 400f);
  }

  /// <summary>
  /// Called on GUI events.
  /// </summary>
  internal void OnGUI()
  {
    EditorGUILayout.BeginVertical();
    this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos, false, false);

    GUILayout.Label("Scenes In Build", EditorStyles.boldLabel);
    Debug.Log(Application.dataPath);

    List<FileInfo> fileInfos = new List<FileInfo>((from c in Directory.GetFiles(Application.dataPath, "*.unity", SearchOption.AllDirectories)
                                                   select new FileInfo(c)).OrderByDescending(c => c.LastWriteTimeUtc));

    foreach (FileInfo fileInfo in fileInfos)
    {
      string name = fileInfo.FullName.Replace(Application.dataPath, string.Empty).TrimStart('\\').TrimStart('/');
      name = name.Substring(0, name.LastIndexOf('.'));

      var pressed = GUILayout.Button(name, new GUIStyle(GUI.skin.GetStyle("Button")) { alignment = TextAnchor.MiddleLeft });
      if (pressed)
      {
        if (EditorApplication.SaveCurrentSceneIfUserWantsTo())
        {
          EditorApplication.OpenScene(fileInfo.FullName);
        }
      }
    }

    EditorGUILayout.EndScrollView();
    EditorGUILayout.EndVertical();
  }
}