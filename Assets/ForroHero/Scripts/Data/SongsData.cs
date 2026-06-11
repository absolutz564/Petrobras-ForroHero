using System.Collections.Generic;
using UnityEngine;

namespace ForroHero.Scripts.Data
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "SongsData", menuName = "ScriptableObjects/SongsData")]
    public class SongsData : ScriptableObject
    {
        [Header("Config")]
        public List<SongData> Songs;
    }
}