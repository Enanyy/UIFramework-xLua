using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System;

public class EventCenter
{
    #region Listener
    interface IEventListener
    {
        int id { get; }
    }
    class EventListener : UnityEvent, IEventListener
    {
        public int id { get; private set; }
        public EventListener(int id) { this.id = id; }
    }
    class EventListener<T0> : UnityEvent<T0>, IEventListener
    {
        public int id { get; private set; }
        public EventListener(int id) { this.id = id; }
    }
    class EventListener<T0, T1> : UnityEvent<T0, T1>, IEventListener
    {
        public int id { get; private set; }
        public EventListener(int id) { this.id = id; }
    }
    class EventListener<T0, T1, T2> : UnityEvent<T0, T1, T2>, IEventListener
    {
        public int id { get; private set; }
        public EventListener(int id) { this.id = id; }
    }
    class EventListener<T0, T1, T2, T3> : UnityEvent<T0, T1, T2, T3>, IEventListener
    {
        public int id { get; private set; }
        public EventListener(int id) { this.id = id; }
    }
    #endregion

    private static EventCenter mInstance;
    private static EventCenter Instance
    {
        get
        {
            if(mInstance == null)
            {
                mInstance = new EventCenter();
            }
            return mInstance;
        }
    }
    private EventCenter() { }


    private  Dictionary<int, Dictionary<Type, IEventListener>> mListeners = new Dictionary<int, Dictionary<Type, IEventListener>>();

    private static Dictionary<int, Dictionary<Type, IEventListener>> listeners { get { return Instance.mListeners; } }

    public static void Register(int id, UnityAction call)
    {
        if(call == null)
        { 
            return;
        }
        if (listeners.TryGetValue(id, out Dictionary<Type, IEventListener> dic) == false)
        {
            dic = new Dictionary<Type, IEventListener>();
            listeners.Add(id, dic);
        }
        Type type = typeof(EventListener);
        if (dic.TryGetValue(type, out IEventListener o) == false)
        {
            o = new EventListener(id);
            dic.Add(type, o);
        }
        EventListener listener = o as EventListener;
        listener.RemoveListener(call);
        listener.AddListener(call);
    }
    public static void Register<T0>(int id, UnityAction<T0> call)
    {
        if (call == null)
        {
            return;
        }
        if (listeners.TryGetValue(id, out Dictionary<Type, IEventListener> dic) == false)
        {
            dic = new Dictionary<Type, IEventListener>();
            listeners.Add(id, dic);
        }
        Type type = typeof(EventListener<T0>);
        if (dic.TryGetValue(type, out IEventListener o) == false)
        {
            o = new EventListener<T0>(id);
            dic.Add(type, o);
        }
        EventListener<T0> listener = o as EventListener<T0>;
        listener.RemoveListener(call);
        listener.AddListener(call);
    }
    public static void Register<T0, T1>(int id, UnityAction<T0, T1> call)
    {
        if (call == null)
        {
            return;
        }
        if (listeners.TryGetValue(id, out Dictionary<Type, IEventListener> dic) == false)
        {
            dic = new Dictionary<Type, IEventListener>();
            listeners.Add(id, dic);
        }
        Type type = typeof(EventListener<T0, T1>);
        if (dic.TryGetValue(type, out IEventListener o) == false)
        {
            o = new EventListener<T0, T1>(id);
            dic.Add(type, o);
        }
        EventListener<T0, T1> listener = o as EventListener<T0, T1>;
        listener.RemoveListener(call);
        listener.AddListener(call);
    }
    public static void Register<T0, T1, T2>(int id, UnityAction<T0, T1, T2> call)
    {
        if (call == null)
        {
            return;
        }
        if (listeners.TryGetValue(id, out Dictionary<Type, IEventListener> dic) == false)
        {
            dic = new Dictionary<Type, IEventListener>();
            listeners.Add(id, dic);
        }
        Type type = typeof(EventListener<T0, T1, T2>);
        if (dic.TryGetValue(type, out IEventListener o) == false)
        {
            o = new EventListener<T0, T1, T2>(id);
            dic.Add(type, o);
        }
        EventListener<T0, T1, T2> listener = o as EventListener<T0, T1, T2>;
        listener.RemoveListener(call);
        listener.AddListener(call);
    }
    public static void Register<T0, T1, T2, T3>(int id, UnityAction<T0, T1, T2, T3> call)
    {
        if (call == null)
        {
            return;
        }
        if (listeners.TryGetValue(id, out Dictionary<Type, IEventListener> dic) == false)
        {
            dic = new Dictionary<Type, IEventListener>();
            listeners.Add(id, dic);
        }
        Type type = typeof(EventListener<T0, T1, T2, T3>);
        if (dic.TryGetValue(type, out IEventListener o) == false)
        {
            o = new EventListener<T0, T1, T2, T3>(id);
            dic.Add(type, o);
        }
        EventListener<T0, T1, T2, T3> listener = o as EventListener<T0, T1, T2, T3>;
        listener.RemoveListener(call);
        listener.AddListener(call);
    }
    public static void UnRegister(int id, UnityAction call)
    {
        if (call == null)
        {
            return;
        }
        if (listeners.TryGetValue(id, out Dictionary<Type, IEventListener> dic))
        {
            Type type = typeof(EventListener);
            if (dic.TryGetValue(type, out IEventListener o))
            {
                EventListener listener = o as EventListener;
                listener.RemoveListener(call);
            }
        }
    }
    public static void UnRegister<T0>(int id, UnityAction<T0> call)
    {
        if (call == null)
        {
            return;
        }
        if (listeners.TryGetValue(id, out Dictionary<Type, IEventListener> dic))
        {
            Type type = typeof(EventListener<T0>);
            if (dic.TryGetValue(type, out IEventListener o))
            {
                EventListener<T0> listener = o as EventListener<T0>;
                listener.RemoveListener(call);
            }
        }
    }
    public static void UnRegister<T0, T1>(int id, UnityAction<T0, T1> call)
    {
        if (call == null)
        {
            return;
        }
        if (listeners.TryGetValue(id, out Dictionary<Type, IEventListener> dic))
        {
            Type type = typeof(EventListener<T0, T1>);
            if (dic.TryGetValue(type, out IEventListener o))
            {
                EventListener<T0, T1> listener = o as EventListener<T0, T1>;
                listener.RemoveListener(call);
            }
        }
    }
    public static void UnRegister<T0, T1, T2>(int id, UnityAction<T0, T1, T2> call)
    {
        if (call == null)
        {
            return;
        }
        if (listeners.TryGetValue(id, out Dictionary<Type, IEventListener> dic))
        {
            Type type = typeof(EventListener<T0, T1, T2>);
            if (dic.TryGetValue(type, out IEventListener o))
            {
                EventListener<T0, T1, T2> listener = o as EventListener<T0, T1, T2>;
                listener.RemoveListener(call);
            }
        }
    }
    public static void UnRegister<T0, T1, T2, T3>(int id, UnityAction<T0, T1, T2, T3> call)
    {
        if (call == null)
        {
            return;
        }
        if (listeners.TryGetValue(id, out Dictionary<Type, IEventListener> dic))
        {
            Type type = typeof(EventListener<T0, T1, T2, T3>);
            if (dic.TryGetValue(type, out IEventListener o))
            {
                EventListener<T0, T1, T2, T3> listener = o as EventListener<T0, T1, T2, T3>;
                listener.RemoveListener(call);
            }
        }
    }

    public static void Dispatch(int id)
    {
        if (listeners.TryGetValue(id, out Dictionary<Type, IEventListener> dic))
        {
            Type type = typeof(EventListener);
            if (dic.TryGetValue(type, out IEventListener o))
            {
                EventListener listener = o as EventListener;
                listener.Invoke();
            }
        }
    }
    public static void Dispatch<T0>(int id, T0 arg0)
    {
        if (listeners.TryGetValue(id, out Dictionary<Type, IEventListener> dic))
        {
            Type type = typeof(EventListener<T0>);
            if (dic.TryGetValue(type, out IEventListener o))
            {
                EventListener<T0> listener = o as EventListener<T0>;
                listener.Invoke(arg0);
            }
        }
    }
    public static void Dispatch<T0, T1>(int id, T0 arg0, T1 arg1)
    {
        if (listeners.TryGetValue(id, out Dictionary<Type, IEventListener> dic))
        {
            Type type = typeof(EventListener<T0, T1>);
            if (dic.TryGetValue(type, out IEventListener o))
            {
                EventListener<T0, T1> listener = o as EventListener<T0, T1>;
                listener.Invoke(arg0, arg1);
            }
        }
    }
    public static void Dispatch<T0, T1, T2>(int id, T0 arg0, T1 arg1, T2 arg2)
    {
        if (listeners.TryGetValue(id, out Dictionary<Type, IEventListener> dic))
        {
            Type type = typeof(EventListener<T0, T1, T2>);
            if (dic.TryGetValue(type, out IEventListener o))
            {
                EventListener<T0, T1, T2> listener = o as EventListener<T0, T1, T2>;
                listener.Invoke(arg0, arg1, arg2);
            }
        }
    }
    public static void Dispatch<T0, T1, T2, T3>(int id, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
    {
        if (listeners.TryGetValue(id, out Dictionary<Type, IEventListener> dic))
        {
            Type type = typeof(EventListener<T0, T1, T2, T3>);
            if (dic.TryGetValue(type, out IEventListener o))
            {
                EventListener<T0, T1, T2, T3> listener = o as EventListener<T0, T1, T2, T3>;
                listener.Invoke(arg0, arg1, arg2, arg3);
            }
        }
    }

    public static void Clear(int id)
    {
        if (listeners.TryGetValue(id, out Dictionary<Type, IEventListener> dic))
        {
            dic.Clear();
        }
        listeners.Remove(id);
    }
    public static void Clear()
    {
        listeners.Clear();
    }
}
