using UnityEngine;

namespace ForroHero.Scripts.Utils
{
    public class ForceFullscreen : MonoBehaviour
    {
        void Awake()
        {
            Screen.SetResolution(1080, 1920, true);
        }
    }
}
