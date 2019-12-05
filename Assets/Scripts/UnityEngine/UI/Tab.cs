using UnityEngine;
using UnityEngine.Events;


namespace UnityEngine.UI
{
    public class Tab : MonoBehaviour
    {
       
        ToggleGroup mGroup;

        public TabEvent onTabValueChanged = new TabEvent();

        private bool mInited = false;
        void Start()
        {
            mGroup = GetComponent<ToggleGroup>();

            if (mGroup == null) mGroup = gameObject.AddComponent<ToggleGroup>();


            RegisterToggle();

            mInited = true;
        }

        private void RegisterToggle()
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                var toggle = transform.GetChild(i).GetComponent<Toggle>();
                if (toggle != null && toggle.enabled && toggle.gameObject.activeInHierarchy)
                {
                    RegisterToggle(toggle);
                }
            }
        }

        public void RegisterToggle(Toggle toggle)
        {
            if(toggle== null)
            {
                return;
            }
            
            if (toggle.group != mGroup)
            {
                mGroup.RegisterToggle(toggle);
            }

            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener((value) => {

                onTabValueChanged.Invoke(toggle);
            });

            if(mInited == false)
            {
                onTabValueChanged.Invoke(toggle);
            }
        }
       

        private void OnTransformChildrenChanged()
        {
            Debug.Log("OnTransformChildrenChanged");
            mInited = false;

            RegisterToggle();

            mInited = true;
        }

        public class TabEvent : UnityEvent<Toggle>
        {
            public TabEvent() { }
        }
    }
}