# Pooling-Manager
Pooling Manager for Unity. This manager allows for all your pools to be contained in one easy to access class. 

It also includes features such as: 
* Error logging 
* State tracking
* Persistence through level loads
* Dynamic pool growth
* The ability to auto parent or manualy set the parent of the pooled object at creation

## Setup
* Add **PooledObjectManager** and **PoolDataSender** to a gameObject in scene. In the PoolDataSender inspector, set up the pools you want to create. 

* Pooled Objects must have a script on them that derives from the PooledObject class. This allows for error logging, tracking the objects state, and some initialization.

## Use
The PoolManager class by default is a Singleton, so to access the methods to get objects from their pools you will need to refrence it's instance ie. ```Poolmanager.Instance.TryGetObjectFromPool(prefab, out T component);```

There are a few ways to get and use objects from their pools:
* ```TryGetObjectFromPool(prefab, out T component);```
  * Prefab is the object type you want to use from the pool.
  * T is the component type you are trying to get from the pooled prefab.
* ```UseObjectFromPool<T>(prefab);```
  * Where T is the component type you want to get from the prefab.
  * Prefab is the object type you want to use from the pool.
* ```UseObjectFromPool(prefab, position, rotation);```
  * This will return the gameObject from the pool of type prefab.
  * Will set the position and rotation after getting it from the pool
  * Will also set it to active after getting it and setting its position.
  
  When done with the pooled object set it to not active ie. ```gameObject.SetActive(false);``` and it will automaticaly be returned to the pool.
