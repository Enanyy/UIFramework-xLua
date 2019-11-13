using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

interface IEventListener
{
    int id { get; }
}
public class EventListener:UnityEvent, IEventListener
{
    public int id { get; private set; }
    public EventListener(int id) { this.id = id; }
}
public class EventListener<T0>:UnityEvent<T0>,IEventListener
{
    public int id { get; private set; }
    public EventListener(int id) { this.id = id; }
}
public class EventListener<T0, T1> : UnityEvent<T0, T1>, IEventListener
{
    public int id { get; private set; }
    public EventListener(int id) { this.id = id; }
}
public class EventListener<T0, T1, T2> : UnityEvent<T0, T1, T2>, IEventListener
{
    public int id { get; private set; }
    public EventListener(int id) { this.id = id; }
}
public class EventListener<T0, T1, T2, T3> : UnityEvent<T0, T1, T2, T3>, IEventListener
{
    public int id { get; private set; }
    public EventListener(int id) { this.id = id; }
}


public class EventDispatch
{
    private static EventDispatch mInstance;
    public static EventDispatch Instance
    {
        get
        {
            if (mInstance == null)
            {
                mInstance = new EventDispatch();
            }
            return mInstance;
        }
    }

    private Dictionary<int, IEventListener> mListeners = new Dictionary<int, IEventListener>();

    public void Register(int id,UnityAction call)
    {
        if(mListeners.TryGetValue(id, out IEventListener o) ==false)
        {
            o = new EventListener(id);
            mListeners.Add(id, o);
        }
        EventListener listener = o as EventListener;
        listener.AddListener(call);
    }
    public void Register<T0>(int id, UnityAction<T0> call)
    {
        if (mListeners.TryGetValue(id, out IEventListener o)==false)
        {
            o = new EventListener<T0>(id);
            mListeners.Add(id, o);
        }
        EventListener<T0> listener = o as EventListener<T0>;
        listener.AddListener(call);
    }
    public void Register<T0,T1>(int id, UnityAction<T0,T1> call)
    {
        if (mListeners.TryGetValue(id, out IEventListener o) ==false)
        {
            o = new EventListener<T0,T1>(id);
            mListeners.Add(id, o);
        }
        EventListener<T0,T1> listener = o as EventListener<T0,T1>;
        listener.AddListener(call);
    }
    public void Register<T0, T1,T2>(int id, UnityAction<T0, T1,T2> call)
    {
        if (mListeners.TryGetValue(id, out IEventListener o) ==false)
        {
            o = new EventListener<T0, T1,T2>(id);
            mListeners.Add(id, o);
        }
        EventListener<T0, T1,T2> listener = o as EventListener<T0, T1,T2>;
        listener.AddListener(call);
    }
    public void Register<T0, T1, T2,T3>(int id, UnityAction<T0, T1, T2,T3> call)
    {
        if (mListeners.TryGetValue(id, out IEventListener o) == false)
        {
            o = new EventListener<T0, T1, T2,T3>(id);
            mListeners.Add(id, o);
        }
        EventListener<T0, T1, T2,T3> listener = o as EventListener<T0, T1, T2,T3>;
        listener.AddListener(call);
    }
    public void UnRegister(int id, UnityAction call)
    {
        if (mListeners.TryGetValue(id, out IEventListener o))
        {
            EventListener listener = o as EventListener;
            listener.RemoveListener(call);
        }
      
    }
    public void UnRegister<T0>(int id, UnityAction<T0> call)
    {
        if (mListeners.TryGetValue(id, out IEventListener o))
        {
            EventListener<T0> listener = o as EventListener<T0>;
            listener.RemoveListener(call);
        }
    }
    public void UnRegister<T0, T1>(int id, UnityAction<T0, T1> call)
    {
        if (mListeners.TryGetValue(id, out IEventListener o) == false)
        {
            EventListener<T0, T1> listener = o as EventListener<T0, T1>;
            listener.RemoveListener(call);
        }
        
    }
    public void UnRegister<T0, T1, T2>(int id, UnityAction<T0, T1, T2> call)
    {
        if (mListeners.TryGetValue(id, out IEventListener o) ==false)
        {
            EventListener<T0, T1, T2> listener = o as EventListener<T0, T1, T2>;
            listener.RemoveListener(call);
        }
    }
    public void UnRegister<T0, T1, T2, T3>(int id, UnityAction<T0, T1, T2, T3> call)
    {
        if (mListeners.TryGetValue(id, out IEventListener o) ==false)
        {
            EventListener<T0, T1, T2, T3> listener = o as EventListener<T0, T1, T2, T3>;
            listener.RemoveListener(call);
        }     
    }
    
    public void Dispatch(int id)
    {
        if (mListeners.TryGetValue(id, out IEventListener o))
        {
            EventListener listener = o as EventListener;
            listener.Invoke();
        }
    }
    public void Dispatch<T0>(int id, T0 arg0)
    {
        if (mListeners.TryGetValue(id, out IEventListener o))
        {
            EventListener<T0> listener = o as EventListener<T0>;
            listener.Invoke(arg0);
        }
    }
    public void Dispatch<T0,T1>(int id, T0 arg0, T1 arg1)
    {
        if (mListeners.TryGetValue(id, out IEventListener o))
        {
            EventListener<T0,T1> listener = o as EventListener<T0,T1>;
            listener.Invoke(arg0, arg1);
        }
    }
    public void Dispatch<T0, T1, T2>(int id, T0 arg0, T1 arg1, T2 arg2)
    {
        if (mListeners.TryGetValue(id, out IEventListener o))
        {
            EventListener<T0, T1, T2> listener = o as EventListener<T0, T1, T2>;
            listener.Invoke(arg0, arg1, arg2);
        }
    }
    public void Dispatch<T0, T1, T2, T3>(int id, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
    {
        if (mListeners.TryGetValue(id, out IEventListener o))
        {
            EventListener<T0, T1, T2, T3> listener = o as EventListener<T0, T1, T2, T3>;
            listener.Invoke(arg0, arg1, arg2, arg3);
        }
    }
}
