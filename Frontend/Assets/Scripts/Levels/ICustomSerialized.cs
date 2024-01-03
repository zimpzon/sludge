namespace Assets.Scripts.Levels
{
    public interface ICustomSerialized
    {
        public string SerializeCustomData();
        public void DeserializeCustomData(string customData);
    }
}
