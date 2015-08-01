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

public enum ButtonPressState
{
  Idle = 1,
  IsDown = 2,
  IsPressed = 4,
  IsUp = 8,
}

public class ButtonsState
{
  private const string TRACE_TAG = "ButtonsState";

  private string _buttonName;
  private float _pressStarted;

  // is down check: ((buttonPressState & ButtonPressState.IsDown) != 0)
  // is up check: ((buttonPressState & ButtonPressState.IsUp) != 0)
  // is pressed check: ((buttonPressState & ButtonPressState.IsPressed) != 0)
  public ButtonPressState buttonPressState;

  //public bool IsDown;
  //public bool IsUp;
  //public bool IsPressed;

  public void Update()
  {
    ButtonPressState state = ButtonPressState.Idle;

    //bool isPressed = Input.GetButton(_buttonName);
    if (Input.GetButton(_buttonName))
      state |= ButtonPressState.IsPressed;

    if (((state & ButtonPressState.IsPressed) != 0)               // IF   currently pressed
      && ((buttonPressState & ButtonPressState.IsPressed) == 0))  // AND  previously not pressed
    {
      _pressStarted = Time.time;

      state |= ButtonPressState.IsDown;

      Logger.Trace(TRACE_TAG, "Button " + _buttonName + " is down.");
    }

    if (((state & ButtonPressState.IsPressed) == 0)               // IF   currently not pressed
      && ((buttonPressState & ButtonPressState.IsPressed) != 0))  // AND  previously pressed
    {
      state |= ButtonPressState.IsUp;
    }

    if ((state & (ButtonPressState.IsUp | ButtonPressState.IsDown | ButtonPressState.IsPressed)) != 0)
      state &= ~ButtonPressState.Idle;

    buttonPressState = state;

    //bool isPressed = Input.GetButton(_buttonName);

    //if (isPressed && !this.IsPressed)
    //{
    //  _pressStarted = Time.time;
    //  this.IsDown = true;

    //  Logger.Trace(TRACE_TAG, "Button " + _buttonName + " is down.");
    //}
    //else
    //{
    //  this.IsDown = false;
    //}

    //this.IsUp = (this.IsPressed && !isPressed);
    //this.IsPressed = isPressed;

    //if (this.IsDown)
    //  Logger.Assert(((state & ButtonPressState.IsDown) != 0), "DOWN wrong");
    //else
    //  Logger.Assert(((state & ButtonPressState.IsDown) == 0), "DOWN wrong");
    //if (this.IsUp)
    //  Logger.Assert(((state & ButtonPressState.IsUp) != 0), "IsUp wrong");
    //else
    //  Logger.Assert(((state & ButtonPressState.IsUp) == 0), "IsUp wrong");
    //if (this.IsPressed)
    //  Logger.Assert(((state & ButtonPressState.IsPressed) != 0), "IsPressed wrong");
    //else
    //  Logger.Assert(((state & ButtonPressState.IsPressed) == 0), "IsPressed wrong");
    //if (this.IsPressed || this.IsUp || this.IsDown)
    //  Logger.Assert(((state & ButtonPressState.Idle) == 0), "Idle wrong");
    //else
    //  Logger.Assert(((state & ButtonPressState.Idle) != 0), "Idle wrong");
    //Debug.Log("test done");
  }

  public float GetPressedTime()
  {
    if (((buttonPressState & ButtonPressState.IsPressed) == 0))
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

