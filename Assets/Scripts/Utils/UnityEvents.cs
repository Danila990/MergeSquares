using System;
using UnityEngine;
using UnityEngine.Events;

namespace Utils
{
    [Serializable]
    public class UnityEventBool : UnityEvent<bool>
    {
    }

    [Serializable]
    public class UnityEventInt : UnityEvent<int>
    {
    }

    [Serializable]
    public class UnityEventFloat : UnityEvent<float>
    {
    }

    [Serializable]
    public class UnityEventString : UnityEvent<string>
    {
    }

    [Serializable]
    public class UnityEventObjectsArray : UnityEvent<object[]>
    {
    }

    [Serializable]
    public class UnityEventGameObject : UnityEvent<GameObject>
    {
    }

    [Serializable]
    public class UnityEventTransform : UnityEvent<Transform>
    {
    }

    [Serializable]
    public class UnityEventVector3 : UnityEvent<Vector3>
    {
    }

    [Serializable]
    public class UnityEventVector2 : UnityEvent<Vector2>
    {
    }

    [Serializable]
    public class UnityEventSprite : UnityEvent<Sprite>
    {
    }
}