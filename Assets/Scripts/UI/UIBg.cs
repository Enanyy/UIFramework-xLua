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
        mButtonClose.onClick.AddListener(() => WindowManager.Instance.Close(context.type == WindowType.Widget? (context as WidgetContext).parent:context));

    }
}