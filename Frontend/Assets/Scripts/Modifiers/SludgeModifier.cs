using UnityEngine;

namespace Sludge.Modifiers
{
    public abstract class SludgeModifier : MonoBehaviour
    {
        public abstract void EngineTick();
        public virtual void Reset() { }
        public virtual void OnEditorLoad() { }
    }
}
