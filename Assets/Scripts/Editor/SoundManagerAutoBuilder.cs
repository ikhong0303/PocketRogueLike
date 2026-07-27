using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PocketRoguelike.EditorTools
{
    public static class SoundManagerAutoBuilder
    {
        [MenuItem("Tools/Build SoundManager Scene Auto")]
        public static void BuildSoundManagerScene()
        {
            string scenePath = "Assets/Scenes/Soundmanager.unity";
            
            Debug.Log("[SoundManagerAutoBuilder] Starting Soundmanager scene creation...");

            // Ensure Assets/Scenes directory exists
            if (!Directory.Exists("Assets/Scenes"))
            {
                Directory.CreateDirectory("Assets/Scenes");
                AssetDatabase.Refresh();
            }

            // Create a new empty scene
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Create empty GameObject named "soundmanager"
            GameObject soundManagerGO = new GameObject("soundmanager");

            // Attach components
            AudioSource audioSource = soundManagerGO.AddComponent<AudioSource>();
            SoundManager soundManager = soundManagerGO.AddComponent<SoundManager>();

            // Find all audio clips in Assets/Sounds
            string soundsFolderPath = "Assets/Sounds";
            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { soundsFolderPath });
            
            Debug.Log($"[SoundManagerAutoBuilder] Found {guids.Length} AudioClips in {soundsFolderPath}.");

            int index = 0;
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip != null && index < 5)
                {
                    soundManager.SetBgmClip(index, clip);
                    Debug.Log($"[SoundManagerAutoBuilder] Assigned BGM [{index}]: {clip.name} ({path})");
                    index++;
                }
            }

            EditorUtility.SetDirty(soundManagerGO);

            // Save Scene
            bool saved = EditorSceneManager.SaveScene(newScene, scenePath);
            if (saved)
            {
                Debug.Log($"[SoundManagerAutoBuilder] Scene successfully saved to {scenePath}");
            }
            else
            {
                Debug.LogError($"[SoundManagerAutoBuilder] Failed to save scene to {scenePath}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
