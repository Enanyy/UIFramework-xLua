using System.Collections.Generic;
using UnityEngine.Events;

namespace UnityEngine.UI
{

    [RequireComponent(typeof(Slider))]
    public class ProgressBar : MonoBehaviour
    {
        private Slider mSlider;
        private float mValue;
        private float mDuration;
        private float mTime;
        private float mFrom;
        private Dictionary<float, List<UnityAction<float>>> mListeners = new Dictionary<float, List<UnityAction<float>>>();

        public ProgressEvent onValueChanged = new ProgressEvent();
        public float value
        {
            get { return mSlider.value; }
            set
            {
                
                if (mSlider.value != value)
                {
                    float from = mSlider.value;
                    mSlider.value = value;
                    onValueChanged.Invoke(from, mSlider.value);
                }
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
            mValue = Mathf.Clamp(to, mSlider.minValue, mSlider.maxValue);
            if (to == value)
            {
                CheckListener(value);
            }
            else
            {
                if (duration <= 0)
                {
                    value = mValue;
                    CheckListener(value);
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

                value = Mathf.Lerp(mFrom, mValue, factor);
                if(value == mValue)
                {
                    mDuration = 0; 
                    mTime = 0;
                }

                CheckListener(previous);
            }
        }

        private void CheckListener(float previous)
        {
            var it = mListeners.GetEnumerator();
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

        public void AddListener(float value, UnityAction<float> call)
        {
            if(mListeners.TryGetValue(value, out List<UnityAction<float>> list) ==false)
            {
                list = new List<UnityAction<float>>();
                mListeners.Add(value, list);
            }
            if(list.Contains(call) ==false)
            {
                list.Add(call);
            }
           
        }
        public void RemoveListener(float vaue, UnityAction<float> call)
        {
            if (mListeners.TryGetValue(value, out List<UnityAction<float>> list))
            {
                list.Remove(call);
            }
        }
        public void RemoveAllListener()
        {
            mListeners.Clear(); 
        }

        public class ProgressEvent:UnityEvent<float,float>
        {
            public ProgressEvent() { }
        }
    }
}
