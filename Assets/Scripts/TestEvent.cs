using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
public class TestEvent : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        Register(1);
        Register(2);
        Dispatch(1);
        Dispatch(3);
        UnRegister();
        Dispatch(1);
    }
    void Register(int id)
    {
        EventCenter.Register(id, OnListen);
        EventCenter.Register<int>(id, OnListen);
        EventCenter.Register<float>(id, OnListen);
        EventCenter.Register<Param>(id, OnListen);
        EventCenter.Register<int, int>(id, OnListen);
        EventCenter.Register<int, float>(id, OnListen);
        EventCenter.Register<float, int>(id, OnListen);
    }

    void UnRegister()
    {
        EventCenter.UnRegister(1, OnListen);
        EventCenter.UnRegister<int>(1, OnListen);
        EventCenter.UnRegister<float>(1, OnListen);
        EventCenter.UnRegister<Param>(1, OnListen);
        EventCenter.UnRegister<int, int>(1, OnListen);
        EventCenter.UnRegister<int, float>(1, OnListen);
        EventCenter.UnRegister<float, int>(1, OnListen);
    }

    void Dispatch(int id)
    {
        EventCenter.Dispatch(id);
        EventCenter.Dispatch(id, 1);
        EventCenter.Dispatch(id, 1.2f);
        EventCenter.Dispatch(id, new Param { i = 1, j = 2 });
        EventCenter.Dispatch(id, 1, 2);
        EventCenter.Dispatch(id, 2, 3.0f);
        EventCenter.Dispatch(id, 9f, 1);   
    }

    // Update is called once per frame
    void Update()
    {

    }


    private void OnListen()
    {
        Debug.Log("No param!");
    }
    private void OnListen(int i)
    {
        Debug.Log("param int:" + i);
    }
    private void OnListen(float i)
    {
        Debug.Log("param float:" + i);
    }
    public class Param
    {
        public int i;
        public float j;
    }
    private void OnListen(Param i)
    {
        Debug.Log("param param:" + i.i + "," + i.j);
    }
    private void OnListen(int i, int j)
    {
        Debug.Log("param int i:" + i + " int j:" + j);
    }
    private void OnListen(int i, float j)
    {
        Debug.Log("param int i:" + i + " float j:" + j);
    }
    private void OnListen(float i, int j)
    {
        Debug.Log("param float i:" + i + " int j:" + j);
    }
}
