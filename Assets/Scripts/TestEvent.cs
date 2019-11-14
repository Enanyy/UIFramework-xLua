using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
public class TestEvent : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        AddListener(1);
        AddListener(2);
        Invoke(1);
        Invoke(3);
        RemoveListener();
        Invoke(1);
    }
    void AddListener(int id)
    {
        EventCenter.AddListener(id, OnListen);
        EventCenter.AddListener<int>(id, OnListen);
        EventCenter.AddListener<float>(id, OnListen);
        EventCenter.AddListener<Param>(id, OnListen);
        EventCenter.AddListener<int, int>(id, OnListen);
        EventCenter.AddListener<int, float>(id, OnListen);
        EventCenter.AddListener<float, int>(id, OnListen);
    }

    void RemoveListener()
    {
        EventCenter.RemoveListener(1, OnListen);
        EventCenter.RemoveListener<int>(1, OnListen);
        EventCenter.RemoveListener<float>(1, OnListen);
        EventCenter.RemoveListener<Param>(1, OnListen);
        EventCenter.RemoveListener<int, int>(1, OnListen);
        EventCenter.RemoveListener<int, float>(1, OnListen);
        EventCenter.RemoveListener<float, int>(1, OnListen);
    }

    void Invoke(int id)
    {
        EventCenter.Invoke(id);
        EventCenter.Invoke(id, 1);
        EventCenter.Invoke(id, 1.2f);
        EventCenter.Invoke(id, new Param { i = 1, j = 2 });
        EventCenter.Invoke(id, 1, 2);
        EventCenter.Invoke(id, 2, 3.0f);
        EventCenter.Invoke(id, 9f, 1);   
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
