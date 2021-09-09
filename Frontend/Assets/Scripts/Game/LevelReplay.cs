using System;
using System.Text;

namespace Sludge.Replays
{
	public class LevelReplay
	{
		public ReplayElement[] Elements = new ReplayElement[5000];
		public ReplayElement[] ElementsRecording = new ReplayElement[5000];
		public int Count = 0;
		public int CountRecording = 0;
		int lastInputState;
		StringBuilder sb = new StringBuilder(5000);
		string recordingUniqueId;
		string committedUniqueId;

		int replayIndex;

		public void BeginRecording(string uniqueId)
		{
			CountRecording = 0;
			lastInputState = -1;
			recordingUniqueId = uniqueId;
		}

		public void BeginReplay()
        {
			replayIndex = -1;
        }

		public bool HasReplay(string uniqueId)
			=> Count > 0 && committedUniqueId == uniqueId;

		public bool ReplayIsDone()
			=> replayIndex >= Count - 1;

		public void CommitReplay()
        {
			Array.Copy(ElementsRecording, Elements, CountRecording);
			Count = CountRecording;
			committedUniqueId = recordingUniqueId;
        }

		public int GetReplayState(int frameCounter)
        {
			bool hasMoreStates = replayIndex < Count - 1;
			bool timeForNextState = frameCounter == Elements[replayIndex + 1].FrameCounter;
			if (hasMoreStates && timeForNextState)
            {
				replayIndex++;
			}

			return Elements[replayIndex].InputState;
		}

		public void RecordState(int inputState, int frameCounter)
		{
			if (inputState == lastInputState)
				return;

			ElementsRecording[CountRecording].InputState = (byte)inputState;
			ElementsRecording[CountRecording].FrameCounter = frameCounter;

			lastInputState = inputState;
			CountRecording++;
		}

		public string LatestCommittedToReplayString() => ToReplayString(Elements, Count);

		public string LatestUncommittedToReplayString() => ToReplayString(ElementsRecording, CountRecording);

		string ToReplayString(ReplayElement[] data, int count)
		{
			if (Count == 0)
				return string.Empty;

			sb.Clear();

			for (int i = 0; i < count; ++i)
            {
				sb.Append(data[i].InputState.ToString());
				sb.Append(',');
				sb.Append(data[i].FrameCounter.ToString());
				sb.Append(',');
			}
			return sb.ToString();
		}

		public void FromString(string replayString, string levelUniqueId)
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
			committedUniqueId = levelUniqueId;
		}
	}

	public struct ReplayElement
	{
		public byte InputState;
		public int FrameCounter;
	}
}