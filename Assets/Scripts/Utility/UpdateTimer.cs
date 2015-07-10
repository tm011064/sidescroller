using UnityEngine;

public class UpdateTimer
{
  private bool _hasStarted;
  private bool _hasEnded;
  private float _startTime;
  private float _endTime;
  protected float _duration;

  protected virtual void DoUpdate(float currentTime) { }

  public void Update()
  {
    if (_hasStarted && !_hasEnded)
    {
      float currentTime = Time.time - _startTime;
      if (currentTime <= _duration)
        DoUpdate(currentTime);
      else
        _hasEnded = true;
    }
  }

  public void Start()
  {
    _hasStarted = true;
    _startTime = Time.time;
    _endTime = _startTime + _duration;
  }

  public bool HasEnded { get { return _hasEnded; } }

  public UpdateTimer(float duration)
  {
    _duration = duration;
  }
}