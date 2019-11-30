using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
public class UINormal : Window
{
//BINDING_DEFINITION_BEGIN	private Button mButtonClose;
	private Button mButtonMain;
	private Button mButtonPop;
	private Button mButtonWidget;
//BINDING_DEFINITION_END
    public UINormal()
    {
        hidePrevious = true;
        fixedWidgets = new List<System.Type> { typeof(UIFixed) };
    }
    private void Awake()
    {
//BINDING_CODE_BEGIN		transform.Find("Tween/SafeArea/@Button.mButtonClose").TryGetComponent(out mButtonClose);
		transform.Find("Tween/SafeArea/@Button.mButtonMain").TryGetComponent(out mButtonMain);
		transform.Find("Tween/SafeArea/@Button.mButtonPop").TryGetComponent(out mButtonPop);
		transform.Find("Tween/SafeArea/@Button.mButtonWidget").TryGetComponent(out mButtonWidget);
//BINDING_CODE_END

        mButtonClose.onClick.AddListener(Close);
        mButtonPop.onClick.AddListener(() => WindowManager.Instance.Open<UIPop>());
        mButtonWidget.onClick.AddListener(() => WindowManager.Instance.Open<UIWidget>());
        mButtonMain.onClick.AddListener(() => WindowManager.Instance.CloseAllAndOpen<UIMain>());
    } 
}