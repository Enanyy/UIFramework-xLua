using UnityEngine;
using UnityEngine.UI;

public class UIWidget : WindowComponent
{
  
//BINDING_DEFINITION_BEGIN	private Button mClose;
	private Button mNormal;
	private Button mPop;
	private Button mMain;
	private Toggle mToggle;
//BINDING_DEFINITION_END

   private void Awake()
    {
        //BINDING_CODE_BEGIN		mClose = GetComponent<Button>("Tween/UIWidget/@Button.mClose");
		mNormal = GetComponent<Button>("Tween/UIWidget/@Button.mNormal");
		mPop = GetComponent<Button>("Tween/UIWidget/@Button.mPop");
		mMain = GetComponent<Button>("Tween/UIWidget/@Button.mMain");
		mToggle = GetComponent<Toggle>("Tween/UIWidget/@Toggle.mToggle");
//BINDING_CODE_END

        mClose.onClick.AddListener(() => WindowManager.Instance.Close(UIDefine.UIWidget));
        mNormal.onClick.AddListener(() => WindowManager.Instance.Open(UIDefine.UINormal));
        mPop.onClick.AddListener(() => WindowManager.Instance.Open(UIDefine.UIPop));
        mMain.onClick.AddListener(() => WindowManager.Instance.CloseAllAndOpen(UIDefine.UIMain));
    } 
}