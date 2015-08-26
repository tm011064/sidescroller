using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public enum PowerUpType
{
  Basic = 1,
  Floater = 2,
  DoubleJump = 4,
  SpinMeleeAttack = 8,
  TaserAttack = 16,
  JetPack = 32,
  //Boots = 8,
  //ForceField = 16,
  Gun
}

[Serializable]
public class PowerUpManager
{
  public enum DamageResult
  {
    IsDead,
    LostPower,
    LostItem
  }

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


  public void ApplyNextInventoryPowerUpItem()
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
    
    // TODO (Roman): this should be somewhere else - this is just test code
    GameObject deathParticles = ObjectPoolingManager.Instance.GetObject(GameManager.instance.gameSettings.pooledObjects.defaultPlayerDeathParticlePrefab.prefab.name);
    deathParticles.transform.position = _gameManager.player.gameObject.transform.position;

    _gameManager.player.Respawn();
  }
  public DamageResult AddDamage()
  {
    if (_currentPowerUpItem.HasValue)
    {
      _orderedPowerUpInventory.Remove(_currentPowerUpItem.Value);
      
      _currentPowerUpControlHandler.Dispose();
      _gameManager.player.RemoveControlHandler(_currentPowerUpControlHandler);

      _currentPowerUpControlHandler = null;
      _currentPowerUpItem = null;
      Logger.Info("Added damage, removed power up item. " + this.ToString());
      return DamageResult.LostItem;
    }
    else
    {
      if (_powerMeter == 1)
      {
        _powerMeter = 0; // TODO (Roman): notify player for rendering...    
        _gameManager.player.ExchangeControlHandler(0, new BadHealthPlayerControlHandler(_gameManager.player));
        Logger.Info("Added damage, reduced power meter. " + this.ToString());
        return DamageResult.LostPower;
      }
      else
      {
        // player died
        Logger.Info("Added damage, respawn. " + this.ToString());
        // TODO (Roman): this should be somewhere else - this is just test code
        GameObject deathParticles = ObjectPoolingManager.Instance.GetObject(GameManager.instance.gameSettings.pooledObjects.defaultPlayerDeathParticlePrefab.prefab.name);
        deathParticles.transform.position = _gameManager.player.gameObject.transform.position;
        _gameManager.player.Respawn();
        return DamageResult.IsDead;
      }
    }
  }

  public void ApplyPowerUpItem(PowerUpType powerUpType)
  {
    if (powerUpType == PowerUpType.Basic)
    {
      _powerMeter = 1;
      _gameManager.player.ExchangeControlHandler(0, new GoodHealthPlayerControlHandler(_gameManager.player));
      Logger.Info("Added basic power up. " + this.ToString());
    }
    else
    {
      if (_powerMeter != 1)
      {
        // when getting a non basic power up, we want to set the power meter to 1 so we automatically go back to that state in case we lose the
        // power up.
        _powerMeter = 1;
        _gameManager.player.ExchangeControlHandler(0, new GoodHealthPlayerControlHandler(_gameManager.player));
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

          case PowerUpType.SpinMeleeAttack:
            _currentPowerUpControlHandler = new PowerUpSpinMeleeAttackControlHandler(_gameManager.player);
            break;
          case PowerUpType.JetPack:
            _currentPowerUpControlHandler = new PowerUpJetPackControlHandler(_gameManager.player, 30f, _gameManager.gameSettings.powerUpSettings);
            break;
          case PowerUpType.Gun:
            _currentPowerUpControlHandler = new PowerUpGunControlHandler(_gameManager.player, -1f, _gameManager.gameSettings.powerUpSettings);
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
