using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace UnityEngine.UI
{
    public class Tab : MonoBehaviour
    {
        Dictionary<Toggle, bool> mToggles = new Dictionary<Toggle, bool>();
        List<Toggle> mTogglesChanged = new List<Toggle>();
        // Start is called before the first frame update

        ToggleGroup mGroup;

        public TabEvent onTabValueChanged = new TabEvent();
        public TabEvent onTabAdd = new TabEvent();
        private bool mChildrenChanged = true;
        void Start()
        {
            mGroup = GetComponent<ToggleGroup>();
            if (mGroup == null) mGroup = gameObject.AddComponent<ToggleGroup>();

            InitTab();

        }

        private void InitTab()
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                var toggle = transform.GetChild(i).GetComponent<Toggle>();
                if (toggle != null && toggle.enabled && toggle.gameObject.activeInHierarchy)
                {
                    if (toggle.group != mGroup)
                    {
                        mGroup.RegisterToggle(toggle);
                    }

                    toggle.onValueChanged.RemoveListener(OnValueChanged);
                    toggle.onValueChanged.AddListener(OnValueChanged);
                    if (mToggles.ContainsKey(toggle) == false)
                    {
                        mToggles.Add(toggle, toggle.isOn);
                        onTabAdd.Invoke(toggle);
                    }
                    else
                    {

                    }
                }
            }

            OnValueChanged(true);

            mChildrenChanged = false;
        }
        void OnValueChanged(bool value)
        {
            mTogglesChanged.Clear();
            var it = mToggles.GetEnumerator();
            while (it.MoveNext())
            {
                var toggle = it.Current.Key;
                if (toggle && toggle.enabled && toggle.gameObject.activeInHierarchy)
                {
                    if (it.Current.Value != toggle.isOn || mChildrenChanged)
                    {
                        mTogglesChanged.Add(toggle);
                    }
                }
            }

            for (int i = 0; i < mTogglesChanged.Count; ++i)
            {
                var toggle = mTogglesChanged[i];
                mToggles[toggle] = toggle.isOn;
                onTabValueChanged.Invoke(toggle);
            }
        }

        private void OnTransformChildrenChanged()
        {
            mChildrenChanged = true;
            Debug.Log("OnTransformChildrenChanged");
            InitTab();

        }

        public class TabEvent : UnityEvent<Toggle>
        {
            public TabEvent() { }
        }
    }
}