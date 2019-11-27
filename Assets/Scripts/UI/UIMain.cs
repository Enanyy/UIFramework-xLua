using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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


    private Dictionary<Transform, int> mGridItemDic = new Dictionary<Transform, int>();
    private List<int> mGridScrollViewDataList = new List<int>();
    public UIMain()
    {
        fixedWidgets = new List<System.Type> { typeof(UIFixed), typeof(UIRight) };
    }
    private void Awake()
    {
        //BINDING_CODE_BEGIN		transform.Find("SafeArea/@Button.mButtonNormal").TryGetComponent(out mButtonNormal);
		transform.Find("SafeArea/@Button.mButtonNormal/@Text.mTextNormal").TryGetComponent(out mTextNormal);
		transform.Find("SafeArea/@Button.mButtonPop").TryGetComponent(out mButtonPop);
		transform.Find("SafeArea/@Button.mButtonWidget").TryGetComponent(out mButtonWidget);
		transform.Find("SafeArea/@Text.mText").TryGetComponent(out mText);
		transform.Find("SafeArea/@VerticalScrollView.mVerticalGridScrollView").TryGetComponent(out mVerticalGridScrollView);
		transform.Find("SafeArea/@Button.mButtonAdd").TryGetComponent(out mButtonAdd);
		transform.Find("SafeArea/@Button.mButtonRemove").TryGetComponent(out mButtonRemove);
		transform.Find("SafeArea/@VerticalScrollView.mVerticalScrollView").TryGetComponent(out mVerticalScrollView);
		transform.Find("SafeArea/@HorizontalScrollView.mHorizontalScrollView").TryGetComponent(out mHorizontalScrollView);
		transform.Find("SafeArea/@Tab.mTab").TryGetComponent(out mTab);
		transform.Find("SafeArea/@ProgressBar.mProgressBar").TryGetComponent(out mProgressBar);
//BINDING_CODE_END

        mButtonNormal.onClick.AddListener(() => WindowManager.Instance.Open<UINormal>());
        mButtonPop.onClick.AddListener(() => WindowManager.Instance.Open<UIPop>());
        mButtonWidget.onClick.AddListener(() => WindowManager.Instance.Open<UIWidget>());

        mButtonAdd.onClick.AddListener(OnButtonAddClick);
        mButtonRemove.onClick.AddListener(OnButtonRemoveClick);
        mTab.onTabValueChanged.AddListener(OnTabSelectChanged);

        mVerticalGridScrollView.onScrollItem.AddListener(OnVerticalGridScrollItem);

        for(int i = 0; i < 100; i++)
        {
            mGridScrollViewDataList.Add(Random.Range(0, 100));
        }

        mVerticalGridScrollView.totalCount = mGridScrollViewDataList.Count;
        mVerticalGridScrollView.Refill();

        mProgressBar.SetMinMax(0, 100);
        mProgressBar.onValueChanged.AddListener(OnProgessBarChanged);
        mProgressBar.AddListener(0, (value) => {
            Debug.Log("Trigger at:" + value);
            mProgressBar.onValueChanged.RemoveListener(OnProgessBarChanged);
            mProgressBar.SetValue(100, 4);
        });
        mProgressBar.AddListener(100, (value) =>
        {
            Debug.Log("Trigger at:" + value);
            mProgressBar.SetValue(0, 2);
        });
        mProgressBar.SetValue(100, 2);
    } 

    void OnVerticalGridScrollItem(Transform item, int index)
    {
        if(mGridItemDic.ContainsKey(item)==false)
        {
            mGridItemDic.Add(item, index);
        }
        else
        {
            mGridItemDic[item] = index;
        }
        item.Find("Text").GetComponent<Text>().text = index.ToString();
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

    void OnTabSelectChanged(Toggle toggle)
    {
        var image = toggle.GetComponent<Image>();
        image.color = toggle.isOn ? Color.yellow : Color.white;
    }

    void OnProgessBarChanged(float from, float to)
    {
        Debug.Log("value from: " + from + " to: " + to);
    }
}
