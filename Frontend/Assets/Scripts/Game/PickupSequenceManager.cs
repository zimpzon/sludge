using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Game
{
    public static class PickupSequenceManager
    {
        public static List<PickupSequence> _pickSequences; 

        public static void Reset(List<PickupSequence> pickSequences)
        {
            _pickSequences = pickSequences;
        }

        public static void OnPickup(ModPickupSequence mod)
        {
            mod.SetPickupActive(false);

            Debug.Log(mod.SequenceValue);

            SoundManager.Play(FxList.Instance.TimePillPickup);
        }
    }
}
