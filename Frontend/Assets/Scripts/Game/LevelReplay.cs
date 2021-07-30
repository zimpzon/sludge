using System;
using System.Text;

namespace Sludge.Replays
{
	public class LevelReplay
	{
		public ReplayElement[] Elements = new ReplayElement[30000];
		public int Count = 0;
		int lastInputState;
		StringBuilder sb = new StringBuilder(5000);

		int replayIndex;

		public void BeginRecording()
		{
			Count = 0;
			lastInputState = -1;
		}

		public void BeginReplay()
        {
			replayIndex = -1;
        }

		public bool HasReplay()
			=> Count > 0;

		public bool ReplayIsDone()
			=> replayIndex >= Count - 1;

		public int GetReplayState(int frameCounter)
        {
			bool hasMoreStates = replayIndex < Count - 1;
			bool timeForNextState = frameCounter == Elements[replayIndex + 1].FrameCounter;
			if (hasMoreStates && timeForNextState)
            {
				replayIndex++;
				//CheckErrors();
			}

			return Elements[replayIndex].InputState;
		}

		//public void CheckErrors()
  //      {
		//	float errorAngle = (float)GameManager.Instance.Player.angle - Elements[replayIndex].Angle;
		//	float errorX = GameManager.Instance.Player.transform.position.x - Elements[replayIndex].PlayerX;
		//	float errorY = GameManager.Instance.Player.transform.position.y - Elements[replayIndex].PlayerY;
		//	//GameManager.SetStatusText($"errorAngle: {errorAngle:0.00000000}, errorX: {errorX:0.00000000}, errorY: {errorY:0.00000000}");
		//}

		public void RecordState(int inputState, int frameCounter)
		{
			if (inputState == lastInputState)
				return;

			Elements[Count].InputState = (byte)inputState;
			Elements[Count].FrameCounter = frameCounter;

			//Elements[Count].Angle = (float)GameManager.Instance.Player.angle;
			//Elements[Count].PlayerX = GameManager.Instance.Player.transform.position.x;
			//Elements[Count].PlayerY = GameManager.Instance.Player.transform.position.y;

			lastInputState = inputState;
			Count++;
		}

		public string ToReplayString()
		{
			if (Count == 0)
				return string.Empty;

			sb.Clear();

			for (int i = 0; i < Count; ++i)
            {
				sb.Append(Elements[i].InputState.ToString());
				sb.Append(',');
				sb.Append(Elements[i].FrameCounter.ToString());
				sb.Append(',');
			}
			return sb.ToString();
		}

		public void FromString(string replayString)
		{
			Count = 0;
			var list = replayString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < list.Length; i += 2)
            {
				int state = int.Parse(list[i + 0]);
				int frameDelta = int.Parse(list[i + 1]);
				Elements[i / 2].InputState = (byte)state;
				Elements[i / 2].FrameCounter = frameDelta;
				Count++;
			}
		}
	}

	public struct ReplayElement
	{
		public byte InputState;
		public int FrameCounter;
		//public float Angle;
		//public float PlayerX;
		//public float PlayerY;
	}
}