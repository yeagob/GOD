using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Sc.InputManager
{
    public class InputFieldHandler : MonoBehaviour, ISelectHandler, IPointerClickHandler
    {
        [DllImport("__Internal")]//WebGlKeyboard.jslib
        public static extern void InputFocusHandleAction(string name, string str);

        private TMP_InputField _inputField;

        private void Awake()
        {
            _inputField = GetComponent<TMP_InputField>();
        }

        public void OnSelect(BaseEventData eventData)
        {
            InputFocusHandleAction(name, _inputField.text);
        }

        //Called from WebGlKeyboard.jslib
        public void ReceiveInputData(string value)
        {
            _inputField.text = value;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            InputFocusHandleAction(name, _inputField.text);
        }
    }
}
