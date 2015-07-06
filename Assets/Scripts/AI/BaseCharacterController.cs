using System.Collections.Generic;
using UnityEngine;

public class BaseCharacterController : BaseMonoBehaviour
{
  [HideInInspector]
  public CharacterPhysicsManager characterPhysicsManager;
  protected CustomStack<BaseControlHandler> _controlHandlers = new CustomStack<BaseControlHandler>();

  public void PushControlHandler(BaseControlHandler controlHandler)
  {
    _controlHandlers.Push(controlHandler);
  }
  public void RemoveControlHandler(BaseControlHandler controlHandler)
  {
    _controlHandlers.Remove(controlHandler);
  }
  public void ExchangeControlHandler(int index, BaseControlHandler controlHandler)
  {
    _controlHandlers.Exchange(index, controlHandler);
  }

  protected virtual void Update()
  {
    BaseControlHandler handler = _controlHandlers.Peek();
    while (!handler.Update())
    {
      BaseControlHandler poppedHandler = _controlHandlers.Pop();
      Logger.Info("Popped handler: " + poppedHandler.ToString());
      handler = _controlHandlers.Peek();
    }
  }
}
