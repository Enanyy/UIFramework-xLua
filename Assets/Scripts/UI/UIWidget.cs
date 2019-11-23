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
//BINDING_CODE_BEGIN		mClose = transform.Find("UIWidget/@Button.mClose").GetComponent<Button>();
		mNormal = transform.Find("UIWidget/@Button.mNormal").GetComponent<Button>();
		mPop = transform.Find("UIWidget/@Button.mPop").GetComponent<Button>();
		mMain = transform.Find("UIWidget/@Button.mMain").GetComponent<Button>();
		mToggle = transform.Find("UIWidget/@Toggle.mToggle").GetComponent<Toggle>();
        //BINDING_CODE_END

        mClose.onClick.AddListener(Close);
        mNormal.onClick.AddListener(() => WindowManager.Instance.Open<UINormal>());
        mPop.onClick.AddListener(() => WindowManager.Instance.Open<UIPop>());
        mMain.onClick.AddListener(() => WindowManager.Instance.CloseAllAndOpen<UIMain>());
    } 
}