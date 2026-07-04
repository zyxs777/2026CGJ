using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace KeepBallMoving
{
    internal static class KeepBallNewInput
    {
        private enum GamepadButton
        {
            Primary,
            Phantom
        }

        private enum ButtonReadMode
        {
            Down,
            Up
        }

        private static readonly KeyCode[] LegacyPrimaryButtons =
        {
            KeyCode.Joystick1Button0,
            KeyCode.Joystick2Button0,
            KeyCode.Joystick3Button0,
            KeyCode.Joystick4Button0,
            KeyCode.Joystick5Button0,
            KeyCode.Joystick6Button0,
            KeyCode.Joystick7Button0,
            KeyCode.Joystick8Button0
        };

        private static readonly KeyCode[] LegacyPhantomButtons =
        {
            KeyCode.Joystick1Button1,
            KeyCode.Joystick2Button1,
            KeyCode.Joystick3Button1,
            KeyCode.Joystick4Button1,
            KeyCode.Joystick5Button1,
            KeyCode.Joystick6Button1,
            KeyCode.Joystick7Button1,
            KeyCode.Joystick8Button1
        };

        public static int GamepadCount
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return GetPlayableDeviceCount();
#else
                return 0;
#endif
            }
        }

        public static bool IsPrimaryDown(int gamepadIndex, bool anyGamepad)
        {
            return ReadButton(gamepadIndex, anyGamepad, GamepadButton.Primary, ButtonReadMode.Down) ||
                ReadRewiredButton(gamepadIndex, anyGamepad, GamepadButton.Primary, ButtonReadMode.Down);
        }

        public static bool IsPrimaryUp(int gamepadIndex, bool anyGamepad)
        {
            return ReadButton(gamepadIndex, anyGamepad, GamepadButton.Primary, ButtonReadMode.Up) ||
                ReadRewiredButton(gamepadIndex, anyGamepad, GamepadButton.Primary, ButtonReadMode.Up);
        }

        public static bool IsPhantomDown(int gamepadIndex, bool anyGamepad)
        {
            return ReadButton(gamepadIndex, anyGamepad, GamepadButton.Phantom, ButtonReadMode.Down) ||
                ReadRewiredButton(gamepadIndex, anyGamepad, GamepadButton.Phantom, ButtonReadMode.Down);
        }

        public static bool IsLegacyPrimaryDown(int gamepadIndex, bool anyGamepad)
        {
            return ReadLegacyButton(gamepadIndex, anyGamepad, GamepadButton.Primary, ButtonReadMode.Down);
        }

        public static bool IsLegacyPrimaryUp(int gamepadIndex, bool anyGamepad)
        {
            return ReadLegacyButton(gamepadIndex, anyGamepad, GamepadButton.Primary, ButtonReadMode.Up);
        }

        public static bool IsLegacyPhantomDown(int gamepadIndex, bool anyGamepad)
        {
            return ReadLegacyButton(gamepadIndex, anyGamepad, GamepadButton.Phantom, ButtonReadMode.Down);
        }

        public static float GetHorizontal(int gamepadIndex, bool anyGamepad)
        {
#if ENABLE_INPUT_SYSTEM
            if (anyGamepad)
            {
                float strongestAxis = 0f;
                int deviceCount = GetPlayableDeviceCount();
                for (int i = 0; i < deviceCount; i++)
                {
                    float axis = ReadHorizontal(GetPlayableDevice(i));
                    if (Mathf.Abs(axis) > Mathf.Abs(strongestAxis))
                    {
                        strongestAxis = axis;
                    }
                }

                return strongestAxis;
            }

            return ReadHorizontal(GetPlayableDevice(gamepadIndex));
#else
            return 0f;
#endif
        }

        public static string GetDebugText()
        {
#if ENABLE_INPUT_SYSTEM
            System.Text.StringBuilder builder = new System.Text.StringBuilder(512);
            builder.AppendLine($"New Input System: ON / Devices: {GetPlayableDeviceCount()}");
            builder.AppendLine($"Gamepad.all: {Gamepad.all.Count} / Joystick.all: {Joystick.all.Count}");

            int deviceCount = GetPlayableDeviceCount();
            for (int i = 0; i < deviceCount; i++)
            {
                InputDevice device = GetPlayableDevice(i);
                if (device == null)
                {
                    continue;
                }

                ButtonControl primary = GetButton(device, GamepadButton.Primary);
                ButtonControl phantom = GetButton(device, GamepadButton.Phantom);
                builder.AppendLine($"{i + 1}. {device.layout} | {device.displayName} | {device.name}");
                builder.AppendLine($"   A:{FormatButton(primary)}  B:{FormatButton(phantom)}  X:{ReadHorizontal(device):0.00}");
            }

            builder.Append(GetRewiredDebugText());
            return builder.ToString();
#else
            return $"New Input System: OFF\n{GetRewiredDebugText()}";
#endif
        }

        public static string GetLegacyDebugText()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder(256);
            string[] joystickNames = Input.GetJoystickNames();
            builder.AppendLine($"Legacy Input Joysticks: {joystickNames.Length}");
            for (int i = 0; i < joystickNames.Length; i++)
            {
                builder.AppendLine($"{i + 1}. {joystickNames[i]}");
            }

            builder.AppendLine($"Any A:{FormatLegacyButton(Input.GetKey(KeyCode.JoystickButton0))}  Any B:{FormatLegacyButton(Input.GetKey(KeyCode.JoystickButton1))}");
            builder.AppendLine($"PVP legacy slots: 蓝方={FormatLegacySlot(GetLegacyJoystickSlotForPlayer(0))} / 红方={FormatLegacySlot(GetLegacyJoystickSlotForPlayer(1))}");
            for (int i = 0; i < LegacyPrimaryButtons.Length; i++)
            {
                builder.AppendLine($"J{i + 1} A:{FormatLegacyButton(Input.GetKey(LegacyPrimaryButtons[i]))}  B:{FormatLegacyButton(Input.GetKey(LegacyPhantomButtons[i]))}");
            }

            return builder.ToString();
        }

        public static bool HasNewInputDevices()
        {
            return GamepadCount > 0;
        }

        public static bool HasSeparatedInputDevices()
        {
            return HasNewInputDevices() || GetRewiredJoystickCount() > 0;
        }

        private static bool ReadRewiredButton(int gamepadIndex, bool anyGamepad, GamepadButton button, ButtonReadMode mode)
        {
            if (!IsRewiredReady())
            {
                return false;
            }

            int buttonIndex = button == GamepadButton.Primary ? 0 : 1;
            if (anyGamepad)
            {
                foreach (Rewired.Joystick joystick in Rewired.ReInput.controllers.Joysticks)
                {
                    if (ReadRewiredButton(joystick, buttonIndex, mode))
                    {
                        return true;
                    }
                }

                return false;
            }

            Rewired.Joystick target = GetRewiredJoystick(gamepadIndex);
            return ReadRewiredButton(target, buttonIndex, mode);
        }

        private static bool ReadRewiredButton(Rewired.Joystick joystick, int buttonIndex, ButtonReadMode mode)
        {
            if (joystick == null || buttonIndex < 0 || buttonIndex >= joystick.buttonCount)
            {
                return false;
            }

            return mode == ButtonReadMode.Down ? joystick.GetButtonDown(buttonIndex) : joystick.GetButtonUp(buttonIndex);
        }

        private static Rewired.Joystick GetRewiredJoystick(int joystickIndex)
        {
            if (!IsRewiredReady() || joystickIndex < 0)
            {
                return null;
            }

            System.Collections.Generic.IList<Rewired.Joystick> joysticks = Rewired.ReInput.controllers.Joysticks;
            return joystickIndex < joysticks.Count ? joysticks[joystickIndex] : null;
        }

        private static int GetRewiredJoystickCount()
        {
            if (!IsRewiredReady())
            {
                return 0;
            }

            return Rewired.ReInput.controllers.joystickCount;
        }

        private static bool IsRewiredReady()
        {
            try
            {
                return Rewired.ReInput.isReady;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        private static string GetRewiredDebugText()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder(256);
            if (!IsRewiredReady())
            {
                builder.AppendLine("Rewired: not ready");
                return builder.ToString();
            }

            System.Collections.Generic.IList<Rewired.Joystick> joysticks = Rewired.ReInput.controllers.Joysticks;
            builder.AppendLine($"Rewired Joysticks: {joysticks.Count}");
            for (int i = 0; i < joysticks.Count; i++)
            {
                Rewired.Joystick joystick = joysticks[i];
                bool primary = joystick.buttonCount > 0 && joystick.GetButton(0);
                bool phantom = joystick.buttonCount > 1 && joystick.GetButton(1);
                builder.AppendLine($"{i + 1}. {joystick.name} | {joystick.hardwareName}");
                builder.AppendLine($"   A0:{FormatLegacyButton(primary)}  B1:{FormatLegacyButton(phantom)}  buttons:{joystick.buttonCount}");
            }

            return builder.ToString();
        }

        private static string FormatLegacyButton(bool isPressed)
        {
            return isPressed ? "DOWN" : "up";
        }

        private static string FormatLegacySlot(int slot)
        {
            return slot >= 0 ? $"J{slot + 1}" : "none";
        }

        private static bool ReadLegacyButton(int gamepadIndex, bool anyGamepad, GamepadButton button, ButtonReadMode mode)
        {
            if (anyGamepad)
            {
                KeyCode anyKey = button == GamepadButton.Primary ? KeyCode.JoystickButton0 : KeyCode.JoystickButton1;
                return mode == ButtonReadMode.Down ? Input.GetKeyDown(anyKey) : Input.GetKeyUp(anyKey);
            }

            int slot = GetLegacyJoystickSlotForPlayer(gamepadIndex);
            if (slot < 0)
            {
                return false;
            }

            KeyCode[] keys = button == GamepadButton.Primary ? LegacyPrimaryButtons : LegacyPhantomButtons;
            return mode == ButtonReadMode.Down ? Input.GetKeyDown(keys[slot]) : Input.GetKeyUp(keys[slot]);
        }

        private static int GetLegacyJoystickSlotForPlayer(int playerIndex)
        {
            if (playerIndex < 0)
            {
                return -1;
            }

            string[] joystickNames = Input.GetJoystickNames();
            int namedSlotIndex = 0;
            int maxSlots = Mathf.Min(joystickNames.Length, LegacyPrimaryButtons.Length);
            for (int i = 0; i < maxSlots; i++)
            {
                if (string.IsNullOrEmpty(joystickNames[i]))
                {
                    continue;
                }

                if (namedSlotIndex == playerIndex)
                {
                    return i;
                }

                namedSlotIndex++;
            }

            return -1;
        }

        private static bool ReadButton(int gamepadIndex, bool anyGamepad, GamepadButton button, ButtonReadMode mode)
        {
#if ENABLE_INPUT_SYSTEM
            if (anyGamepad)
            {
                int deviceCount = GetPlayableDeviceCount();
                for (int i = 0; i < deviceCount; i++)
                {
                    if (ReadButton(GetPlayableDevice(i), button, mode))
                    {
                        return true;
                    }
                }

                return false;
            }

            return ReadButton(GetPlayableDevice(gamepadIndex), button, mode);
#else
            return false;
#endif
        }

#if ENABLE_INPUT_SYSTEM
        private static int GetPlayableDeviceCount()
        {
            int count = Gamepad.all.Count;
            foreach (Joystick joystick in Joystick.all)
            {
                count++;
            }

            return count;
        }

        private static InputDevice GetPlayableDevice(int deviceIndex)
        {
            if (deviceIndex < 0)
            {
                return null;
            }

            if (deviceIndex < Gamepad.all.Count)
            {
                return Gamepad.all[deviceIndex];
            }

            int joystickIndex = deviceIndex - Gamepad.all.Count;
            foreach (Joystick joystick in Joystick.all)
            {
                if (joystickIndex == 0)
                {
                    return joystick;
                }

                joystickIndex--;
            }

            return null;
        }

        private static bool ReadButton(InputDevice device, GamepadButton button, ButtonReadMode mode)
        {
            ButtonControl control = GetButton(device, button);
            if (control == null)
            {
                return false;
            }

            return mode == ButtonReadMode.Down ? control.wasPressedThisFrame : control.wasReleasedThisFrame;
        }

        private static ButtonControl GetButton(InputDevice device, GamepadButton button)
        {
            if (device == null)
            {
                return null;
            }

            Gamepad gamepad = device as Gamepad;
            if (gamepad != null)
            {
                return button == GamepadButton.Primary ? gamepad.buttonSouth : gamepad.buttonEast;
            }

            string[] names = button == GamepadButton.Primary
                ? new[] { "trigger", "buttonSouth", "button0" }
                : new[] { "buttonEast", "button1", "button2" };
            for (int i = 0; i < names.Length; i++)
            {
                ButtonControl control = device.TryGetChildControl<ButtonControl>(names[i]);
                if (control != null)
                {
                    return control;
                }
            }

            return GetFallbackButton(device, button == GamepadButton.Primary ? 0 : 1);
        }

        private static ButtonControl GetFallbackButton(InputDevice device, int buttonIndex)
        {
            int index = 0;
            foreach (InputControl control in device.allControls)
            {
                ButtonControl buttonControl = control as ButtonControl;
                if (buttonControl == null || buttonControl.synthetic)
                {
                    continue;
                }

                if (index == buttonIndex)
                {
                    return buttonControl;
                }

                index++;
            }

            return null;
        }

        private static string FormatButton(ButtonControl control)
        {
            if (control == null)
            {
                return "none";
            }

            return $"{control.name} {(control.isPressed ? "DOWN" : "up")}";
        }

        private static float ReadHorizontal(InputDevice device)
        {
            if (device == null)
            {
                return 0f;
            }

            Gamepad gamepad = device as Gamepad;
            if (gamepad != null)
            {
                float gamepadAxis = gamepad.leftStick.x.ReadValue();
                if (Mathf.Abs(gamepadAxis) >= 0.55f)
                {
                    return gamepadAxis;
                }

                return gamepad.rightStick.x.ReadValue();
            }

            Joystick joystick = device as Joystick;
            if (joystick == null || joystick.stick == null)
            {
                return 0f;
            }

            float axis = joystick.stick.x.ReadValue();
            if (Mathf.Abs(axis) >= 0.55f)
            {
                return axis;
            }

            return 0f;
        }
#endif
    }
}
