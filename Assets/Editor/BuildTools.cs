#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class BuildTools
{
  //-------------------------------------------------------------------------------------------------------------------------
  public static void BuildWeb()
  {
    Build(@"AutoBuild\WebBuild.log", BuildTarget.WebPlayer, @"AutoBuild\Builds\Web\" + PlayerSettings.productName);
  }


  //-------------------------------------------------------------------------------------------------------------------------
  public static void BuildPC()
  {
    Build(@"AutoBuild\PCBuild.log", BuildTarget.StandaloneWindows, @"AutoBuild\Builds\Win32\" + PlayerSettings.productName + ".exe");
  }


  //-------------------------------------------------------------------------------------------------------------------------
  public static void BuildAndroid()
  {
    Build(@"AutoBuild\AndroidBuild.log", BuildTarget.Android, @"AutoBuild\Builds\Android\" + PlayerSettings.productName + ".apk");
  }


  //-------------------------------------------------------------------------------------------------------------------------
  public static void Build(string log_filename, BuildTarget target, string output)
  {
    using (LogFile log = new LogFile(log_filename, false))
    {
      log.Message("Building Platform: " + target.ToString());
      log.Message("");

      string[] level_list = FindScenes();
      log.Message("Scenes to be processed: " + level_list.Length);

      foreach (string s in level_list)
      {
        string cutdown_level_name = s.Remove(s.IndexOf(".unity"));
        log.Message("   " + cutdown_level_name);
      }

      // Make sure the paths exist before building.
      try
      {
        string directoryName = output.Substring(0, output.LastIndexOf('\\'));
        Debug.Log("Checking whether directory " + directoryName + " exists...");
        DirectoryInfo directoryInfo = new DirectoryInfo(directoryName);
        if (!directoryInfo.Exists)
        {
          Debug.Log("Create directory " + directoryName);
          directoryInfo.Create();
        }
      }
      catch
      {
        Debug.LogError("Failed to create directories: " + new DirectoryInfo(output).FullName);
      }

      string results = BuildPipeline.BuildPlayer(level_list, output, target, BuildOptions.None);
      log.Message("");

      if (results.Length == 0)
        log.Message("No Build Errors");
      else
        log.Message("Build Error:" + results);
    }
  }

  //-------------------------------------------------------------------------------------------------------------------------
  public static string[] FindScenes()
  {
    int num_scenes = 0;

    // Count active scenes.
    foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
    {
      if (scene.enabled)
        num_scenes++;
    }

    // Build the list of scenes.
    string[] scenes = new string[num_scenes];

    int x = 0;
    foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
    {
      if (scene.enabled)
        scenes[x++] = scene.path;
    }

    return (scenes);
  }


}

#endif
