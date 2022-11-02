using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.PoolManager;

public class PooledObjectComponentExample : PooledObject
{
    // This is an example class that would be attached to a pooled object.
    // only one class on the pooled object needs to derive from PooledObject.
    // this class doesn't need to do anything besides being attached to the pooled object for error logging, tracking, and some initialization.

    // You can just add this class or one like it to the pooled object if you don't want your classes derived from PooledObject.
}
