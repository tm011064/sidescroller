#if UNITY_EDITOR

using UnityEngine;
using System.Collections;
using System.IO;
using System;

public class LogFile : IDisposable
{
  //-------------------------------------------------------------------------------------------------------------------------
  // A simple log to file system.

  //-------------------------------------------------------------------------------------------------------------------------
  private StreamWriter _logStream;
  private bool _echo;

  //-------------------------------------------------------------------------------------------------------------------------
  public LogFile(string filename, bool echo_to_debug)
  {
    FileInfo fileInfo = new FileInfo(filename);
    if (!fileInfo.Exists)
    {
      if (!fileInfo.Directory.Exists)
        fileInfo.Directory.Create();

      if (fileInfo.Exists)
        fileInfo.Create();
    }
    
    _logStream = new StreamWriter(filename);

    _echo = echo_to_debug;
  }

  //-------------------------------------------------------------------------------------------------------------------------
  public void Message(string msg)
  {
    if (_logStream != null)
    {
      _logStream.WriteLine(msg);

      if (_echo)
        Debug.Log(msg);
    }
  }

  #region IDisposable Members

  public void Dispose()
  {
    try
    {
      _logStream.Dispose();
    }
    catch (Exception err)
    {
      Debug.LogException(err);
    }
  }

  #endregion
}


#endif
