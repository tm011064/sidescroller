using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class InputState
{
  private string _buttonName;
  private float _pressStarted;

  public bool IsDown;
  public bool IsUp;
  public bool IsPressed;

  public void Update()
  {
    bool isPressed = Input.GetButton(_buttonName);

    if (isPressed && !this.IsPressed)
    {
      _pressStarted = Time.time;
      this.IsDown = true;

      Logger.Trace("Button " + _buttonName + " down at " + Time.time);
    }
    else
    {
      this.IsDown = false;
    }

    this.IsUp = (this.IsPressed && !isPressed);
    this.IsPressed = isPressed;
  }

  public float GetPressedTime()
  {
    if (!this.IsPressed)
    {
      return 0f;
    }
    return Time.time - _pressStarted;
  }

  public InputState(string buttonName)
  {
    _buttonName = buttonName;
  }
}

public class InputStateManager
{
  private Dictionary<string, InputState> _inputStates;

  public InputState this[string buttonName]
  {
    get
    {
      return _inputStates[buttonName];
    }
  }

  public void Update()
  {
    foreach (var value in _inputStates.Values)
    {
      value.Update();
    }
  }

  public InputStateManager(params string[] buttonNames)
  {
    _inputStates = new Dictionary<string, InputState>();

    for (int i = 0; i < buttonNames.Length; i++)
    {
      _inputStates[buttonNames[i]] = new InputState(buttonNames[i]);
    }
  }
}

