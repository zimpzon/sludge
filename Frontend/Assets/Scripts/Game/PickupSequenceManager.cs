using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Game
{
    public static class PickupSequenceManager
    {
        public static int TotalLetters;
        public static int LettersLeft;
        private static int? currentValue;

        private static List<ModPickupSequence> _pickSequenceMods;

        public static void Reset(List<PickupSequence> pickSequences)
        {
            _pickSequenceMods = pickSequences.Select(x => x.GetComponent<ModPickupSequence>()).ToList();
            _pickSequenceMods = _pickSequenceMods.OrderBy(x => x.SequenceValue).ToList();

            var current = _pickSequenceMods.FirstOrDefault();
            current?.OnBecameActive();
            currentValue = current?.SequenceValue;

            TotalLetters = _pickSequenceMods.Count;
            LettersLeft = TotalLetters;
        }

        public static void OnPickup(ModPickupSequence mod)
        {
            // ensures letters are picked up in order
            // it is valid to have multiple of the same letter, ex more than one 'A'
            if (mod.SequenceValue != currentValue)
                return;

            LettersLeft--;

            mod.OnPickedUp();
            SoundManager.Play(FxList.Instance.TimePillPickup);

            _pickSequenceMods.Remove(mod);

            ModPickupSequence next = _pickSequenceMods.FirstOrDefault();
            next?.OnBecameActive();
            currentValue = next?.SequenceValue;

            //GameManager.I.OnSequenceLetterEaten();
        }
    }
}
