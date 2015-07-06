using UnityEngine;
using System.Collections;
using System;

public class GameManager : MonoBehaviour
{
  public static GameManager instance = null;

  public PlayerController player;

  [HideInInspector]
  public GameSettings gameSettings;
  [HideInInspector]
  public PowerUpManager powerUpManager;

  private int _totalCoins = 0;

  public void AddCoin()
  {
    _totalCoins++;
  }

  void Awake()
  {
    Logger.Info("Awaking Game Manager at " + DateTime.Now.ToString());

    if (instance == null)
    {
      Logger.Info("Setting Game Manager instance.");
      instance = this;
    }
    else if (instance != this)
    {
      Logger.Info("Destroying Game Manager instance.");
      Destroy(gameObject);
    }

    powerUpManager = new PowerUpManager(this);

    DontDestroyOnLoad(gameObject);
  }

  void Update()
  {
    if (Input.GetKey("escape"))
    {
      Logger.Info("quit");
      Application.Quit();
    }

    UpdateFPS();
  }

  void OnDestroy()
  {
    Logger.Destroy();
  }
  
  #region fps draws
#if !FINAL
  private float FPS;
  private float FPSTime;
  private int FPSFrames;
#endif

  //-------------------------------------------------------------------------------------------------------------------------
  void OnGUI()
  {
    RenderFPS();
  }

  //-------------------------------------------------------------------------------------------------------------------------
  void InitFPS()
  {
#if !FINAL
    // Clear the fps counters.
    FPS = 0.0f;
    FPSTime = 0.0f;
    FPSFrames = 0;
#endif
  }

  //-------------------------------------------------------------------------------------------------------------------------
  void UpdateFPS()
  {
#if !FINAL
    // Increase the number of frames.
    FPSFrames++;

    // Accumulate time.
    FPSTime += Time.deltaTime;

    // Have we reached 1 second.
    if (FPSTime > 1.0f)
    {
      // Store number of frames per second.
      FPS = FPSFrames;

      // Reset for the next frame.
      FPSTime -= 1.0f;
      FPSFrames = 0;
    }
#endif
  }

  //-------------------------------------------------------------------------------------------------------------------------
  void RenderFPS()
  {
#if !FINAL
    // Display on screen the current Frames Per Second.
    string fps_string = string.Format("FPS: {0:d2}", (int)FPS);
    GUI.Label(new Rect(10, 10, 600, 30), fps_string);
#endif
  }
  #endregion
}
