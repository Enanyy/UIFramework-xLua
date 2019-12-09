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
           
            if(toggle.onValueChanged.GetPersistentEventCount() == 0)
            {
                toggle.onValueChanged.AddListener((value) =>
                {
                    onTabValueChanged.Invoke(toggle, mToggles.IndexOf(toggle));
                });
                onTabRegisterToggle.Invoke(toggle, mToggles.IndexOf(toggle));
            }
        }

        public void SetOn(int index)
        {
            if(index >=0 && index< mToggles.Count)
            {
                mToggles[index].isOn = true;
            }
        }
       
        private void OnTransformChildrenChanged()
        {
            Debug.Log("OnTransformChildrenChanged");
          
            RegisterToggle();

        }

        public class TabEvent : UnityEvent<Toggle, int>
        {
            public TabEvent() { }
        }
    }
}