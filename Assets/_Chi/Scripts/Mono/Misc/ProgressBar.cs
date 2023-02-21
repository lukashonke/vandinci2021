using UnityEngine;

namespace _Chi.Scripts.Mono.Misc
{
    public class ProgressBar : MonoBehaviour
    {
        public RectTransform valueTransform;

        public int maxVal;
        public int currentVal;

        public void AddValue(int val)
        {
            currentVal += val;
            
            Recalculate();
        }

        public void SetMaxValue(int val)
        {
            maxVal = val;
            
            Recalculate();
        }

        public void ResetValue()
        {
            currentVal = 0;
            
            Recalculate();
        }
        
        public void Recalculate()
        {
            if (maxVal > 0)
            {
                SetValue((float)currentVal / maxVal);
            }
            else
            {
                SetValue(0);
            }
        }
        
        private void SetValue(float val)
        {
            valueTransform.localScale = new Vector3(val, 1, 1);
        }
    }
}