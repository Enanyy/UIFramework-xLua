using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIMain : WindowComponent
{
    //BINDING_DEFINITION_BEGIN	private Button mButtonNormal;
	private Text mTextNormal;
	private Button mButtonPop;
	private Button mButtonWidget;
    private Button mButtonRight;
    private Text mText;
	private ScrollView mVerticalGridScrollView;
	private Button mButtonAdd;
	private Button mButtonRemove;
	private ScrollView mVerticalScrollView;
	private ScrollView mHorizontalScrollView;
	private Tab mTab;
	private ProgressBar mProgressBar;
	private Timer mTimer;
//BINDING_DEFINITION_END


    class GridItem
    {
        public Transform mRoot;
        public int index;
        public int data;

        private Text mText;
        public void Init(Transform item)
        {
            mRoot = item;
            mRoot.Find("Text").TryGetComponent(out mText);
        }

        public void UpdateData( int index, int data)
        {
            this.index = index;
            this.data = data;

            mText.text = string.Format("{0} {1}", index, data);
        }
    }
    private Dictionary<Transform, GridItem> mGridItemDic = new Dictionary<Transform, GridItem>();
    private List<int> mGridScrollViewDataList = new List<int>();

    public enum TabEnum
    {
        T1,
        T2,
        T3
    }

    TabEnum current = TabEnum.T2;
  
    private void Awake()
    {
        //BINDING_CODE_BEGIN		mButtonNormal = GetComponent<Button>("Tween/SafeArea/@Button.mButtonNormal");
		mTextNormal = GetComponent<Text>("Tween/SafeArea/@Button.mButtonNormal/@Text.mTextNormal");
		mButtonPop = GetComponent<Button>("Tween/SafeArea/@Button.mButtonPop");
		mButtonWidget = GetComponent<Button>("Tween/SafeArea/@Button.mButtonWidget");
        mButtonRight = GetComponent<Button>("Tween/SafeArea/@Button.mButtonRight");
        mText = GetComponent<Text>("Tween/SafeArea/@Text.mText");
		mVerticalGridScrollView = GetComponent<ScrollView>("Tween/SafeArea/@ScrollView.mVerticalGridScrollView");
		mButtonAdd = GetComponent<Button>("Tween/SafeArea/@Button.mButtonAdd");
		mButtonRemove = GetComponent<Button>("Tween/SafeArea/@Button.mButtonRemove");
		mVerticalScrollView = GetComponent<ScrollView>("Tween/SafeArea/@ScrollView.mVerticalScrollView");
		mHorizontalScrollView = GetComponent<ScrollView>("Tween/SafeArea/@ScrollView.mHorizontalScrollView");
		mTab = GetComponent<Tab>("Tween/SafeArea/@Tab.mTab");
		mProgressBar = GetComponent<ProgressBar>("Tween/SafeArea/@ProgressBar.mProgressBar");
		mTimer = GetComponent<Timer>("Tween/SafeArea/@Timer.mTimer");
//BINDING_CODE_END

        mButtonNormal.onClick.AddListener(() => WindowManager.Instance.Open("UINormal"));
        mButtonPop.onClick.AddListener(() => WindowManager.Instance.Open("UIPop"));
        mButtonWidget.onClick.AddListener(() => WindowManager.Instance.Open("UIWidget"));
        mButtonRight.onClick.AddListener(() => SetWidgetActive("UIRight", !IsWidgetActive("UIRight")));

        mButtonAdd.onClick.AddListener(OnButtonAddClick);
        mButtonRemove.onClick.AddListener(OnButtonRemoveClick);
        
        mTab.onTabValueChanged.AddListener(OnTabValueChanged);
        mTab.onTabRegisterToggle.AddListener(OnTabRegisterToggle);
        mTab.RegisterToggle();

        mVerticalGridScrollView.onScrollItem.AddListener(OnVerticalGridScrollItem);

        for(int i = 0; i < 100; i++)
        {
            mGridScrollViewDataList.Add(Random.Range(0, 100));
        }

        mVerticalGridScrollView.totalCount = mGridScrollViewDataList.Count;
        mVerticalGridScrollView.Refill();

        mProgressBar.SetMinMax(0, 100);
        mProgressBar.onValueChanged.AddListener(OnProgessBarChanged);
        mProgressBar.AddTrigger(0, (progressBar, value) => {
            Debug.Log("Trigger at:" + value);
            mProgressBar.onValueChanged.RemoveListener(OnProgessBarChanged);
            mProgressBar.SetValue(100, 4);
        });
        mProgressBar.AddTrigger(100, (progressBar, value) =>
        {
            Debug.Log("Trigger at:" + value);
            mProgressBar.SetValue(0, 2);
        });
        mProgressBar.SetValue(100, 2);

        mTimer.TryGetComponent(out Text text);
        mTimer.onTimerValueChanged.AddListener((timer,previous) => { 
            
            if(text)
            {
                text.text = string.Format("{0}s", timer.value);
            }
        });
        mTimer.AddTrigger(0, (timer, value) => {
            Debug.Log("timer trigger at:" + value);
        });
        mTimer.Begin(5, 0, 1, -1);
    }

    void OnVerticalGridScrollItem(Transform item, int index)
    {    
        if(mGridItemDic.TryGetValue(item, out GridItem gridItem)==false)
        {
            gridItem = new GridItem();
            gridItem.Init(item);
            mGridItemDic.Add(item, gridItem);
        }
        gridItem.UpdateData(index, mGridScrollViewDataList[index]);
    }

    void OnButtonAddClick()
    {
        for(int i = 0; i <10; i++)
        {
            mGridScrollViewDataList.Add(Random.Range(0, 100));
        }
        mVerticalGridScrollView.totalCount = mGridScrollViewDataList.Count;
        mVerticalGridScrollView.Refresh();
    }

    void OnButtonRemoveClick()
    {
        for(int i = 0; i < 10; ++i)
        {
            if(mGridScrollViewDataList.Count>0)
            {
                mGridScrollViewDataList.RemoveAt(Random.Range(0, mGridScrollViewDataList.Count));
            }
        }
        mVerticalGridScrollView.totalCount = mGridScrollViewDataList.Count;
        mVerticalGridScrollView.Refresh();
    }

    void OnTabRegisterToggle(Toggle toggle, int index)
    {
        if (index == (int)current)
        {
            toggle.isOn = true;
        }
    }
    void OnTabValueChanged(Toggle toggle, int index)
    {
        var image = toggle.GetComponent<Image>();
        image.color = toggle.isOn ? Color.yellow : Color.white;
    }

    void OnProgessBarChanged(ProgressBar progressBar, float from)
    {
        Debug.Log("value from: " + from + " to: " + progressBar.value);
    }

    public override void OnShow()
    {
        Debug.LogError("OnShow");
    }

    public override void OnHide()
    {
        Debug.LogError("OnHide");
    }


    public void Update()
    {
        
    }

    [WindowMenu("打开主界面")]
    private static void OpenUIMain()
    {
        WindowManager.Instance.Open("UIMain");
    }
}
