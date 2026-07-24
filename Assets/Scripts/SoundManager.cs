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
        [SerializeField] private bool playOnStart = true;

        [Header("BGM Tracks (5 Slots)")]
        [SerializeField] private AudioClip[] bgmClips = new AudioClip[5];

        [Header("Combat SFX")]
        [SerializeField] private AudioClip attackSfxClip;
        [SerializeField] private AudioClip hurtSfxClip;
        [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;

        [Header("Current Status")]
        [SerializeField] private int currentBgmIndex = 0;

        public AudioSource Source => audioSource;
        public AudioClip[] BgmClips => bgmClips;
        public AudioClip AttackSfxClip => attackSfxClip;
        public AudioClip HurtSfxClip => hurtSfxClip;
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

        private void Start()
        {
            if (playOnStart)
            {
                PlayBGM(currentBgmIndex);
            }
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

            if (bgmClips == null || bgmClips.Length != 5)
            {
                System.Array.Resize(ref bgmClips, 5);
            }

            if (audioSource != null)
            {
                audioSource.volume = volume;
                audioSource.loop = loop;

                // Sync current clip to AudioSource field if empty
                if (audioSource.clip == null && bgmClips != null && currentBgmIndex >= 0 && currentBgmIndex < bgmClips.Length)
                {
                    audioSource.clip = bgmClips[currentBgmIndex];
                }
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

            audioSource.loop = loop;
            audioSource.volume = volume;

            if (bgmClips != null && currentBgmIndex >= 0 && currentBgmIndex < bgmClips.Length && bgmClips[currentBgmIndex] != null)
            {
                audioSource.clip = bgmClips[currentBgmIndex];
            }
        }

        /// <summary>
        /// Play BGM by index (0 ~ 4)
        /// </summary>
        [ContextMenu("Play Current BGM")]
        public void PlayCurrentBGM()
        {
            PlayBGM(currentBgmIndex);
        }

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
        /// Play Next BGM (Cycle 0~4)
        /// </summary>
        [ContextMenu("Play Next BGM")]
        public void PlayNextBGM()
        {
            int nextIndex = (currentBgmIndex + 1) % bgmClips.Length;
            PlayBGM(nextIndex);
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
        [ContextMenu("Stop BGM")]
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
                if (index == currentBgmIndex && audioSource != null)
                {
                    audioSource.clip = clip;
                }
            }
        }

        public void ConfigureGameAudio(AudioClip bgm, AudioClip attackSfx, AudioClip hurtSfx)
        {
            SetBgmClip(0, bgm);
            currentBgmIndex = 0;
            attackSfxClip = attackSfx;
            hurtSfxClip = hurtSfx;
            loop = true;
            playOnStart = true;
            InitAudioSource();
            audioSource.clip = bgm;
            audioSource.loop = true;
            audioSource.playOnAwake = false;
        }

        public void PlayAttackSfx()
        {
            PlayOneShot(attackSfxClip, "Attack", "slap");
        }

        public void PlayHurtSfx()
        {
            PlayOneShot(hurtSfxClip, "Hurt", "ouch");
        }

        private void PlayOneShot(AudioClip clip, string label, string expectedName)
        {
            if (clip == null)
            {
                Debug.LogWarning($"[SoundManager] {label} SFX is missing (expected {expectedName}).");
                return;
            }

            InitAudioSource();
            audioSource.PlayOneShot(clip, sfxVolume);
            Debug.Log($"[SoundManager] {label} SFX: {clip.name}");
        }
    }
}
