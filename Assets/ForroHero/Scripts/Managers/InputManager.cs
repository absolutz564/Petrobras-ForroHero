using System;
using UnityEngine;
using UnityEngine.InputSystem;
using ForroHero.Scripts.Utils;

namespace ForroHero.Scripts.Managers
{
    public class InputManager : SingletonBehaviour<InputManager>
    {
        [Header("Keyboard Keys")]
        [SerializeField] private Key m_FirstKey = Key.A;
        [SerializeField] private Key m_SecondKey = Key.S;
        [SerializeField] private Key m_ThirdKey = Key.K;
        [SerializeField] private Key m_FourthKey = Key.L;
        
        [Header("Arcade Keys")]
        [SerializeField] private string m_FirstArcadeKey = "joystick button 0";
        [SerializeField] private string m_SecondArcadeKey = "joystick button 1";
        [SerializeField] private string m_ThirdArcadeKey = "joystick button 2";
        [SerializeField] private string m_FourthArcadeKey = "joystick button 3";
        
        private Gamepad m_Gamepad;
        private Keyboard m_Keyboard;
        
        private Key[] _keyboardKeys;
        private KeyCode[] _arcadeKeyCodes;
        
        public event Action<int> OnNotePressed;
        public event Action<int> OnNoteReleased;

        protected override void Awake()
        {
            base.Awake();
            
            m_Gamepad = Gamepad.current;
            m_Keyboard = Keyboard.current;
            
            _keyboardKeys = new[]
            {
                m_FirstKey,
                m_SecondKey,
                m_ThirdKey,
                m_FourthKey
            };
            
            // Faz o parse das strings de arcade para KeyCode
            _arcadeKeyCodes = new KeyCode[4];
            _arcadeKeyCodes[0] = ParseArcadeKey(m_FirstArcadeKey, KeyCode.JoystickButton0);
            _arcadeKeyCodes[1] = ParseArcadeKey(m_SecondArcadeKey, KeyCode.JoystickButton1);
            _arcadeKeyCodes[2] = ParseArcadeKey(m_ThirdArcadeKey, KeyCode.JoystickButton2);
            _arcadeKeyCodes[3] = ParseArcadeKey(m_FourthArcadeKey, KeyCode.JoystickButton3);
        }

        private void Update()
        {
            if (m_Gamepad != null)
                CheckGamepadInput();
            
            if (m_Keyboard != null)
                CheckKeyboardInput();
            
            CheckArcadeInput();
        }

        private void CheckGamepadInput()
        {
            // L2 → leftTrigger
            if (m_Gamepad.leftTrigger.wasPressedThisFrame)  
                TriggerNotePressed(0);
            if (m_Gamepad.leftTrigger.wasReleasedThisFrame) 
                TriggerNoteReleased(0);
            
            // L1 → leftShoulder
            if (m_Gamepad.leftShoulder.wasPressedThisFrame)  
                TriggerNotePressed(1);
            if (m_Gamepad.leftShoulder.wasReleasedThisFrame) 
                TriggerNoteReleased(1);
            
            // R1 → rightShoulder
            if (m_Gamepad.rightShoulder.wasPressedThisFrame)  
                TriggerNotePressed(2);
            if (m_Gamepad.rightShoulder.wasReleasedThisFrame) 
                TriggerNoteReleased(2);
            
            // R2 → rightTrigger
            if (m_Gamepad.rightTrigger.wasPressedThisFrame)  
                TriggerNotePressed(3);
            if (m_Gamepad.rightTrigger.wasReleasedThisFrame) 
                TriggerNoteReleased(3);
        }

        private void CheckKeyboardInput()
        {
            for (int i = 0; i < _keyboardKeys.Length; i++)
            {
                if (m_Keyboard[_keyboardKeys[i]].wasPressedThisFrame)
                    TriggerNotePressed(i);
                if (m_Keyboard[_keyboardKeys[i]].wasReleasedThisFrame)
                    TriggerNoteReleased(i);
            }
        }
        
        private void CheckArcadeInput()
        {
            for (int i = 0; i < _arcadeKeyCodes.Length; i++)
            {
                if (Input.GetKeyDown(_arcadeKeyCodes[i]))
                    TriggerNotePressed(i);
                if (Input.GetKeyUp(_arcadeKeyCodes[i]))
                    TriggerNoteReleased(i);
            }
        }

        private KeyCode ParseArcadeKey(string keyName, KeyCode fallback)
        {
            // Remove espaços e ignora maiúsculas/minúsculas
            var cleaned = keyName.Replace(" ", "");
            if (Enum.TryParse(cleaned, true, out KeyCode parsed))
                return parsed;
            Debug.LogWarning($"[InputManager] Não encontrou KeyCode para '{keyName}', usando {fallback}");
            return fallback;
        }

        private void TriggerNotePressed(int lane)
        {
            Debug.Log($"Nota {lane} ativada");
            OnNotePressed?.Invoke(lane);
        }
        
        private void TriggerNoteReleased(int lane)
        {
            Debug.Log($"Nota {lane} liberada");
            OnNoteReleased?.Invoke(lane);
        }
    }
}
