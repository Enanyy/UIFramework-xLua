using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIMain : Window,IUpdateable
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
   
    public UIMain()
    {
        fixedWidgets = new List<System.Type> { /*typeof(UIFixed),*/ typeof(UIRight) };
    }
    public override void OnLoad(GameObject go)
    {
        base.OnLoad(go);
        //BINDING_CODE_BEGIN		mButtonNormal = GetComponent<Button>("Tween/SafeArea/@Button.mButtonNormal");
		mTextNormal = GetComponent<Text>("Tween/SafeArea/@Button.mButtonNormal/@Text.mTextNormal");
		mButtonPop = GetComponent<Button>("Tween/SafeArea/@Button.mButtonPop");
		mButtonWidget = GetComponent<Button>("Tween/SafeArea/@Button.mButtonWidget");
		mText = GetComponent<Text>("Tween/SafeArea/@Text.mText");
		mVerticalGridScrollView = GetComponent<VerticalScrollView>("Tween/SafeArea/@VerticalScrollView.mVerticalGridScrollView");
		mButtonAdd = GetComponent<Button>("Tween/SafeArea/@Button.mButtonAdd");
		mButtonRemove = GetComponent<Button>("Tween/SafeArea/@Button.mButtonRemove");
		mVerticalScrollView = GetComponent<VerticalScrollView>("Tween/SafeArea/@VerticalScrollView.mVerticalScrollView");
		mHorizontalScrollView = GetComponent<HorizontalScrollView>("Tween/SafeArea/@HorizontalScrollView.mHorizontalScrollView");
		mTab = GetComponent<Tab>("Tween/SafeArea/@Tab.mTab");
		mProgressBar = GetComponent<ProgressBar>("Tween/SafeArea/@ProgressBar.mProgressBar");
//BINDING_CODE_END

        mButtonNormal.onClick.AddListener(() => WindowManager.Instance.Open<UINormal>());
        mButtonPop.onClick.AddListener(() => WindowManager.Instance.Open<UIPop>());
        mButtonWidget.onClick.AddListener(() => WindowManager.Instance.Open<UIWidget>());

        mButtonAdd.onClick.AddListener(OnButtonAddClick);
        mButtonRemove.onClick.AddListener(OnButtonRemoveClick);
        
        mTab.onTabValueChanged.AddListener(OnTabValueChanged);
        mTab.onTabRegisterToggle.AddListener(OnTabRegisterToggle);

        mVerticalGridScrollView.onScrollItem.AddListener(OnVerticalGridScrollItem);

        for(int i = 0; i < 100; i++)
        {
            mGridScrollViewDataList.Add(Random.Range(0, 100));
        }

        mVerticalGridScrollView.totalCount = mGridScrollViewDataList.Count;
        mVerticalGridScrollView.Refill();

        mProgressBar.SetMinMax(0, 100);
        mProgressBar.onValueChanged.AddListener(OnProgessBarChanged);
        mProgressBar.AddTrigger(0, (progressBar, triggerAt) => {
            Debug.Log("Trigger at:" + triggerAt);
            mProgressBar.onValueChanged.RemoveListener(OnProgessBarChanged);
            mProgressBar.SetValue(100, 4);
        });
        mProgressBar.AddTrigger(100, (progressBar, triggerAt) =>
        {
            Debug.Log("Trigger at:" + triggerAt);
            mProgressBar.SetValue(0, 2);
        });
        mProgressBar.SetValue(100, 2);
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

    public void Update()
    {
        
    }
}
