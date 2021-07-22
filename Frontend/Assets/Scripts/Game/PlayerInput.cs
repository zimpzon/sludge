using UnityEngine;

namespace Sludge.PlayerInputs
{
    public class PlayerInput
    {
        public int Up;
        public int Down;
        public int Left;
        public int Right;

        public bool UpTap;
        public bool DownTap;
        public bool LeftTap;
        public bool RightTap;
        public bool SelectTap;
        public bool BackTap;

        public void GetHumanInput()
        {
            Up = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || Input.GetAxisRaw("Vertical") > 0 ? 1 : 0;
            Down = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.DownArrow) || Input.GetAxisRaw("Vertical") < 0 ? 2 : 0;
            Left = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) || Input.GetAxisRaw("Horizontal") < 0 ? 4 : 0;
            Right = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) || Input.GetAxisRaw("Horizontal") > 0 ? 8 : 0;

            UpTap = Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
            DownTap = Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.DownArrow);
            LeftTap = Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
            RightTap = Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);
            SelectTap = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.E);
            BackTap = Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Backspace);
        }

        public void SetState(int state)
        {
            Up = state & 1;
            Down = state & 2;
            Left = state & 4;
            Right = state & 8;
        }

        public int GetState()
            => Up + Down + Left + Right;
    }
}
