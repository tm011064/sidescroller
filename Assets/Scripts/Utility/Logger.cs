using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Threading;

public class Logger : IDisposable
{
  #region nested
  private enum LogLevel
  {
    Trace,
    Info,
    Warning,
    Error,
    Assert
  };
  #endregion

  #region members
  private static Logger _logger;

  private LogSettings _loggerSettings;
  private StreamWriter _outputStream;
  private HashSet<string> _enabledTraceTags;

  private Timer _writeTimer;
  private List<string> _messagesToWrite = new List<string>();
  private readonly object _messagesToWriteLock = new object();
  #endregion

  #region properties
  public bool BreakOnError { get { return _loggerSettings.breakOnError; } }
  public bool BreakOnAssert { get { return _loggerSettings.breakOnAssert; } }
  #endregion

  #region methods
  private void Write(string traceTag, LogLevel type, string message)
  {
#if !FINAL
    if (!string.IsNullOrEmpty(traceTag))
    {
      if (!_loggerSettings.enableAllTraceTags && !_enabledTraceTags.Contains(traceTag))
        return;

      if (_loggerSettings.addTraceTagToMessage)
        message = "[" + traceTag + "] " + message;
    }

    if (_loggerSettings.addTimeStamp)
    {
      message = Time.time.ToString(".0000000") + " " + message;
    }

    lock (_messagesToWriteLock)
    {
      _messagesToWrite.Add(message);
    }

    if (_loggerSettings.echoToConsole)
    {
      if (type == LogLevel.Trace || type == LogLevel.Info)
        UnityEngine.Debug.Log(message);
      else if (type == LogLevel.Warning)
        UnityEngine.Debug.LogWarning(message);
      else // Both Error and Assert go here.
        UnityEngine.Debug.LogError(message);
    }
#endif
  }

  private void WriteToDisk(object obj)
  {
    if (_outputStream != null)
    {
      List<string> messages;
      lock (_messagesToWrite)
      {
        // make a local copy so we don't lock the main thread while writing
        messages = new List<string>(_messagesToWrite);
        _messagesToWrite = new List<string>();
      }

      for (int i = 0; i < messages.Count; i++)
      {
        _outputStream.WriteLine(messages[i]);
        _outputStream.Flush();
      }
    }
  }

  #region static
  [Conditional("DEBUG"), Conditional("PROFILE")]
  public static void Info(string msg)
  {
#if !FINAL
    if (_logger != null)
    {
      _logger.Write(null, LogLevel.Info, msg);
    }
    else
    {
      UnityEngine.Debug.Log(msg);
    }
#endif
  }

  [Conditional("DEBUG"), Conditional("PROFILE")]
  public static void Trace(string tag, string msg, params object[] args)
  {
#if !FINAL
    if (_logger != null)
    {
      _logger.Write(tag, LogLevel.Trace, string.Format(msg, args));
    }
    else
    {
      UnityEngine.Debug.Log(msg);
    }
#endif
  }
  [Conditional("DEBUG"), Conditional("PROFILE")]
  public static void Trace(string msg)
  {
#if !FINAL
    if (_logger != null)
    {
      _logger.Write(null, LogLevel.Trace, msg);
    }
    else
    {
      UnityEngine.Debug.Log(msg);
    }
#endif
  }
  [Conditional("DEBUG"), Conditional("PROFILE")]
  public static void Trace(object obj)
  {
#if !FINAL
    if (obj == null)
      Trace("NULL");
    else
      Trace(obj.ToString());
#endif
  }
  [Conditional("DEBUG"), Conditional("PROFILE")]
  public static void Trace(string tag, object obj)
  {
#if !FINAL
    if (obj == null)
      Trace(tag, "NULL");
    else
      Trace(tag, obj.ToString());
#endif
  }

  [Conditional("DEBUG"), Conditional("PROFILE")]
  public static void Warning(string msg)
  {
#if !FINAL
    if (_logger != null)
      _logger.Write(null, LogLevel.Warning, msg);
    else
    {
      UnityEngine.Debug.Log(msg);
    }
#endif
  }

  [Conditional("DEBUG"), Conditional("PROFILE")]
  public static void Error(Exception err)
  {
#if !FINAL
    Error(err.Message + Environment.NewLine + err.StackTrace);
#endif
  }

  [Conditional("DEBUG"), Conditional("PROFILE")]
  public static void Error(string msg, Exception err)
  {
#if !FINAL
    Error(msg + Environment.NewLine + err.StackTrace);
#endif
  }

  [Conditional("DEBUG"), Conditional("PROFILE")]
  public static void Error(string msg)
  {
#if !FINAL
    if (_logger != null)
    {
      _logger.Write(null, LogLevel.Error, msg);

      if (_logger.BreakOnError)
        UnityEngine.Debug.Break();
    }
    else
    {
      UnityEngine.Debug.Log(msg);
    }
#endif
  }

  [Conditional("DEBUG"), Conditional("PROFILE")]
  public static void Assert(bool condition, string msg)
  {
#if !FINAL
    if (condition)
      return;

    if (_logger != null)
    {
      _logger.Write(null, LogLevel.Assert, msg);

      if (_logger.BreakOnAssert)
        UnityEngine.Debug.Break();
    }
#endif
  }
  #endregion

  #endregion

  #region IDisposable Members

  public void Dispose()
  {
#if !FINAL
    if (_outputStream != null)
    {
      // write remaining messages
      WriteToDisk(null);
      _writeTimer.Dispose();

      try
      {
        _outputStream.Dispose();
      }
      catch (Exception err)
      {
        UnityEngine.Debug.LogException(err);
      }

      try
      {
        FileInfo fileInfo = new FileInfo(_loggerSettings.logFile);

        if (_loggerSettings.totalArchivedFilesToKeep > 0)
        {
          string archivedFileName = Path.Combine(fileInfo.DirectoryName, fileInfo.Name.Substring(0, fileInfo.Name.LastIndexOf('.')) + "_" + DateTime.Now.ToString("ddMMyy_HHmm") + ".txt");
          UnityEngine.Debug.Log("Archiving current log file to: " + archivedFileName);
          fileInfo.CopyTo(archivedFileName, true);
        }

        Regex regex = new Regex("^" + fileInfo.Name.Substring(0, fileInfo.Name.LastIndexOf('.')) + "_([0-9]{6})_([0-9]{4}).txt$");
        List<FileInfo> archivedFiles = new List<FileInfo>();
        foreach (FileInfo file in fileInfo.Directory.GetFiles())
        {
          if (regex.IsMatch(file.Name))
            archivedFiles.Add(file);
        }
        int totalArchivedFiles = archivedFiles.Count;
        if (totalArchivedFiles > _loggerSettings.totalArchivedFilesToKeep)
        {
          UnityEngine.Debug.Log("Running log file archive cleanup. Total archived files: " + totalArchivedFiles + ", threshold: " + _loggerSettings.totalArchivedFilesToKeep);
          archivedFiles = archivedFiles.OrderBy(c => c.CreationTimeUtc).ToList();
          for (int i = 0; i < (totalArchivedFiles - _loggerSettings.totalArchivedFilesToKeep); i++)
          {
            UnityEngine.Debug.Log("Deleting archived file: " + archivedFiles[i].FullName);
            archivedFiles[i].Delete();
          }
        }
      }
      catch (Exception err)
      {
        UnityEngine.Debug.LogException(err);
      }
    }
#endif
  }

  #endregion

  #region static methods

  public static void Restart()
  {
    if (_logger != null)
      Initialize(_logger._loggerSettings);
  }

  public static void Destroy()
  {
    if (_logger != null)
    {
      _logger.Dispose();
    }
  }

  #endregion

  #region constructors
  public static void Initialize(LogSettings logSettings)
  {
    if (_logger != null)
      _logger.Dispose();

    _logger = new Logger(logSettings);
  }

  private Logger(LogSettings logSettings)
  {
    this._loggerSettings = logSettings;
    this._enabledTraceTags = new HashSet<string>(_loggerSettings.enabledTraceTags);

#if !FINAL
    _writeTimer = new Timer(WriteToDisk, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

    FileInfo fileInfo = new FileInfo(logSettings.logFile);
    if (!fileInfo.Exists)
    {
      if (!fileInfo.Directory.Exists)
        fileInfo.Directory.Create();

      if (fileInfo.Exists)
        fileInfo.Create();
    }
    UnityEngine.Debug.Log("Logger initialized. File location: " + fileInfo.FullName + ", Time: " + DateTime.Now.ToString());

    // Open the log file to append the new log to it.
    _outputStream = new StreamWriter(logSettings.logFile, false);
#endif
  }

  #endregion
}