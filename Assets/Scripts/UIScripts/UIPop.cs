using UnityEngine;
using UnityEngine.UI;

public class UIPop : Window
{
//BINDING_DEFINITION_BEGIN
	private Button mButtonClose;
	private Button mbuttonMain;
	private Button mButtonNormal;
	private Button mButtonWidget;
	private Text mText;
//BINDING_DEFINITION_END

    private void Awake()
    {
//BINDING_CODE_BEGIN
		mButtonClose = transform.Find("Widget/@Button.mButtonClose").GetComponent<Button>();
		mbuttonMain = transform.Find("Widget/@Button.mbuttonMain").GetComponent<Button>();
		mButtonNormal = transform.Find("Widget/@Button.mButtonNormal").GetComponent<Button>();
		mButtonWidget = transform.Find("Widget/@Button.mButtonWidget").GetComponent<Button>();
		mText = transform.Find("Widget/@Text.mText").GetComponent<Text>();
        //BINDING_CODE_END
        mButtonClose.onClick.AddListener(Close);
        mButtonNormal.onClick.AddListener(() => WindowManager.Instance.Open<UINormal>());
        mButtonWidget.onClick.AddListener(() => WindowManager.Instance.Open<UIWidget>());

    }
}