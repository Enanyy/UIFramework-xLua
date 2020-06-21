using UnityEngine;
using UnityEngine.UI;

public class UIPop : WindowComponent
{
//BINDING_DEFINITION_BEGIN	private Button mButtonClose;
	private Button mbuttonMain;
	private Button mButtonNormal;
	private Button mButtonWidget;
	private Text mText;
//BINDING_DEFINITION_END

    private void Awake()
    {

        //BINDING_CODE_BEGIN		mButtonClose = GetComponent<Button>("Tween/Widget/@Button.mButtonClose");
		mbuttonMain = GetComponent<Button>("Tween/Widget/@Button.mbuttonMain");
		mButtonNormal = GetComponent<Button>("Tween/Widget/@Button.mButtonNormal");
		mButtonWidget = GetComponent<Button>("Tween/Widget/@Button.mButtonWidget");
		mText = GetComponent<Text>("Tween/Widget/@Text.mText");
//BINDING_CODE_END
        mButtonClose.onClick.AddListener(()=>WindowManager.Instance.Close(UIDefine.UIPop));
        mButtonNormal.onClick.AddListener(() => WindowManager.Instance.Open(UIDefine.UINormal));
        mButtonWidget.onClick.AddListener(() => WindowManager.Instance.Open(UIDefine.UIWidget));
        mbuttonMain.onClick.AddListener(() => WindowManager.Instance.CloseAllAndOpen(UIDefine.UIMain));
    }
}