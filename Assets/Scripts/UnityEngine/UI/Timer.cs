using UnityEngine;
using System.Collections;
using UnityEngine.Events;

namespace UnityEngine.UI
{
    public class Timer : MonoBehaviour
    {
        private float mFrom;
        private float mTo;
        private float mAddtion;
        private float mInterval;
        private float mTime;

        private bool mTicking = false;

        private float mValue;
        public float value
        {
            get { return mValue; }
            set 
            { 
                mValue = value;
                onTimerValueChanged.Invoke(this);
            }
        }
       
        public TimerEvent onTimerValueChanged = new TimerEvent();

        public void SetTimer(float from, float to, float interval , float addtion)
        {
            mFrom = from;
            mTo = to;
            mAddtion = addtion;
            mInterval = interval;

            value = mFrom;

            mTicking = true;

            mTime = Time.time;
        }

        public void Pause() { mTicking = false; }
        public void Resume() { mTicking = true; }

        // Update is called once per frame
        void Update()
        {
            if (mTicking ==false)
            {
                return;
            }

            if(Time.time - mTime < mInterval)
            {
                return;
            }

            mTime = Time.time;

            float v = value + mAddtion;
           
            if(mTo > mFrom)
            {
                if(v > mTo)
                {
                    value = mTo;
                    mTicking = false;
                }
                else
                {
                    value = v;
                }
            }
            else
            {
                if (v < mTo)
                {
                    value = mTo;
                    mTicking = false;
                }
                else
                {
                    value = v;
                }
            }
            
        }

        public class TimerEvent : UnityEvent<Timer>
        {

        }
    }
}
