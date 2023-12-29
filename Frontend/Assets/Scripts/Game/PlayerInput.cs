using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sludge.PlayerInputs
{
    public class PlayerInput
    {
        public enum InputType { Up, Down, Left, Right, Select, Back, ColorNext, ColorPrev };

        Dictionary<InputType, InputState> inputs = new Dictionary<InputType, InputState>();

        public PlayerInput()
        {
            inputs[InputType.Up] = new InputState { IsActivated = UpActive };
            inputs[InputType.Down] = new InputState { IsActivated = DownActive };
            inputs[InputType.Left] = new InputState { IsActivated = LeftActive };
            inputs[InputType.Right] = new InputState { IsActivated = RightActive };
            inputs[InputType.Back] = new InputState { IsActivated = BackActive };
            inputs[InputType.Select] = new InputState { IsActivated = SelectActive };
            inputs[InputType.ColorNext] = new InputState { IsActivated = ColorNextActive };
            inputs[InputType.ColorPrev] = new InputState { IsActivated = ColorPrevActive };
        }

        private class InputState
        {
            public Func<bool> IsActivated;
            public bool WasActive;
            public double TimeLastTap;
            public bool IsTapped;
            public bool IsDoubleTapped;
        }

        public int Up;
        public int Down;
        public int Left;
        public int Right;
        public int Shoot;

        public bool RestartKey() => Input.GetKeyDown(KeyCode.R);
        public bool CustomCheatKey() => Input.GetKeyDown(KeyCode.P);

        public bool UpActive() => Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || Input.GetAxisRaw("Vertical") > 0.5f;
        public bool DownActive() => Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || Input.GetAxisRaw("Vertical") < -0.5f;
        public bool LeftActive() => Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) || Input.GetAxisRaw("Horizontal") < -0.5f;
        public bool RightActive() => Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) || Input.GetAxisRaw("Horizontal") > 0.5f;
        public bool BackActive() => Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.Escape) || Input.GetKey(KeyCode.Backspace);
        public bool SelectActive() => Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter);
        public bool ShootActive() => Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.LeftShift);
        public bool ColorNextActive() => Input.GetKey(KeyCode.X);
        public bool ColorPrevActive() => Input.GetKey(KeyCode.Z);

        public bool IsTapped(InputType inputType, bool claimEvent = true)
        {
            bool result = inputs[inputType].IsTapped;
            if (claimEvent)
                inputs[inputType].IsTapped = false;
            return result;
        }

        private float noInputBefore;

        public void PauseInput(float delay)
        {
            noInputBefore = Time.time + delay;
            ClearState();
        }

        public void GetHumanInput()
        {
            if (Time.time < noInputBefore)
                return;

            // Flags
            Up = UpActive() ? 1 : 0;
            Down = DownActive() ? 2 : 0;
            Left = LeftActive() ? 4 : 0;
            Right = RightActive() ? 8 : 0;
            Shoot = ShootActive() ? 16 : 0;

            const double DoubleTapTime = 0.4;
            foreach(var input in inputs.Values)
            {
                input.IsTapped = false;
                input.IsDoubleTapped = false;

                bool active = input.IsActivated();
                if (active)
                {
                    input.IsTapped = !input.WasActive;
                    if (input.IsTapped)
                    {
                        input.IsDoubleTapped = Time.time - input.TimeLastTap < DoubleTapTime;
                        input.TimeLastTap = Time.time;
                    }
                }

                input.WasActive = active;
            }
        }

        public void ClearState()
        {
            SetState(0);
            foreach (var input in inputs.Values)
            {
                input.IsTapped = false;
                input.IsDoubleTapped = false;
            }
        }

        public void SetState(int state)
        {
            Up = state & 1;
            Down = state & 2;
            Left = state & 4;
            Right = state & 8;
            Shoot = state & 16;
        }

        public int GetState()
            => Up + Down + Left + Right + Shoot;
    }
}
