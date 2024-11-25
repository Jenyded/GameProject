using TMPro;
using UnityEngine;

namespace Windows
{
    public class InfoWindow : WindowBase<InfoWindow.Model>
    {
        public class Model
        {
            public readonly string Message;
            
            public Model(string message)
            {
                Message = message;
            }
        }

        [SerializeField] private TextMeshProUGUI _message;
        
        protected override void OnOpen()
        {
            _message.text = ActiveModel.Message;
        }
    }
}