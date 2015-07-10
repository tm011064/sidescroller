using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class AxisState
{
  private string _axisName;

  public float value;
  public float lastValue;

  public void Update()
  {
    lastValue = value;
    value = Input.GetAxis(_axisName);
  }

  public AxisState Clone()
  {
    return (AxisState)this.MemberwiseClone();
  }

  public AxisState(float value)
  {
    this.value = value;
  }

  public AxisState(string axisName)
  {
    _axisName = axisName;
  }
}

public class ButtonsState
{
  private const string TRACE_TAG = "ButtonsState";

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

      Logger.Trace(TRACE_TAG, "Button " + _buttonName + " is down.");
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

  public ButtonsState(string buttonName)
  {
    _buttonName = buttonName;
  }
}

public class InputStateManager
{
  private Dictionary<string, ButtonsState> _buttonStates;
  private Dictionary<string, AxisState> _axisStates;

  public ButtonsState GetButtonState(string buttonName)
  {
    return _buttonStates[buttonName];
  }
  public AxisState GetAxisState(string axisName)
  {
    return _axisStates[axisName];
  }

  public void Update()
  {
    foreach (var value in _buttonStates.Values)
    {
      value.Update();
    }
    foreach (var value in _axisStates.Values)
    {
      value.Update();
    }
  }

  public void InitializeButtons(params string[] buttonNames)
  {
    for (int i = 0; i < buttonNames.Length; i++)
    {
      _buttonStates[buttonNames[i]] = new ButtonsState(buttonNames[i]);
    }
  }
  public void InitializeAxes(params string[] azisNames)
  {
    for (int i = 0; i < azisNames.Length; i++)
    {
      _axisStates[azisNames[i]] = new AxisState(azisNames[i]);
    }
  }

  public InputStateManager()
  {
    _buttonStates = new Dictionary<string, ButtonsState>();
    _axisStates = new Dictionary<string, AxisState>();
  }
}

