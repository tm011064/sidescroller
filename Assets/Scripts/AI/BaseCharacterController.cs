using System;
using System.Collections.Generic;
using UnityEngine;

public class BaseCharacterController : BaseMonoBehaviour
{
  [HideInInspector]
  public CharacterPhysicsManager characterPhysicsManager;
  private CustomStack<BaseControlHandler> _controlHandlers = new CustomStack<BaseControlHandler>();

  private BaseControlHandler _currentBaseControlHandler = null;
  protected BaseControlHandler CurrentControlHandler { get { return _currentBaseControlHandler; } }

  public void ResetControlHandlers(BaseControlHandler controlHandler)
  {
    Logger.Info("Resetting character control handlers.");
    for (int i = _controlHandlers.Count - 1; i >= 0; i--)
    {
      Logger.Info("Removing handler: " + _controlHandlers[i].ToString());
      _controlHandlers[i].Dispose();
      _controlHandlers.RemoveAt(i);
    }

    PushControlHandler(controlHandler);
  }
  public void PushControlHandler(BaseControlHandler controlHandler)
  {
    Logger.Info("Pushing handler: " + controlHandler.ToString());
    _controlHandlers.Push(controlHandler);

    _currentBaseControlHandler = controlHandler;
  }
  public void RemoveControlHandler(BaseControlHandler controlHandler)
  {
    Logger.Info("Removing handler: " + controlHandler.ToString());
    _controlHandlers.Remove(controlHandler);
    controlHandler.Dispose();

    _currentBaseControlHandler = _controlHandlers.Peek();
  }
  public void ExchangeControlHandler(int index, BaseControlHandler controlHandler)
  {
    Logger.Info("Exchanging handler " + _controlHandlers[index].ToString() + " (index: " + index + ") with " + controlHandler.ToString());
    _controlHandlers.Exchange(index, controlHandler);

    _currentBaseControlHandler = _controlHandlers.Peek();
  }

  protected virtual void Update()
  {
    BaseControlHandler handler = _controlHandlers.Peek();
    try
    {
      while (!handler.Update())
      {
        BaseControlHandler poppedHandler = _controlHandlers.Pop();

        Logger.Info("Popped handler: " + poppedHandler.ToString());
        poppedHandler.Dispose();

        handler = _controlHandlers.Peek();
        _currentBaseControlHandler = handler;
      }
    }
    catch (Exception err)
    {
      Logger.Error("Game object " + this.name + " misses default control handler.", err);
      throw;
    }
  }
}
