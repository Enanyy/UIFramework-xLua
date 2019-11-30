using UnityEngine;
using UnityEngine.UI;

public class UIPop : Window
{
//BINDING_DEFINITION_BEGIN	private Button mButtonClose;
	private Button mbuttonMain;
	private Button mButtonNormal;
	private Button mButtonWidget;
	private Text mText;
//BINDING_DEFINITION_END

    private void Awake()
    {
//BINDING_CODE_BEGIN		transform.Find("Tween/Widget/@Button.mButtonClose").TryGetComponent(out mButtonClose);
		transform.Find("Tween/Widget/@Button.mbuttonMain").TryGetComponent(out mbuttonMain);
		transform.Find("Tween/Widget/@Button.mButtonNormal").TryGetComponent(out mButtonNormal);
		transform.Find("Tween/Widget/@Button.mButtonWidget").TryGetComponent(out mButtonWidget);
		transform.Find("Tween/Widget/@Text.mText").TryGetComponent(out mText);
//BINDING_CODE_END
        mButtonClose.onClick.AddListener(Close);
        mButtonNormal.onClick.AddListener(() => WindowManager.Instance.Open<UINormal>());
        mButtonWidget.onClick.AddListener(() => WindowManager.Instance.Open<UIWidget>());
        mbuttonMain.onClick.AddListener(() => WindowManager.Instance.CloseAllAndOpen<UIMain>());
    }
}