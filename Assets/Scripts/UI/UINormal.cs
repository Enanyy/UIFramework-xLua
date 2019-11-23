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
//BINDING_CODE_BEGIN		mButtonClose = transform.Find("SafeArea/@Button.mButtonClose").GetComponent<Button>();
		mButtonMain = transform.Find("SafeArea/@Button.mButtonMain").GetComponent<Button>();
		mButtonPop = transform.Find("SafeArea/@Button.mButtonPop").GetComponent<Button>();
		mButtonWidget = transform.Find("SafeArea/@Button.mButtonWidget").GetComponent<Button>();
        //BINDING_CODE_END

        mButtonClose.onClick.AddListener(Close);
        mButtonPop.onClick.AddListener(() => WindowManager.Instance.Open<UIPop>());
        mButtonWidget.onClick.AddListener(() => WindowManager.Instance.Open<UIWidget>());
        mButtonMain.onClick.AddListener(() => WindowManager.Instance.CloseAllAndOpen<UIMain>());
    } 
}