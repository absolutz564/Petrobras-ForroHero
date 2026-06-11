using UnityEngine;

namespace ForroHero.Scripts.Audio
{
    public class MoveStorageArea : MonoBehaviour
    {
        private void OnDestroy()
        {
            AudioSource[] temp = GetComponentsInChildren<AudioSource>();

            for (int i = 0; i < temp.Length; i++)
            {
                temp[i].Stop();
                temp[i].transform.parent = AudioManager.Instance.StorageArea;
            }
        }
    }
}
