using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    public class Timer : MonoBehaviour
    {
        /// <summary>
        /// 开始值
        /// </summary>
        public float from { get; set; }
        /// <summary>
        /// 结束值
        /// </summary>
        public float to { get; set; }
        /// <summary>
        /// 增量
        /// </summary>
        public float addtion { get; set; }
        /// <summary>
        /// 间隔
        /// </summary>
        public float interval { get; set; }

        private float mTime;

        /// <summary>
        /// 是否计时中
        /// </summary>
        public bool isTick { get; set; }

        private float mValue;
        /// <summary>
        /// 当前值
        /// </summary>
        public float value
        {
            get { return mValue; }
            set 
            {
                float previous = mValue;
                mValue = value;
                onTimerValueChanged.Invoke(this, previous);
                Trigger(previous);
            }
        }
       
        public TimerEvent onTimerValueChanged = new TimerEvent();

        private Dictionary<float, List<UnityAction<Timer, float>>> mTriggers = new Dictionary<float, List<UnityAction<Timer, float>>>();

        /// <summary>
        /// 开始计时
        /// </summary>
        /// <param name="from">开始值</param>
        /// <param name="to">结束值</param>
        /// <param name="interval">间隔</param>
        /// <param name="addtion">增量</param>
        public void Begin(float from, float to, float interval, float addtion)
        {
            this.from = from;
            this.to = to;
            this.addtion = addtion;
            this.interval = interval;

            mValue = from;

            if (from < to)
            {
                Trigger(value - 0.0001f);
            }
            else
            {
                Trigger(value + 0.0001f);
            }
            isTick = true;

            mTime = Time.time;
        }

        // Update is called once per frame
        void Update()
        {
            if (isTick ==false)
            {
                return;
            }

            if(Time.time - mTime < interval)
            {
                return;
            }

            mTime = Time.time;

            float previous = value;
            float v = value + addtion;
           
            if(to > from)
            {
                if(v > to)
                {
                    value = to;
                    isTick = false;
                }
                else
                {
                    value = v;
                }
            }
            else
            {
                if (v < to)
                {
                    value = to;
                    isTick = false;
                }
                else
                {
                    value = v;
                }
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
                            it.Current.Value[i].Invoke(this, value);
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
        public void AddTrigger(float triggerAt, UnityAction<Timer, float> call)
        {
            if (call == null)
            {
                return;
            }
            if (mTriggers.TryGetValue(triggerAt, out List<UnityAction<Timer, float>> list) == false)
            {
                list = new List<UnityAction<Timer, float>>();
                mTriggers.Add(triggerAt, list);
            }
            if (list.Contains(call) == false)
            {
                list.Add(call);
            }

        }
        /// <summary>
        /// 移除触发器
        /// </summary>
        /// <param name="value"></param>
        /// <param name="call"></param>
        public void RemoveTrigger(float triggerAt, UnityAction<Timer, float> call)
        {
            if (call == null)
            {
                return;
            }
            if (mTriggers.TryGetValue(triggerAt, out List<UnityAction<Timer, float>> list))
            {
                list.Remove(call);
            }
        }
        public void RemoveAllTrigger()
        {
            mTriggers.Clear();
        }

        public class TimerEvent : UnityEvent<Timer,float>
        {

        }
    }
}
