using UnityEditor;

namespace PocketRoguelike.EditorTools
{
    public static class CatDataAutoGenerator
    {
        [MenuItem("Tools/Generate 300 PDF Cat Data ScriptableObjects")]
        public static void Generate300CatData()
        {
            CatEncyclopediaImporter.Apply();
        }

        // Compatibility entry point retained for older scene-builder calls.
        public static void Generate100CatData()
        {
            Generate300CatData();
        }
    }
}
