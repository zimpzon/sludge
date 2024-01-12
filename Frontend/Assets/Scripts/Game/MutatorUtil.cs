using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Game
{
    public enum MutatorTypeAirJumpCount { SingleJump, DoubleJump, TripleJump, QuadJump, ForeverJump }

    public enum MutatorTypeJumpPower { NoPower, DefaultPower, MegaPower }

    public enum MutatorTypePlayerSize { DefaultMe, MiniMe, MegaMe }

    public static class MutatorUtil
    {
        public static void OnPickupAirJump(ModMutatorAirJump m)
        {
            GameManager.I.Player.StateParam.airJumpCount = m.AirJump;
            ParticleEmitter.I.EmitDust(m.transform.position, 10);
            ParticleEmitter.I.EmitPills(m.transform.position, 4);
            FeelTools.SpawnMutatorFloatingText(m.DisplayText, m.transform.position + Vector3.up * 0.5f);
            SoundManager.Play(FxList.Instance.KeyPickup);
        }

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
