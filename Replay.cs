using System;

namespace Sludge.Replays
{
	public class Replay
	{
		public ReplayElement[] = new ReplayElement[30000];
		public int Count = 0;

		public Clear()
		{
			Count = 0;
		}

		public void SaveState(PlayerInput playerInput)
        {

        }

		public string ToReplayString()
        {
			if (Count == 0)
				return string.Empty;
        }

		public void FromString(string replayString)
        {

        }
	}

	public struct ReplayElement
	{
		public byte Controls;
		public int FrameCountDelta;
	}
}