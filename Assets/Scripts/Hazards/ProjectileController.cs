using UnityEngine;
using System.Collections;
using System;

public class ProjectileController : MonoBehaviour
{
  private GameManager _gameManager;

  void OnTriggerStay2D(Collider2D col)
  {
    if (col.gameObject == _gameManager.player.gameObject)
    {
      if (_gameManager.player.isInvincible)
        return;

      ObjectPoolingManager.Instance.Deactivate(this.gameObject);

      switch (_gameManager.powerUpManager.AddDamage())
      {
        case PowerUpManager.DamageResult.IsDead:
          return;

        default:

          _gameManager.player.PushControlHandler(new DamageTakenPlayerControlHandler());
          break;
      }
    }
  }

  void OnTriggerEnter2D(Collider2D col)
  {
    // we have to check for player as the hazard might have collided with a hazard destroy trigger
    if (col.gameObject == _gameManager.player.gameObject)
    {
      if (_gameManager.player.isInvincible)
        return;

      ObjectPoolingManager.Instance.Deactivate(this.gameObject);

      switch (_gameManager.powerUpManager.AddDamage())
      {
        case PowerUpManager.DamageResult.IsDead:
          return;

        default:

          _gameManager.player.PushControlHandler(new DamageTakenPlayerControlHandler());
          break;
      }
    }
  }
  #region private members
  private CustomStack<BaseProjectileControlHandler> _controlHandlers = new CustomStack<BaseProjectileControlHandler>();
  private BaseProjectileControlHandler _currentBaseProjectileControlHandler = null;
  #endregion

  #region properties
  public BaseProjectileControlHandler CurrentControlHandler { get { return _currentBaseProjectileControlHandler; } }
  #endregion

  private void TryActivateCurrentControlHandler(BaseProjectileControlHandler previousControlHandler)
  {
    _currentBaseProjectileControlHandler = _controlHandlers.Peek();

    while (_currentBaseProjectileControlHandler != null
      && !_currentBaseProjectileControlHandler.TryActivate(previousControlHandler))
    {
      previousControlHandler = _controlHandlers.Pop();
      Logger.Info("Popped handler: " + previousControlHandler.ToString());
      previousControlHandler.Dispose();

      _currentBaseProjectileControlHandler = _controlHandlers.Peek();
    }
  }


  void Update()
  {
    try
    {
      while (!_currentBaseProjectileControlHandler.Update())
      {
        BaseProjectileControlHandler poppedHandler = _controlHandlers.Pop();
        poppedHandler.Dispose();

        Logger.Info("Popped handler: " + poppedHandler.ToString());
        TryActivateCurrentControlHandler(poppedHandler);
      }
    }
    catch (Exception err)
    {
      Logger.Error("Game object " + this.name + " misses default control handler.", err);
      throw;
    }
  }
  
  #region control handlers
  /// <summary>
  /// Resets the control handlers.
  /// </summary>
  /// <param name="controlHandler">The control handler.</param>
  public void ResetControlHandlers(BaseProjectileControlHandler controlHandler)
  {
    Logger.Info("Resetting character control handlers.");
    for (int i = _controlHandlers.Count - 1; i >= 0; i--)
    {
      Logger.Info("Removing handler: " + _controlHandlers[i].ToString());
      _controlHandlers[i].Dispose();
      _controlHandlers.RemoveAt(i);
    }

    _currentBaseProjectileControlHandler = null;
    PushControlHandler(controlHandler);
  }
  /// <summary>
  /// Pushes the control handler.
  /// </summary>
  /// <param name="controlHandlers">The control handlers.</param>
  public void PushControlHandler(params BaseProjectileControlHandler[] controlHandlers)
  {
    for (int i = 0; i < controlHandlers.Length; i++)
    {
      Logger.Info("Pushing (chained) handler: " + controlHandlers[i].ToString());
      _controlHandlers.Push(controlHandlers[i]);
    }

    TryActivateCurrentControlHandler(_currentBaseProjectileControlHandler);
  }
  public void InsertControlHandler(int index, BaseProjectileControlHandler controlHandler)
  {
    Logger.Info("Pushing handler: " + controlHandler.ToString());

    if (index >= _controlHandlers.Count)
    {
      PushControlHandler(controlHandler);
    }
    else
    {
      _controlHandlers.Insert(index, controlHandler);
    }
  }
  /// <summary>
  /// Pushes the control handler.
  /// </summary>
  /// <param name="controlHandler">The control handler.</param>
  public void PushControlHandler(BaseProjectileControlHandler controlHandler)
  {
    Logger.Info("Pushing handler: " + controlHandler.ToString());

    _controlHandlers.Push(controlHandler);
    TryActivateCurrentControlHandler(_currentBaseProjectileControlHandler);
  }
  /// <summary>
  /// Removes the control handler.
  /// </summary>
  /// <param name="controlHandler">The control handler.</param>
  public void RemoveControlHandler(BaseProjectileControlHandler controlHandler)
  {
    Logger.Info("Removing handler: " + controlHandler.ToString());

    if (controlHandler == _currentBaseProjectileControlHandler)
    {
      BaseProjectileControlHandler poppedHandler = _controlHandlers.Pop();
      poppedHandler.Dispose();

      TryActivateCurrentControlHandler(poppedHandler);
    }
    else
    {
      _controlHandlers.Remove(controlHandler);
      controlHandler.Dispose();
    }
  }
  /// <summary>
  /// Exchanges the control handler.
  /// </summary>
  /// <param name="index">The index.</param>
  /// <param name="controlHandler">The control handler.</param>
  public void ExchangeControlHandler(int index, BaseProjectileControlHandler controlHandler)
  {
    Logger.Info("Exchanging handler " + _controlHandlers[index].ToString() + " (index: " + index + ") with " + controlHandler.ToString());

    if (_controlHandlers[index] == _currentBaseProjectileControlHandler)
    {
      BaseProjectileControlHandler poppedHandler = _controlHandlers.Exchange(index, controlHandler);
      poppedHandler.Dispose();

      TryActivateCurrentControlHandler(poppedHandler);
    }
    else
    {
      _controlHandlers.Exchange(index, controlHandler);
    }
  }
  #endregion

  void Awake()
  {
    _gameManager = GameManager.instance;
  }
}
