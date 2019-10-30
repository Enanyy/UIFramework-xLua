using UnityEngine;
using System.Collections;
using UnityEngine.UI;


[RequireComponent(typeof(UnityEngine.UI.ScrollView))]
[DisallowMultipleComponent]
public class InitOnStart : MonoBehaviour
{
    public int totalCount = -1;
    void Start()
    {
        var ls = GetComponent<ScrollView>();
        ls.totalCount = totalCount;
        ls.Refill();
    }
}
    
