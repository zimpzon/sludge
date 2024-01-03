using UnityEngine;

namespace Sludge.Modifiers
{
    public abstract class SludgeModifier : MonoBehaviour
    {
        public abstract void EngineTick();
        public virtual void Reset() { }

        /// <summary>
        /// Called immediately after the level is loaded
        /// </summary>
        public virtual void OnLoaded() { }
    }
}
