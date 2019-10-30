using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScrollIndexCallback3 : MonoBehaviour
{
    public Text text;
    void OnScrollItem(int idx)
    {
        string name = "Item " + idx.ToString();
        if (text != null)
        {
            text.text = name;
        }
    }
}
