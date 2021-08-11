using Sludge.Modifiers;
using Sludge.Utility;
using UnityEngine;

namespace Sludge.SludgeObjects
{
    public abstract class SludgeObject : MonoBehaviour
    {
        public SludgeModifier[] Modifiers;
        public abstract EntityType EntityType { get; }

        void Awake()
        {
            Modifiers = transform.GetComponentsInChildren<SludgeModifier>();
        }

        public virtual void Reset()
        {
            if (Modifiers == null)
                return;

            for (int i = 0; i < Modifiers.Length; ++i)
                Modifiers[i].Reset();
        }

        public void EngineTick()
        {
            if (Modifiers == null)
                return;

            for (int i = 0; i < Modifiers.Length; ++i)
                Modifiers[i].EngineTick();
        }
    }
}
