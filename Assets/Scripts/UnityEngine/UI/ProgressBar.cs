using System.Collections.Generic;
using UnityEngine.Events;

namespace UnityEngine.UI
{
    public class ProgressBar : MonoBehaviour
    {
        private float mFrom;
        private float mTo;
        private float mDuration;
        private float mTime;
        private Dictionary<float, List<UnityAction<ProgressBar, float>>> mTriggers = new Dictionary<float, List<UnityAction<ProgressBar,float>>>();

        public ProgressBarEvent onValueChanged = new ProgressBarEvent();

        private float mValue;
        public float value
        {
            get { return mValue; }
            set
            {
                float from = mValue;
                mValue = value;
                onValueChanged.Invoke(this,from);

                UpdateSlider();
            }
        }
        public float minValue { get; set; }
        public float maxValue { get; set; }

        /// <summary>
        /// 要控制的Slider
        /// </summary>
        public Slider slider;
        /// <summary>
        /// 是否控制Slider
        /// </summary>
        public bool updateSlider = true;

        public void SetMinMax(float min, float max)
        {
            minValue = min;
            maxValue = max;

            UpdateSlider();
        }

        public void UpdateSlider()
        {
            if (updateSlider)
            {
                if (slider == null)
                {
                    TryGetComponent(out slider);
                }
                if (slider)
                {
                    slider.minValue = minValue;
                    slider.maxValue = maxValue;
                    slider.value = mValue;
                }
            }
        }

        public void SetValue(float to, float duration = 0)
        {
            mFrom = mValue;
            mTo = Mathf.Clamp(to, minValue,maxValue);
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
                float previous = value;

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
            using (var it = mTriggers.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    float key = it.Current.Key;
                    if ((previous < key && value >= key) || (previous > key && value <= key))
                    {
                        for (int i = 0; i < it.Current.Value.Count; ++i)
                        {
                            if (it.Current.Value[i] != null)
                            {
                                it.Current.Value[i].Invoke(this, value);
                            }
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
        public void AddTrigger(float triggerAt, UnityAction<ProgressBar, float> call)
        {
            if(call == null)
            {
                return;
            }
            if(mTriggers.TryGetValue(triggerAt, out List<UnityAction<ProgressBar, float>> list) ==false)
            {
                list = new List<UnityAction<ProgressBar, float>>();
                mTriggers.Add(triggerAt, list);
            }
            if(list.Contains(call) ==false)
            {
                list.Add(call);
            }
           
        }
        /// <summary>
        /// 移除触发器
        /// </summary>
        /// <param name="value"></param>
        /// <param name="call"></param>
        public void RemoveTrigger(float triggerAt, UnityAction<ProgressBar, float> call)
        {
            if (call == null)
            {
                return;
            }
            if (mTriggers.TryGetValue(triggerAt, out List<UnityAction<ProgressBar, float>> list))
            {
                list.Remove(call);
            }
        }
        public void RemoveAllTrigger()
        {
            mTriggers.Clear(); 
        }

        public class ProgressBarEvent:UnityEvent<ProgressBar, float>
        {
            public ProgressBarEvent() { }
        }
    }
}
