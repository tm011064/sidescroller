﻿using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The object pool is a list of already instantiated game objects of the same type.
/// </summary>
public class ObjectPool
{
  //the list of objects.
  private List<GameObject> _pooledObjects;

  //sample of the actual object to store.
  //used if we need to grow the list.
  private GameObject _pooledObj;

  //maximum number of objects to have in the list.
  private int _maxPoolSize;

  //initial and default number of objects to have in the list.
  private int _initialPoolSize;

  /// <summary>
  /// Constructor for creating a new Object Pool.
  /// </summary>
  /// <param name="obj">Game Object for this pool</param>
  /// <param name="initialPoolSize">Initial and default size of the pool.</param>
  /// <param name="maxPoolSize">Maximum number of objects this pool can contain.</param>
  /// <param name="shouldShrink">Should this pool shrink back to the initial size.</param>
  public ObjectPool(GameObject obj, int initialPoolSize, int maxPoolSize)
  {
    //instantiate a new list of game objects to store our pooled objects in.
    _pooledObjects = new List<GameObject>();

    //create and add an object based on initial size.
    for (int i = 0; i < initialPoolSize; i++)
    {
      //instantiate and create a game object with useless attributes.
      //these should be reset anyways.
      GameObject nObj = GameObject.Instantiate(obj, Vector3.zero, Quaternion.identity) as GameObject;

      //make sure the object isn't active.
      nObj.SetActive(false);

      //add the object too our list.
      _pooledObjects.Add(nObj);

      //Don't destroy on load, so
      //we can manage centrally.
      GameObject.DontDestroyOnLoad(nObj);
    }

    //store our other variables that are useful.
    this._maxPoolSize = maxPoolSize;
    this._pooledObj = obj;
    this._initialPoolSize = initialPoolSize;
  }

  /// <summary>
  /// Returns an active object from the object pool without resetting any of its values.
  /// You will need to set its values and set it inactive again when you are done with it.
  /// </summary>
  /// <returns>Game Object of requested type if it is available, otherwise null.</returns>
  public GameObject GetObject()
  {
    return this.GetObject(null);
  }
  /// <summary>
  /// Returns an active object from the object pool without resetting any of its values.
  /// You will need to set its values and set it inactive again when you are done with it.
  /// </summary>
  /// <returns>Game Object of requested type if it is available, otherwise null.</returns>
  public GameObject GetObject(Vector3? position)
  {
    //iterate through all pooled objects.
    for (int i = 0; i < _pooledObjects.Count; i++)
    {
      //look for the first one that is inactive.
      if (_pooledObjects[i].activeSelf == false)
      {
        if (position.HasValue)
          _pooledObjects[i].transform.position = position.Value;

        //set the object to active.
        _pooledObjects[i].SetActive(true);
        //return the object we found.
        return _pooledObjects[i];
      }
    }

    //if we make it this far, we obviously didn't find an inactive object.
    //so we need to see if we can grow beyond our current count.
    if (this._maxPoolSize > this._pooledObjects.Count)
    {
      //Instantiate a new object.
      GameObject nObj = GameObject.Instantiate(_pooledObj, position.HasValue ? position.Value : Vector3.zero, Quaternion.identity) as GameObject;
      //set it to active since we are about to use it.
      nObj.SetActive(true);
      //add it to the pool of objects
      _pooledObjects.Add(nObj);
      //return the object to the requestor.
      return nObj;
    }

    //if we made it this far obviously we didn't have any inactive objects
    //we also were unable to grow, so return null as we can't return an object.
    return null;
  }

  public IEnumerable<GameObject> IterateOverGameObjects()
  {
    for (int i = 0; i < _pooledObjects.Count; i++)
    {
      yield return _pooledObjects[i];
    }
  }

  /// <summary>
  /// Iterate through the pool and releases as many objects as
  /// possible until the pool size is back to the initial default size.
  /// </summary>
  /// <param name="sender">Who initiated this event?</param>
  /// <param name="eventArgs">The arguments for this event.</param>
  public void Shrink()
  {
    //how many objects are we trying to remove here?
    int objectsToRemoveCount = _pooledObjects.Count - _initialPoolSize;
    //Are there any objects we need to remove?
    if (objectsToRemoveCount <= 0)
    {
      //cool lets get out of here.
      return;
    }

    //iterate through our list and remove some objects
    //we do reverse iteration so as we remove objects from
    //the list the shifting of objects does not affect our index
    //Also notice the offset of 1 to account for zero indexing
    //and i >= 0 to ensure we reach the first object in the list.
    for (int i = _pooledObjects.Count - 1; i >= 0; i--)
    {
      //Is this object active?
      if (!_pooledObjects[i].activeSelf)
      {
        //Guess not, lets grab it.
        GameObject obj = _pooledObjects[i];
        //and kill it from the list.
        _pooledObjects.Remove(obj);
      }
    }
  }
}
