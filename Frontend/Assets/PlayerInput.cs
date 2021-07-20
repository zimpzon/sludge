using UnityEngine;

namespace Sludge.PlayerInputs
{
    public class PlayerInput
    {
        public int Up;
        public int Down;
        public int Left;
        public int Right;

        public void GetHumanInput()
        {
            Up = Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetAxisRaw("Vertical") > 0 ? 1 : 0;
            Down = Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetAxisRaw("Vertical") < 0 ? 2 : 0;
            Left = Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetAxisRaw("Horizontal") < 0 ? 4 : 0;
            Right = Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetAxisRaw("Horizontal") > 0 ? 8 : 0;
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
