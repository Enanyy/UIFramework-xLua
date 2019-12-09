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
        hideOther = true;
        fixedWidgets = new List<System.Type> { typeof(UIFixed) };
    }
    public override void OnLoad(GameObject go)
    {
        base.OnLoad(go);

        //BINDING_CODE_BEGIN		mButtonClose = GetComponent<Button>("Tween/SafeArea/@Button.mButtonClose");
		mButtonMain = GetComponent<Button>("Tween/SafeArea/@Button.mButtonMain");
		mButtonPop = GetComponent<Button>("Tween/SafeArea/@Button.mButtonPop");
		mButtonWidget = GetComponent<Button>("Tween/SafeArea/@Button.mButtonWidget");
//BINDING_CODE_END

        mButtonClose.onClick.AddListener(Close);
        mButtonPop.onClick.AddListener(() => WindowManager.Instance.Open<UIPop>());
        mButtonWidget.onClick.AddListener(() => WindowManager.Instance.Open<UIWidget>());
        mButtonMain.onClick.AddListener(() => WindowManager.Instance.CloseAllAndOpen<UIMain>());
    } 
}