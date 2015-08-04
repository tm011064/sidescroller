using System;
using System.Collections.Generic;
using UnityEngine;

public class BaseCharacterController : BaseMonoBehaviour
{
  #region public fields
  [HideInInspector]
  public CharacterPhysicsManager characterPhysicsManager;
  #endregion

  #region private members
  private CustomStack<BaseControlHandler> _controlHandlers = new CustomStack<BaseControlHandler>();
  private BaseControlHandler _currentBaseControlHandler = null;
  #endregion

  #region properties
  public BaseControlHandler CurrentControlHandler { get { return _currentBaseControlHandler; } }
  #endregion

  #region methods

  #region private
  private void TryActivateCurrentControlHandler(BaseControlHandler previousControlHandler)
  {
    _currentBaseControlHandler = _controlHandlers.Peek();

    while (_currentBaseControlHandler != null
      && !_currentBaseControlHandler.TryActivate(previousControlHandler))
    {
      previousControlHandler = _controlHandlers.Pop();
      Logger.Info("Popped handler: " + previousControlHandler.ToString());
      previousControlHandler.Dispose();

      _currentBaseControlHandler = _controlHandlers.Peek();
    }
  }

  protected virtual void Update()
  {
    try
    {
      while (!_currentBaseControlHandler.Update())
      {
        BaseControlHandler poppedHandler = _controlHandlers.Pop();
        poppedHandler.Dispose();

        Logger.Info("Popped handler: " + poppedHandler.ToString());
        TryActivateCurrentControlHandler(poppedHandler);
      }

      // after we updated the control handler, we now want to notify all stack members (excluding the current handler/peek) that an update
      // has occurred. This is necessary in case stacked handlers need to react to actions, for example: melee attack is interrupted by wall jump handler
      for (int i = _controlHandlers.Count - 2; i >= 0; i--)
        _controlHandlers[i].OnAfterStackPeekUpdate();
    }
    catch (Exception err)
    {
      Logger.Error("Game object " + this.name + " misses default control handler.", err);
      throw;
    }
  }
  #endregion

  #region control handlers
  /// <summary>
  /// Resets the control handlers.
  /// </summary>
  /// <param name="controlHandler">The control handler.</param>
  public void ResetControlHandlers(BaseControlHandler controlHandler)
  {
    Logger.Info("Resetting character control handlers.");
    for (int i = _controlHandlers.Count - 1; i >= 0; i--)
    {
      Logger.Info("Removing handler: " + _controlHandlers[i].ToString());
      _controlHandlers[i].Dispose();
      _controlHandlers.RemoveAt(i);
    }

    _currentBaseControlHandler = null;
    PushControlHandler(controlHandler);
  }
  /// <summary>
  /// Pushes the control handler.
  /// </summary>
  /// <param name="controlHandlers">The control handlers.</param>
  public void PushControlHandler(params BaseControlHandler[] controlHandlers)
  {
    for (int i = 0; i < controlHandlers.Length; i++)
    {
      Logger.Info("Pushing (chained) handler: " + controlHandlers[i].ToString());
      _controlHandlers.Push(controlHandlers[i]);
    }

    TryActivateCurrentControlHandler(_currentBaseControlHandler);
  }
  public void InsertControlHandler(int index, BaseControlHandler controlHandler)
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
  public void PushControlHandler(BaseControlHandler controlHandler)
  {
    Logger.Info("Pushing handler: " + controlHandler.ToString());

    _controlHandlers.Push(controlHandler);
    TryActivateCurrentControlHandler(_currentBaseControlHandler);
  }
  /// <summary>
  /// Removes the control handler.
  /// </summary>
  /// <param name="controlHandler">The control handler.</param>
  public void RemoveControlHandler(BaseControlHandler controlHandler)
  {
    Logger.Info("Removing handler: " + controlHandler.ToString());

    if (controlHandler == _currentBaseControlHandler)
    {
      BaseControlHandler poppedHandler = _controlHandlers.Pop();
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
  public void ExchangeControlHandler(int index, BaseControlHandler controlHandler)
  {
    Logger.Info("Exchanging handler " + _controlHandlers[index].ToString() + " (index: " + index + ") with " + controlHandler.ToString());

    if (_controlHandlers[index] == _currentBaseControlHandler)
    {
      BaseControlHandler poppedHandler = _controlHandlers.Exchange(index, controlHandler);
      poppedHandler.Dispose();

      TryActivateCurrentControlHandler(poppedHandler);
    }
    else
    {
      _controlHandlers.Exchange(index, controlHandler);
    }
  }
  #endregion

  #endregion
}
