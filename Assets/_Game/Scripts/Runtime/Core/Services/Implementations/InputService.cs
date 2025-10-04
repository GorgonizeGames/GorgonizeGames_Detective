using System;
using UnityEngine;

namespace Game.Runtime.Core.Services
{
    public class InputService : MonoBehaviour, IInputService
    {
        private bool _inputEnabled = true;
        
        public event Action<KeyCode> OnKeyPressed;
        public event Action<int> OnMouseButtonPressed;
        public event Action OnEscapePressed;
        
        public Vector2 MousePosition => Input.mousePosition;
        public bool IsCtrlPressed => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        public bool IsShiftPressed => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        public bool IsAltPressed => Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        public bool IsInputEnabled => _inputEnabled;
        
        private void Update()
        {
            if (!_inputEnabled) return;
            
            HandleMouseInput();
            HandleKeyboardInput();
        }
        
        private void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0)) OnMouseButtonPressed?.Invoke(0);
            if (Input.GetMouseButtonDown(1)) OnMouseButtonPressed?.Invoke(1);
            if (Input.GetMouseButtonDown(2)) OnMouseButtonPressed?.Invoke(2);
        }
        
        private void HandleKeyboardInput()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnEscapePressed?.Invoke();
                OnKeyPressed?.Invoke(KeyCode.Escape);
            }
            
            if (Input.anyKeyDown)
            {
                foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(key))
                    {
                        OnKeyPressed?.Invoke(key);
                        break;
                    }
                }
            }
        }
        
        public bool GetMouseButtonDown(int button) => _inputEnabled && Input.GetMouseButtonDown(button);
        public bool GetMouseButton(int button) => _inputEnabled && Input.GetMouseButton(button);
        public bool GetMouseButtonUp(int button) => _inputEnabled && Input.GetMouseButtonUp(button);
        public bool GetKeyDown(KeyCode key) => _inputEnabled && Input.GetKeyDown(key);
        public bool GetKey(KeyCode key) => _inputEnabled && Input.GetKey(key);
        public bool GetKeyUp(KeyCode key) => _inputEnabled && Input.GetKeyUp(key);
        
        public void EnableInput()
        {
            _inputEnabled = true;
        }
        
        public void DisableInput()
        {
            _inputEnabled = false;
        }
    }
}