using UnityEngine;
using UnityEngine.UI;

public class UIMain : Window
{
    //BINDING_DEFINITION_BEGIN	private Button mButtonNormal;
	private Text mTextNormal;
	private Button mButtonPop;
	private Button mButtonWidget;
	private Text mText;
	private VerticalScrollView mVerticalGridScrollView;
	private Button mButtonAdd;
	private Button mButtonRemove;
	private VerticalScrollView mVerticalScrollView;
	private HorizontalScrollView mHorizontalScrollView;
	private Tab mTab;
	private ProgressBar mProgressBar;
//BINDING_DEFINITION_END

    private void Awake()
    {
        //BINDING_CODE_BEGIN		mButtonNormal = transform.Find("SafeArea/@Button.mButtonNormal").GetComponent<Button>();
		mTextNormal = transform.Find("SafeArea/@Button.mButtonNormal/@Text.mTextNormal").GetComponent<Text>();
		mButtonPop = transform.Find("SafeArea/@Button.mButtonPop").GetComponent<Button>();
		mButtonWidget = transform.Find("SafeArea/@Button.mButtonWidget").GetComponent<Button>();
		mText = transform.Find("SafeArea/@Text.mText").GetComponent<Text>();
		mVerticalGridScrollView = transform.Find("SafeArea/@VerticalScrollView.mVerticalGridScrollView").GetComponent<VerticalScrollView>();
		mButtonAdd = transform.Find("SafeArea/@Button.mButtonAdd").GetComponent<Button>();
		mButtonRemove = transform.Find("SafeArea/@Button.mButtonRemove").GetComponent<Button>();
		mVerticalScrollView = transform.Find("SafeArea/@VerticalScrollView.mVerticalScrollView").GetComponent<VerticalScrollView>();
		mHorizontalScrollView = transform.Find("SafeArea/@HorizontalScrollView.mHorizontalScrollView").GetComponent<HorizontalScrollView>();
		mTab = transform.Find("SafeArea/@Tab.mTab").GetComponent<Tab>();
		mProgressBar = transform.Find("SafeArea/@ProgressBar.mProgressBar").GetComponent<ProgressBar>();
        //BINDING_CODE_END

        mButtonNormal.onClick.AddListener(() => WindowManager.Instance.Open<UINormal>());
        mButtonPop.onClick.AddListener(() => WindowManager.Instance.Open<UIPop>());
        mButtonWidget.onClick.AddListener(() => WindowManager.Instance.Open<UIWidget>());

    } 
}
