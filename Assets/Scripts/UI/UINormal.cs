using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
public class UINormal : WindowComponent
{
//BINDING_DEFINITION_BEGIN	private Button mButtonClose;
	private Button mButtonMain;
	private Button mButtonPop;
	private Button mButtonWidget;
//BINDING_DEFINITION_END

    private void Awake()
    {
        //BINDING_CODE_BEGIN		mButtonClose = GetComponent<Button>("Tween/SafeArea/@Button.mButtonClose");
		mButtonMain = GetComponent<Button>("Tween/SafeArea/@Button.mButtonMain");
		mButtonPop = GetComponent<Button>("Tween/SafeArea/@Button.mButtonPop");
		mButtonWidget = GetComponent<Button>("Tween/SafeArea/@Button.mButtonWidget");
//BINDING_CODE_END

        mButtonPop.onClick.AddListener(() => WindowManager.Instance.Open("UIPop"));
        mButtonWidget.onClick.AddListener(() => WindowManager.Instance.Open("UISerialized"));
        mButtonMain.onClick.AddListener(() => WindowManager.Instance.CloseAllAndOpen("UIMain"));
    } 
}