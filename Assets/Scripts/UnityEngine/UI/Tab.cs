using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;


namespace UnityEngine.UI
{
    public class Tab : MonoBehaviour
    {
       
        ToggleGroup mGroup;

        public TabEvent onTabValueChanged = new TabEvent();
        public TabEvent onTabRegisterToggle = new TabEvent();

        private List<Toggle> mToggles = new List<Toggle>();

        private bool mChildrenChanged = false;

        private void Awake()
        {
            mGroup = GetComponent<ToggleGroup>();

            if (mGroup == null) mGroup = gameObject.AddComponent<ToggleGroup>();

        }
      
        public void RegisterToggle()
        {
            mToggles.Clear();

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

            if(mToggles.Contains(toggle)==false)
            {
                mToggles.Add(toggle);
            }
            int index = mToggles.IndexOf(toggle);

            if (toggle.onValueChanged.GetPersistentEventCount() == 0)
            {
                toggle.onValueChanged.AddListener((value) =>
                {
                    index = mToggles.IndexOf(toggle);
                    if (index >= 0 && index < mToggles.Count)
                    {
                        onTabValueChanged.Invoke(toggle, index);
                    }
                });
                if (index >= 0 && index < mToggles.Count)
                {
                    onTabRegisterToggle.Invoke(toggle, index);
                }
            }
        }

        public void SetOn(int index)
        {
            if(index >=0 && index< mToggles.Count)
            {
                mToggles[index].isOn = true;
            }
        }
        private void Update()
        {
            if(mChildrenChanged)
            {
                mChildrenChanged = false;
                RegisterToggle();
            }
        }

        private void OnTransformChildrenChanged()
        {
            mChildrenChanged = true;
        }

        public class TabEvent : UnityEvent<Toggle, int>
        {
            public TabEvent() { }
        }
    }
}