using UnityEngine;
using UnityEngine.Events;


namespace UnityEngine.UI
{
    public class Tab : MonoBehaviour
    {
       
        ToggleGroup mGroup;

        public TabEvent onTabValueChanged = new TabEvent();

        void Start()
        {
            mGroup = GetComponent<ToggleGroup>();

            if (mGroup == null) mGroup = gameObject.AddComponent<ToggleGroup>();

            RegisterToggle();
        }

        private void RegisterToggle()
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                if (transform.GetChild(i).TryGetComponent(out Toggle toggle))
                {
                    if (toggle != null && toggle.enabled && toggle.gameObject.activeInHierarchy)
                    {
                        RegisterToggle(toggle);
                    }
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

            if(toggle.onValueChanged.GetPersistentEventCount() == 0)
            {
                toggle.onValueChanged.AddListener((value) =>
                {
                    onTabValueChanged.Invoke(toggle);
                });
                onTabValueChanged.Invoke(toggle);
            }
        }
       

        private void OnTransformChildrenChanged()
        {
            Debug.Log("OnTransformChildrenChanged");
          
            RegisterToggle();

        }

        public class TabEvent : UnityEvent<Toggle>
        {
            public TabEvent() { }
        }
    }
}