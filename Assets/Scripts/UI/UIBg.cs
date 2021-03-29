using UnityEngine;
using UnityEngine.UI;

public class UIBg : WindowComponent
{
//BINDING_DEFINITION_BEGIN
	private Button mButtonClose;
//BINDING_DEFINITION_END
   
    private void Awake()
    {
//BINDING_CODE_BEGIN
		mButtonClose = GetComponent<Button>("Tween/SafeArea/@Button.mButtonClose");
//BINDING_CODE_END
        mButtonClose.onClick.AddListener(() => WindowManager.Instance.Close(contextbase.type == WindowType.Widget? (contextbase as WidgetContext).parent:contextbase));

    }

    void OnEnable()
    {
        RectTransform rect = transform as RectTransform;

        Debug.Log(rect.anchorMin);
        Debug.Log(rect.anchorMax);
        Debug.Log(rect.offsetMin);
        Debug.Log(rect.offsetMax);
    }
}