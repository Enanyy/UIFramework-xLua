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

    public override void OnLoad(GameObject go)
    {
        base.OnLoad(go);

        //BINDING_CODE_BEGIN		mClose = GetComponent<Button>("Tween/UIWidget/@Button.mClose");
		mNormal = GetComponent<Button>("Tween/UIWidget/@Button.mNormal");
		mPop = GetComponent<Button>("Tween/UIWidget/@Button.mPop");
		mMain = GetComponent<Button>("Tween/UIWidget/@Button.mMain");
		mToggle = GetComponent<Toggle>("Tween/UIWidget/@Toggle.mToggle");
//BINDING_CODE_END

        mClose.onClick.AddListener(Close);
        mNormal.onClick.AddListener(() => WindowManager.Instance.Open<UINormal>());
        mPop.onClick.AddListener(() => WindowManager.Instance.Open<UIPop>());
        mMain.onClick.AddListener(() => WindowManager.Instance.CloseAllAndOpen<UIMain>());
    } 
}