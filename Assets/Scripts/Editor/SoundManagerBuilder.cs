using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PocketRoguelike.EditorTools
{
    public static class SoundManagerBuilder
    {
        [MenuItem("Tools/Build SoundManager Scene")]
        public static void BuildSoundManagerScene()
        {
            Debug.Log("[SoundManagerBuilder] Building Soundmanager scene via Unity CLI / Editor script...");

            // 1. Create a new scene
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // 2. Create empty GameObject named "soundmanager"
            GameObject soundManagerGO = new GameObject("soundmanager");
            Undo.RegisterCreatedObjectUndo(soundManagerGO, "Create soundmanager GameObject");

            // 3. Attach AudioSource and SoundManager components
            AudioSource audioSource = soundManagerGO.AddComponent<AudioSource>();
            SoundManager soundManager = soundManagerGO.AddComponent<SoundManager>();

            // 4. Find the 5 audio files in Assets/Sounds
            string soundsFolderPath = "Assets/Sounds";
            string[] soundGuids = AssetDatabase.FindAssets("t:AudioClip", new[] { soundsFolderPath });
            
            Debug.Log($"[SoundManagerBuilder] Found {soundGuids.Length} AudioClips in {soundsFolderPath}.");

            AudioClip[] loadedClips = new AudioClip[5];
            int count = 0;
            foreach (string guid in soundGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
                if (clip != null && count < 5)
                {
                    loadedClips[count] = clip;
                    soundManager.SetBgmClip(count, clip);
                    Debug.Log($"[SoundManagerBuilder] Assigned Slot [{count}]: {clip.name} ({assetPath})");
                    count++;
                }
            }

            // Save serialized changes
            EditorUtility.SetDirty(soundManagerGO);

            // 5. Ensure Assets/Scenes directory exists
            string scenesDirectory = "Assets/Scenes";
            if (!Directory.Exists(scenesDirectory))
            {
                Directory.CreateDirectory(scenesDirectory);
                AssetDatabase.Refresh();
            }

            // Save scene as Assets/Scenes/Soundmanager.unity
            string scenePath = Path.Combine(scenesDirectory, "Soundmanager.unity");
            bool saved = EditorSceneManager.SaveScene(newScene, scenePath);

            if (saved)
            {
                Debug.Log($"[SoundManagerBuilder] Successfully saved scene to: {scenePath}");
            }
            else
            {
                Debug.LogError($"[SoundManagerBuilder] Failed to save scene to: {scenePath}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
