using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GOD.InputManager
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
#if !UNITY_EDITOR 
            InputFocusHandleAction(name, _inputField.text);
#endif
        }

        //Called from WebGlKeyboard.jslib
        public void ReceiveInputData(string value)
        {
            _inputField.text = value;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
#if !UNITY_EDITOR

            InputFocusHandleAction(name, _inputField.text);
#endif
        }
    }
}
