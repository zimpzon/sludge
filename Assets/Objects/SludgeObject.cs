using Sludge.Modifiers;
using UnityEngine;

namespace Sludge.SludgeObjects
{
    public abstract class SludgeObject : MonoBehaviour
    {
        public SludgeModifier[] Modifiers;
        
        void Awake()
        {
            Modifiers = transform.GetComponents<SludgeModifier>();
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
