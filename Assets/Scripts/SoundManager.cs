using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PocketRoguelike
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("Audio Settings")]
        [SerializeField] private AudioSource audioSource;
        [Range(0f, 1f)] [SerializeField] private float volume = 1f;
        [SerializeField] private bool loop = true;

        [Header("BGM Tracks (5 Slots)")]
        [SerializeField] private AudioClip[] bgmClips = new AudioClip[5];

        [Header("Current Status")]
        [SerializeField] private int currentBgmIndex = 0;

        public AudioSource Source => audioSource;
        public AudioClip[] BgmClips => bgmClips;
        public int CurrentBgmIndex => currentBgmIndex;
        public float Volume
        {
            get => volume;
            set
            {
                volume = Mathf.Clamp01(value);
                if (audioSource != null)
                {
                    audioSource.volume = volume;
                }
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            InitAudioSource();
        }

        private void Reset()
        {
            InitAudioSource();
        }

        private void OnValidate()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            if (audioSource != null)
            {
                audioSource.volume = volume;
                audioSource.loop = loop;
            }

            if (bgmClips == null || bgmClips.Length != 5)
            {
                System.Array.Resize(ref bgmClips, 5);
            }
        }

        private void InitAudioSource()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }
            audioSource.playOnAwake = false;
            audioSource.loop = loop;
            audioSource.volume = volume;
        }

        /// <summary>
        /// Play BGM by index (0 ~ 4)
        /// </summary>
        public void PlayBGM(int index)
        {
            if (bgmClips == null || bgmClips.Length == 0)
            {
                Debug.LogWarning("[SoundManager] No BGM clips assigned.");
                return;
            }

            if (index < 0 || index >= bgmClips.Length)
            {
                Debug.LogWarning($"[SoundManager] Index {index} is out of bounds (0 ~ {bgmClips.Length - 1}).");
                return;
            }

            AudioClip selectedClip = bgmClips[index];
            if (selectedClip == null)
            {
                Debug.LogWarning($"[SoundManager] BGM slot {index} is empty.");
                return;
            }

            currentBgmIndex = index;
            InitAudioSource();
            audioSource.clip = selectedClip;
            audioSource.Play();
            Debug.Log($"[SoundManager] Playing BGM [{index}]: {selectedClip.name}");
        }

        /// <summary>
        /// Play BGM by clip name
        /// </summary>
        public void PlayBGMByName(string clipName)
        {
            if (bgmClips == null) return;

            for (int i = 0; i < bgmClips.Length; i++)
            {
                if (bgmClips[i] != null && bgmClips[i].name.Equals(clipName, System.StringComparison.OrdinalIgnoreCase))
                {
                    PlayBGM(i);
                    return;
                }
            }

            Debug.LogWarning($"[SoundManager] BGM with name '{clipName}' not found.");
        }

        /// <summary>
        /// Stop current BGM
        /// </summary>
        public void StopBGM()
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
                Debug.Log("[SoundManager] BGM Stopped.");
            }
        }

        /// <summary>
        /// Set specific BGM clip to slot (0 ~ 4)
        /// </summary>
        public void SetBgmClip(int index, AudioClip clip)
        {
            if (bgmClips == null || bgmClips.Length != 5)
            {
                System.Array.Resize(ref bgmClips, 5);
            }

            if (index >= 0 && index < 5)
            {
                bgmClips[index] = clip;
            }
        }
    }
}
