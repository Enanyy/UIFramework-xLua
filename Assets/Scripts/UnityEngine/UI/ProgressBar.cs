using System.Collections.Generic;
using UnityEngine.Events;

namespace UnityEngine.UI
{
    [RequireComponent(typeof(Slider))]
    public class ProgressBar : MonoBehaviour
    {
        private Slider mSlider;
        private float mFrom;
        private float mTo;
        private float mDuration;
        private float mTime;
        private Dictionary<float, List<UnityAction<float>>> mTriggers = new Dictionary<float, List<UnityAction<float>>>();

        public ProgressValueChanged onValueChanged = new ProgressValueChanged();
        public float value
        {
            get { return mSlider.value; }
            set
            {
                float from = mSlider.value;
                mSlider.value = value;
                onValueChanged.Invoke(from, mSlider.value);
            }
        }
        private void Awake()
        {
            mSlider = GetComponent<Slider>();
            if(mSlider == null)
            {
                Debug.LogError("ProgressBar 必须添加 Slider");
            }
        }
        public void SetMinMax(float min, float max)
        {
            mSlider.minValue = min;
            mSlider.maxValue = max;
        }

        public void SetValue(float to, float duration = 0)
        {
            mFrom = mSlider.value;
            mTo = Mathf.Clamp(to, mSlider.minValue, mSlider.maxValue);
            if (to == value)
            {
                Trigger(value - 0.0001f);
                Trigger(value + 0.0001f);
            }
            else
            {
                if (duration <= 0)
                {
                    value = mTo;
                    Trigger(value);
                }
                else
                {
                    mDuration = duration;
                    mTime = 0;
                }
            }
        }

       
        // Update is called once per frame
        void Update()
        {
            if (mDuration > 0)
            {
                mTime += Time.deltaTime;
                if (mTime >= mDuration)
                {
                    mTime = mDuration;
                }
                float previous = mSlider.value;

                float factor = mTime / mDuration;

                value = Mathf.Lerp(mFrom, mTo, factor);
                if(value == mTo)
                {
                    mDuration = 0; 
                    mTime = 0;
                }

                Trigger(previous);
            }
        }

        private void Trigger(float previous)
        {
            var it = mTriggers.GetEnumerator();
            while (it.MoveNext())
            {
                float key = it.Current.Key;
                if ((previous < key && value >= key) || (previous > key && value <= key))
                {
                    for (int i = 0; i < it.Current.Value.Count; ++i)
                    {
                        if (it.Current.Value[i] != null)
                        {
                            it.Current.Value[i].Invoke(value);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 添加触发器
        /// </summary>
        /// <param name="value"></param>
        /// <param name="call"></param>
        public void AddTrigger(float value, UnityAction<float> call)
        {
            if(call == null)
            {
                return;
            }
            if(mTriggers.TryGetValue(value, out List<UnityAction<float>> list) ==false)
            {
                list = new List<UnityAction<float>>();
                mTriggers.Add(value, list);
            }
            if(list.Contains(call) ==false)
            {
                list.Add(call);
            }
           
        }
        /// <summary>
        /// 移除触发器
        /// </summary>
        /// <param name="vaue"></param>
        /// <param name="call"></param>
        public void RemoveTrigger(float vaue, UnityAction<float> call)
        {
            if (call == null)
            {
                return;
            }
            if (mTriggers.TryGetValue(value, out List<UnityAction<float>> list))
            {
                list.Remove(call);
            }
        }
        public void RemoveAllTrigger()
        {
            mTriggers.Clear(); 
        }

        public class ProgressValueChanged:UnityEvent<float,float>
        {
            public ProgressValueChanged() { }
        }
    }
}
