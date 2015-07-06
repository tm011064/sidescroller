﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public enum PowerUpType
{
  Basic = 1,
  Floater = 2,
  DoubleJump = 4,
  Boots = 8,
  ForceField = 16,
}

[Serializable]
public class PowerUpManager
{
  private GameManager _gameManager;

  public PowerUpManager(GameManager gameManager)
  {
    _gameManager = gameManager;
  }

  private int _powerMeter;
  [HideInInspector]
  public int PowerMeter
  {
    get { return _powerMeter; }
    set { _powerMeter = value; }
  }


  public void ApplyNextInventotyPowerUpItem()
  {
    Logger.Info("Switch to next powerup item");
    if (_orderedPowerUpInventory.Count > 0)
    {
      int index = 0;
      if (_currentPowerUpItem.HasValue)
      {
        index = _orderedPowerUpInventory.IndexOf(_currentPowerUpItem.Value);

        if (index == _orderedPowerUpInventory.Count - 1)
          index = 0;
        else
          index++;
      }

      ApplyPowerUpItem(_orderedPowerUpInventory[index]);
    }
  }

  private List<PowerUpType> _orderedPowerUpInventory = new List<PowerUpType>();

  private PowerUpType? _currentPowerUpItem = null;
  private BaseControlHandler _currentPowerUpControlHandler = null;

  public void KillPlayer()
  {
    if (_currentPowerUpItem.HasValue)
    {
      _orderedPowerUpInventory.Remove(_currentPowerUpItem.Value);

      _currentPowerUpControlHandler.Dispose();
      _gameManager.player.RemoveControlHandler(_currentPowerUpControlHandler);

      _currentPowerUpControlHandler = null;
      _currentPowerUpItem = null;
      Logger.Info("Added damage, removed power up item. " + this.ToString());
    }

    // player died
    Logger.Info("Added damage, respawn. " + this.ToString());
    _gameManager.player.Respawn();
  }
  public void AddDamage()
  {
    if (_currentPowerUpItem.HasValue)
    {
      _orderedPowerUpInventory.Remove(_currentPowerUpItem.Value);

      _currentPowerUpControlHandler.Dispose();
      _gameManager.player.RemoveControlHandler(_currentPowerUpControlHandler);

      _currentPowerUpControlHandler = null;
      _currentPowerUpItem = null;
      Logger.Info("Added damage, removed power up item. " + this.ToString());
    }
    else
    {
      if (_powerMeter == 1)
      {
        _powerMeter = 0; // TODO (Roman): notify player for rendering...    
        _gameManager.player.ExchangeControlHandler(0, new BadHealthPlayerControlHandler(_gameManager.player, float.MaxValue));
        Logger.Info("Added damage, reduced power meter. " + this.ToString());
      }
      else
      {
        // player died
        Logger.Info("Added damage, respawn. " + this.ToString());
        _gameManager.player.Respawn();
      }
    }
  }

  public void ApplyPowerUpItem(PowerUpType powerUpType)
  {
    if (powerUpType == PowerUpType.Basic)
    {
      _powerMeter = 1;
      _gameManager.player.ExchangeControlHandler(0, new GoodHealthPlayerControlHandler(_gameManager.player, float.MaxValue));
      Logger.Info("Added basic power up. " + this.ToString());
    }
    else
    {
      if (_powerMeter != 1)
      {
        // when getting a non basic power up, we want to set the power meter to 1 so we automatically go back to that state in case we lose the
        // power up.
        _powerMeter = 1;
        _gameManager.player.ExchangeControlHandler(0, new GoodHealthPlayerControlHandler(_gameManager.player, float.MaxValue));
        Logger.Info("Added basic power up. " + this.ToString());
      }

      if (!_currentPowerUpItem.HasValue || _currentPowerUpItem.Value != powerUpType)
      {
        if (_currentPowerUpItem.HasValue)
        {
          _gameManager.player.RemoveControlHandler(_currentPowerUpControlHandler);
        }

        switch (powerUpType)
        {
          case PowerUpType.Floater:
            _currentPowerUpControlHandler = new PowerUpFloaterControlHandler(_gameManager.player, _gameManager.gameSettings.powerUpSettings);
            break;

          case PowerUpType.DoubleJump:
            _currentPowerUpControlHandler = new PowerUpDoubleJumpControlHandler(_gameManager.player);
            break;
        }

        _gameManager.player.PushControlHandler(_currentPowerUpControlHandler);
        _currentPowerUpItem = powerUpType;

        if (!_orderedPowerUpInventory.Contains(powerUpType))
        {
          _orderedPowerUpInventory.Add(powerUpType);
          _orderedPowerUpInventory.Sort();
        }

        Logger.Info("Added " + powerUpType.ToString() + " power up. " + this.ToString());
      }
    }
  }

  public override string ToString()
  {
    StringBuilder sb = new StringBuilder();
    foreach (PowerUpType powerUpType in _orderedPowerUpInventory)
      sb.Append(powerUpType.ToString() + ", ");

    return "Power Meter: " + _powerMeter
      + ", Current Power-Up: " + (_currentPowerUpItem.HasValue ? _currentPowerUpItem.Value.ToString() : "NULL")
      + ", Inventory: " + (sb.Length == 0 ? "empty" : sb.ToString().TrimEnd(", ".ToCharArray()));
  }

}
