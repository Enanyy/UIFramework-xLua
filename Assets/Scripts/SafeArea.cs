using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[DisallowMultipleComponent]
public class SafeArea : MonoBehaviour
{

    private RectTransform mTarget;

#if UNITY_EDITOR
    [SerializeField]
    private bool Simulate = false;
#endif

    void Awake()
    {
        mTarget = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    void ApplySafeArea()
    {
        var area = GetSafeAreaRect();

#if UNITY_EDITOR

        /*
        iPhone X 横持手机方向:
        iPhone X 分辨率
        2436 x 1125 px

        safe area
        2172 x 1062 px

        左右边距分别
        132px

        底边距 (有Home条)
        63px

        顶边距
        0px
        */

        float width = 2436f;
        float height = 1125f;
        float margin = 132f;
        
        if ((Screen.width == (int)width && Screen.height == (int)height))
        {
            Simulate = true;
        }

        if (Simulate)
        {
            var insets = area.width * margin / width;
            var positionOffset = new Vector2(insets, 0);
            var sizeOffset = new Vector2(insets * 2, 0);
            area.position = area.position + positionOffset;
            area.size = area.size - sizeOffset;
        }
#endif

        var anchorMin = area.position;
        var anchorMax = area.position + area.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;
        mTarget.anchorMin = anchorMin;
        mTarget.anchorMax = anchorMax;
    }

#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern void GetSafeArea(out float x, out float y, out float w, out float h);
#endif


    /// <summary>
    /// 获取iPhone X 等苹果未来的异性屏幕的安全区域Safe are
    /// </summary>
    /// <param name="showInsetsBottom"></param>
    /// <returns></returns>
    public static Rect GetSafeAreaRect()
    {
        float x, y, w, h;
#if UNITY_IOS && !UNITY_EDITOR
        GetSafeArea(out x, out y, out w, out h);
#else
        x = 0;
        y = 0;
        w = Screen.width;
        h = Screen.height;
#endif
        return new Rect(x, y, w, h);
    }
}
