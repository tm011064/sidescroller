#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System;

public class BuildMenu : ScriptableObject
{
  //-------------------------------------------------------------------------------------------------------------------------
  [MenuItem("Build/Build Web", false, 1000)]
  public static void BuildWebPlayer()
  {
    BuildTools.BuildWeb();
  }

  //-------------------------------------------------------------------------------------------------------------------------
  [MenuItem("Build/Build Win32", false, 1000)]
  public static void BuildPCPlayer()
  {
    BuildTools.BuildPC();
  }

  //-------------------------------------------------------------------------------------------------------------------------
  [MenuItem("Build/Build Android", false, 1000)]
  public static void BuildAndroidPlayer()
  {
    BuildTools.BuildAndroid();
  }

  //-------------------------------------------------------------------------------------------------------------------------
  [MenuItem("Build/Config Debug", false, 2000)]
  public static void BuildSetDebug()
  {
    PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "DEBUG");
    PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.WebPlayer, "DEBUG");
    PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "DEBUG");

    Debug.Log("Setting build to DEBUG.");
  }

  //-------------------------------------------------------------------------------------------------------------------------
  [MenuItem("Build/Config Profile", false, 2000)]
  public static void BuildSetProfile()
  {
    PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "PROFILE");
    PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.WebPlayer, "PROFILE");
    PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "PROFILE");

    Debug.Log("Setting build to Profile.");
  }

  //-------------------------------------------------------------------------------------------------------------------------
  [MenuItem("Build/Config Final", false, 2000)]
  public static void BuildSetFinal()
  {
    PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "FINAL");
    PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.WebPlayer, "FINAL");
    PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "FINAL");

    Debug.Log("Setting build to FINAL.");
  }
}

#endif

