using UnityEngine;
using UnityEngine.UI;

public class UIBg : WindowComponent
{
//BINDING_DEFINITION_BEGIN
	private Button mButtonClose;
//BINDING_DEFINITION_END
   
    private void Awake()
    {
//BINDING_CODE_BEGIN
		mButtonClose = GetComponent<Button>("Tween/SafeArea/@Button.mButtonClose");
//BINDING_CODE_END
        mButtonClose.onClick.AddListener(() => Close());

    }

    public override void OnInit()
    {
        if(parameters!=null)
        {
            using(var it = parameters.GetEnumerator())
            {
                while(it.MoveNext())
                {
                    Debug.LogError("param:" + it.Current.Key + "," + it.Current.Value);
                }
            }
        }
    }

    public override void OnShow()
    {
        base.OnShow();
        if(context.name == "UIMain")
        {
            mButtonClose.gameObject.SetActive(false);
        }
    }
}