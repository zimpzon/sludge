using UnityEngine;

namespace Assets.Scripts.Game
{
    public enum MutatorJumpType { SingleJump, WallJump, DoubleJump, TripleJump, QuadJump, ForeverJump }

    public enum MutatorTypePlayerSize { DefaultMe, MiniMe, MegaMe }

    public static class MutatorUtil
    {
        public static void OnPickupJumpType(ModMutatorJumpType m)
        {
            GameManager.I.Player.StateParam.jumpType = m.JumpType;
            ParticleEmitter.I.EmitDust(m.transform.position, 10);
            ParticleEmitter.I.EmitPills(m.transform.position, 4);
            FeelTools.SpawnMutatorFloatingText(m.DisplayText, m.transform.position + Vector3.up * 0.5f);
            SoundManager.Play(FxList.Instance.KeyPickup);
        }

        public static int GetJumpCount(MutatorJumpType m)
        {
            return 1; // double jump
        }
    }
}
