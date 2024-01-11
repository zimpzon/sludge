namespace Assets.Scripts.Game
{
    public enum MutatorTypeAirJumpCount { SingleJump, DoubleJump, TripleJump, QuadJump, ForeverJump }

    public enum MutatorTypeJumpPower { NoPower, DefaultPower, MegaPower }

    public enum MutatorTypePlayerSize { DefaultMe, MiniMe, MegaMe }

    public static class MutatorUtil
    {
        public static int GetJumpCount(MutatorTypeAirJumpCount m)
        {
            switch (m)
            {
                case MutatorTypeAirJumpCount.SingleJump:
                    return 0;
                case MutatorTypeAirJumpCount.DoubleJump:
                    return 1;
                case MutatorTypeAirJumpCount.TripleJump:
                    return 2;
                case MutatorTypeAirJumpCount.QuadJump:
                    return 3;
                case MutatorTypeAirJumpCount.ForeverJump:
                    return -1;
                default:
                    return 1;
            }
        }
    }
}
