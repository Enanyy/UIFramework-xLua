using UnityEngine;
using UnityEngine.UI;

public class UIWidget : Window
{
    public UIWidget()
    {
        type = WindowType.Widget;
        fixedOrder = 1000;
    }
//BINDING_DEFINITION_BEGIN	private Button mClose;
	private Button mNormal;
	private Button mPop;
	private Button mMain;
	private Toggle mToggle;
//BINDING_DEFINITION_END

    private void Awake()
    {
//BINDING_CODE_BEGIN		transform.Find("Tween/UIWidget/@Button.mClose").TryGetComponent(out mClose);
		transform.Find("Tween/UIWidget/@Button.mNormal").TryGetComponent(out mNormal);
		transform.Find("Tween/UIWidget/@Button.mPop").TryGetComponent(out mPop);
		transform.Find("Tween/UIWidget/@Button.mMain").TryGetComponent(out mMain);
		transform.Find("Tween/UIWidget/@Toggle.mToggle").TryGetComponent(out mToggle);
//BINDING_CODE_END

        mClose.onClick.AddListener(Close);
        mNormal.onClick.AddListener(() => WindowManager.Instance.Open<UINormal>());
        mPop.onClick.AddListener(() => WindowManager.Instance.Open<UIPop>());
        mMain.onClick.AddListener(() => WindowManager.Instance.CloseAllAndOpen<UIMain>());
    } 
}