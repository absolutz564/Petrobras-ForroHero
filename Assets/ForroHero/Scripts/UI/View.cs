using UnityEngine;

namespace ForroHero.Scripts.UI
{
    public class View : MonoBehaviour
    {
        protected virtual void Awake() { }
        
        public virtual void OnShow() { }

        public virtual void OnHide() { }

        public virtual void OnInputKeyDown() { }
        
        public virtual void OnInputKeyUp() { }
    }
}
