using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
  public static GameManager instance = null;

  public PlayerController player;

  [HideInInspector]
  public GameSettings gameSettings;
  [HideInInspector]
  public PowerUpManager powerUpManager;
  [HideInInspector]
  public InputStateManager inputStateManager;

  private int _totalCoins = 0;

  public void AddCoin()
  {
    _totalCoins++;
  }

  private List<Checkpoint> _orderedSceneCheckpoints;
  private int _currentCheckpointIndex = 0;
  
  public void LoadScene()
  {
    GameObject checkpoint = null;
    switch (Application.loadedLevelName)
    {
      case "Platforms And Enemies":
        _orderedSceneCheckpoints = new List<Checkpoint>(GameObject.FindObjectsOfType<Checkpoint>());
        _orderedSceneCheckpoints.Sort((a, b) => b.index.CompareTo(a.index));

        _currentCheckpointIndex = 0;
        checkpoint = _orderedSceneCheckpoints[_currentCheckpointIndex].gameObject;
        break;

      default:
        // TODO (Roman): don't hardcode tags
        _orderedSceneCheckpoints = new List<Checkpoint>(GameObject.FindObjectsOfType<Checkpoint>());
        _orderedSceneCheckpoints.Sort((a, b) => a.index.CompareTo(b.index));

        _currentCheckpointIndex = 0;
        checkpoint = _orderedSceneCheckpoints[_currentCheckpointIndex].gameObject;
        break;
    }

    PlayerController playerController = Instantiate(GameManager.instance.player, checkpoint.transform.position, Quaternion.identity) as PlayerController;

    playerController.spawnLocation = checkpoint.transform.position;

    this.player = playerController;
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
    inputStateManager = new InputStateManager();
    inputStateManager.InitializeButtons("Jump", "Dash", "Fall", "SwitchPowerUp", "Attack");
    inputStateManager.InitializeAxes("Horizontal", "Vertical");

    DontDestroyOnLoad(gameObject);
  }

  void Update()
  {
    inputStateManager.Update();

    // TODO (Roman): this must not make it into release
//#if !FINAL

    if (Input.GetKeyUp("n"))
    {
      Debug.Log("Key Command: Go to next checkpoint");

      _currentCheckpointIndex--;
      if (_currentCheckpointIndex < 0)
        _currentCheckpointIndex = _orderedSceneCheckpoints.Count - 1;

      GameObject checkpoint = _orderedSceneCheckpoints[_currentCheckpointIndex].gameObject;
      this.player.spawnLocation = checkpoint.gameObject.transform.position;
      this.player.Respawn();
    }
    if (Input.GetKeyUp("p"))
    {
      Debug.Log("Key Command: Go to previous checkpoint");

      _currentCheckpointIndex++;
      if (_currentCheckpointIndex >= _orderedSceneCheckpoints.Count)
        _currentCheckpointIndex = 0;

      GameObject checkpoint = _orderedSceneCheckpoints[_currentCheckpointIndex].gameObject;
      this.player.spawnLocation = checkpoint.gameObject.transform.position;
      this.player.Respawn();
    }
    if (Input.GetKeyUp("z"))
    {
      Debug.Log("Key Command: add all powerups");

      GameManager.instance.powerUpManager.ApplyPowerUpItem(PowerUpType.Floater);
      GameManager.instance.powerUpManager.ApplyPowerUpItem(PowerUpType.DoubleJump);
      GameManager.instance.powerUpManager.ApplyPowerUpItem(PowerUpType.SpinMeleeAttack);
    }


//#endif

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
    GUIDrawRect(new Rect(0, 0, 60, 22), Color.red);
    GUI.Label(new Rect(4, 0, 60, 22), fps_string);
#endif
  }

#if !FINAL
  private static Texture2D _staticRectTexture;
  private static GUIStyle _staticRectStyle;

  // Note that this function is only meant to be called from OnGUI() functions.
  public static void GUIDrawRect(Rect position, Color color)
  {
    if (_staticRectTexture == null)
    {
      _staticRectTexture = new Texture2D(1, 1);
    }

    if (_staticRectStyle == null)
    {
      _staticRectStyle = new GUIStyle();
    }

    _staticRectTexture.SetPixel(0, 0, color);
    _staticRectTexture.Apply();

    _staticRectStyle.normal.background = _staticRectTexture;

    GUI.Box(position, GUIContent.none, _staticRectStyle);
  }
#endif

  #endregion
}
