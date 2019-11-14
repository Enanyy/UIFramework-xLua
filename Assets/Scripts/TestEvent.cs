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
        RemoveListener(1);
        Invoke(1);
        Invoke(2);
        RemoveListener(1);
    }
    void AddListener(int id)
    {
        EventSystem.AddListener(id, OnListen);
        EventSystem.AddListener<int>(id, OnListen);
        EventSystem.AddListener<float>(id, OnListen);
        EventSystem.AddListener<Param>(id, OnListen);
        EventSystem.AddListener<int, int>(id, OnListen);
        EventSystem.AddListener<int, float>(id, OnListen);
        EventSystem.AddListener<float, int>(id, OnListen);
    }

    void RemoveListener(int id)
    {
        EventSystem.RemoveListener(id, OnListen);
        EventSystem.RemoveListener<int>(id, OnListen);
        EventSystem.RemoveListener<float>(id, OnListen);
        EventSystem.RemoveListener<Param>(id, OnListen);
        EventSystem.RemoveListener<int, int>(id, OnListen);
        EventSystem.RemoveListener<int, float>(id, OnListen);
        EventSystem.RemoveListener<float, int>(id, OnListen);
    }

    void Invoke(int id)
    {
        EventSystem.Invoke(id);
        EventSystem.Invoke(id, 1);
        EventSystem.Invoke(id, 1.2f);
        EventSystem.Invoke(id, new Param { i = 1, j = 2 });
        EventSystem.Invoke(id, 1, 2);
        EventSystem.Invoke(id, 2, 3.0f);
        EventSystem.Invoke(id, 9f, 1);   
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
