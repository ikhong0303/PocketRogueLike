using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PocketRoguelike.EditorTools
{
    public static class GameAudioSceneConfigurator
    {
        private const string ScenePath = "Assets/Scenes/MainGame.unity";
        private const string BgmPath = "Assets/Sounds/BGM.mp3";
        private const string AttackPath = "Assets/Sounds/slap.mp3";
        private const string HurtPath = "Assets/Sounds/ouch.mp3";
        private const string DrinkPath = "Assets/Sounds/Drink.mp3";
        private const string HealPath = "Assets/Sounds/Heal.mp3";
        private const string CatchPath = "Assets/Sounds/Catch.mp3";

        [MenuItem("Tools/PocketRoguelike/Configure Game Audio")]
        public static void Apply()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != ScenePath)
                throw new InvalidOperationException($"Open {ScenePath} before configuring game audio. Active scene: {scene.path}");

            SoundManager manager = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<SoundManager>(true))
                .FirstOrDefault();
            if (manager == null) throw new InvalidOperationException("SoundManager is missing from MainGame scene.");

            AudioClip bgm = LoadClip(BgmPath);
            AudioClip attack = LoadClip(AttackPath);
            AudioClip hurt = LoadClip(HurtPath);
            AudioClip drink = LoadClip(DrinkPath);
            AudioClip heal = LoadClip(HealPath);
            AudioClip catchClip = LoadClip(CatchPath);
            manager.ConfigureGameAudio(bgm, attack, hurt, drink, heal, catchClip);

            AudioSource source = manager.Source;
            if (source == null) throw new InvalidOperationException("SoundManager AudioSource is missing.");
            source.clip = bgm;
            source.loop = true;
            source.playOnAwake = false;

            EditorUtility.SetDirty(manager);
            EditorUtility.SetDirty(source);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
                throw new InvalidOperationException("Failed to save MainGame audio configuration.");
            AssetDatabase.SaveAssets();

            Validate(manager, bgm, attack, hurt, drink, heal, catchClip);
            Debug.Log("[GameAudioValidation] PASS: BGM loops from BGM.mp3; slap, ouch, Drink, Heal, and Catch are assigned as SFX.");
        }

        private static AudioClip LoadClip(string path)
        {
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip == null) throw new InvalidOperationException($"AudioClip is missing: {path}");
            return clip;
        }

        private static void Validate(SoundManager manager, AudioClip bgm, AudioClip attack, AudioClip hurt, AudioClip drink, AudioClip heal, AudioClip catchClip)
        {
            if (manager.BgmClips == null || manager.BgmClips.Length == 0 || manager.BgmClips[0] != bgm)
                throw new InvalidOperationException("BGM.mp3 is not assigned to BGM slot 0.");
            if (manager.AttackSfxClip != attack) throw new InvalidOperationException("slap.mp3 is not assigned as attack SFX.");
            if (manager.HurtSfxClip != hurt) throw new InvalidOperationException("ouch.mp3 is not assigned as hurt SFX.");
            if (manager.DrinkSfxClip != drink) throw new InvalidOperationException("Drink.mp3 is not assigned as drink SFX.");
            if (manager.HealSfxClip != heal) throw new InvalidOperationException("Heal.mp3 is not assigned as heal SFX.");
            if (manager.CatchSfxClip != catchClip) throw new InvalidOperationException("Catch.mp3 is not assigned as catch SFX.");
            if (manager.Source == null || manager.Source.clip != bgm || !manager.Source.loop)
                throw new InvalidOperationException("BGM AudioSource is not configured for looping playback.");
        }
    }
}
