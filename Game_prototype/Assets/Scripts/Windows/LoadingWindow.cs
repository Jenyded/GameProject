using TMPro;
using UnityEngine;

namespace Windows
{
    public class LoadingWindow : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _progress;
        
        private int _lastValue;

        public void ReportProgress(float progress)
        {
            var currValue = Mathf.RoundToInt(progress * 100);

            if (currValue == _lastValue)
                return;
            
            _lastValue = currValue;
            _progress.text = _lastValue != 0 && _lastValue != 100 ? $"{_lastValue}/100" : string.Empty;
        }
    }
}