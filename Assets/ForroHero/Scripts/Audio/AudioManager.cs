using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ForroHero.Scripts.Data;
using ForroHero.Scripts.Managers;
using ForroHero.Scripts.Utils;
using UnityEngine;
using UnityEngine.Audio;

namespace ForroHero.Scripts.Audio
{
    public class AudioManager : SingletonBehaviour<AudioManager>
    {
        [Header("Config")]
        [SerializeField] private AudioMixer m_Mixer;
        [SerializeField] private Transform m_StorageArea;
        
        public Transform StorageArea => m_StorageArea;
        private List<AudioClass> BGMs => GameManager.Instance.AudioData.BGMs;
        private List<AudioClass> SFXs => GameManager.Instance.AudioData.SFXs;
        
        private Stack<GameObject> m_GOAudioSourcePool;
        private List<AudioSource> m_AudioSourceActive;

        private AudioMixerGroup m_MasterMixer;
        private AudioMixerGroup m_BGMMixer;
        private AudioMixerGroup m_SFXMixer;
        
        protected override void Awake()
        {
            base.Awake();
            
            m_GOAudioSourcePool = new Stack<GameObject>();
            m_AudioSourceActive = new List<AudioSource>();
            
            SetMixers();
        }
        
        private void SetMixers()
        {
            m_MasterMixer = m_Mixer.FindMatchingGroups("Master").FirstOrDefault();
            m_BGMMixer = m_Mixer.FindMatchingGroups("BGM").FirstOrDefault();
            m_SFXMixer = m_Mixer.FindMatchingGroups("SFX").FirstOrDefault();
        }
        
        public void PlayBGM(string audioTag, float volume = 1, float pitch = 1, bool loop = true)
        {
            AudioClip clip = SearchClip(audioTag, BGMs);
            StartCoroutine(PlayAudioClipCo(StorageArea.gameObject, clip, audioTag, m_BGMMixer, volume, pitch, loop));
        }
        
        public void PlaySFX(string audioTag, float volume = 1, float pitch = 1)
        {
            AudioClip clip = SearchClip(audioTag, SFXs);
            StartCoroutine(PlayAudioOneShotCo(StorageArea.gameObject, clip, audioTag, m_SFXMixer, volume, pitch));
        }
        
        private AudioClip SearchClip(string audioTag, List<AudioClass> lList)
        {
            for (int i = 0; i < lList.Count(); i++)
            {
                if (lList[i].Tag.Equals(audioTag))
                    return lList[i].Clip;
            }

            Debug.Log("Sound Não Encontrado, Verifique a nome!!");
            return null;
        }
        
        private IEnumerator PlayAudioClipCo(GameObject lMyGO, AudioClip lClip, string audioTag, AudioMixerGroup lOutput, float lVolume, float lPitch, bool loop)
        {
            if (!lClip)
            {
                Debug.LogError("[UI Audio]: Could not play null audio clip.");
                yield break;
            }

            if (!lMyGO.GetComponent<MoveStorageArea>())
                lMyGO.AddComponent<MoveStorageArea>();
            
            if (m_GOAudioSourcePool.Count == 0)
                InstantiateAudioSource(audioTag);
            
            GameObject lAudioObj = m_GOAudioSourcePool.Pop();
            
            lAudioObj.transform.SetParent(lMyGO.transform);
            
            AudioSource lAudioSource = lAudioObj.GetComponent<AudioSource>();
            
            lAudioSource.clip = lClip;
            lAudioSource.outputAudioMixerGroup = lOutput;
            lAudioSource.volume = lVolume;
            lAudioSource.pitch = lPitch;
            lAudioSource.loop = loop;

            yield return null;

            lAudioSource.enabled = true;
            lAudioSource.Play();
            
            m_AudioSourceActive.Add(lAudioSource);
            StartCoroutine(MovePoolCo(lAudioSource));
        }
        
        private IEnumerator PlayAudioOneShotCo(GameObject lMyGO, AudioClip lClip, string audioTag, AudioMixerGroup lOutput, float lVolume, float lPitch)
        {
            if (!lClip)
            {
                Debug.LogError("[UI Audio]: Could not play null audio clip.");
                yield break;
            }

            if (!lMyGO.GetComponent<MoveStorageArea>())
                lMyGO.AddComponent<MoveStorageArea>();

            if (m_GOAudioSourcePool.Count == 0)
                InstantiateAudioSource(audioTag);

            GameObject lAudioObj = m_GOAudioSourcePool.Pop();
            lAudioObj.transform.SetParent(lMyGO.transform);
            
            AudioSource lAudioSource = lAudioObj.GetComponent<AudioSource>();

            lAudioSource.outputAudioMixerGroup = lOutput;
            lAudioSource.volume = lVolume;
            lAudioSource.pitch = lPitch;
            lAudioSource.loop = false;

            yield return null;

            lAudioSource.enabled = true;
            lAudioSource.PlayOneShot(lClip);

            m_AudioSourceActive.Add(lAudioSource);
            StartCoroutine(MovePoolCo(lAudioSource));
        }
        
        private void InstantiateAudioSource(string audioTag)
        {
            GameObject lGO = new GameObject("AudioSource [" + audioTag + "]");
            AudioSource lAudioSource = lGO.AddComponent<AudioSource>();          
            
            lAudioSource.clip = null;
            lAudioSource.outputAudioMixerGroup = null;
            lAudioSource.mute = false;
            lAudioSource.bypassEffects = false;
            lAudioSource.bypassListenerEffects = false;
            lAudioSource.bypassReverbZones = false;
            lAudioSource.playOnAwake = false;
            lAudioSource.loop = false;
            lAudioSource.priority = 128;
            lAudioSource.volume = 1f;
            lAudioSource.pitch = 1f;
            lAudioSource.panStereo = 0f;
            lAudioSource.spatialBlend = 0f;
            lAudioSource.reverbZoneMix = 1f;

            lAudioSource.enabled = false;
            m_GOAudioSourcePool.Push(lGO);
        }
        
        private IEnumerator MovePoolCo(AudioSource lAudioSource)
        {
            yield return new WaitUntil(() => !lAudioSource.isPlaying);
            m_AudioSourceActive.Remove(lAudioSource);
            m_GOAudioSourcePool.Push(lAudioSource.gameObject);
        }
        
        public void StopAllAudio()
        {
            for (int i = 0; i < m_AudioSourceActive.Count; i++)
            {
                if (!m_AudioSourceActive[i])
                {
                    continue;
                }

                m_AudioSourceActive[i].Stop();
                StartCoroutine(MovePoolCo(m_AudioSourceActive[i]));
            }
           
            m_AudioSourceActive.Clear();
        }
    }
}
