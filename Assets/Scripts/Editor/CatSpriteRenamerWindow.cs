using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PocketRoguelike.EditorTools
{
    public class CatSpriteRenamerWindow : EditorWindow
    {
        [SerializeField] private List<Texture2D> spriteSheets = new List<Texture2D>();
        [SerializeField] private string[] catNames = new string[300];

        private string namePrefix = "cat_";
        private string bulkPasteText = "";
        private Vector2 scrollPos;
        private bool showNameArray = false;

        [MenuItem("Tools/Cat Sprite Renamer")]
        public static void ShowWindow()
        {
            CatSpriteRenamerWindow window = GetWindow<CatSpriteRenamerWindow>("Cat Sprite Renamer");
            window.minSize = new Vector2(450, 600);
            window.InitDefaultValues();
            window.Show();
        }

        private void OnEnable()
        {
            InitDefaultValues();
        }

        private void InitDefaultValues()
        {
            string[] searchFolders = new string[] { "Assets/Image/Cats", "Assets/cats", "Assets/Cats" };
            List<string> foundPaths = new List<string>();

            foreach (string folder in searchFolders)
            {
                if (Directory.Exists(folder))
                {
                    string[] files = Directory.GetFiles(folder, "*.png")
                        .OrderBy(f =>
                        {
                            string filename = Path.GetFileNameWithoutExtension(f);
                            int dashPos = filename.IndexOf('-');
                            if (dashPos > 0 && int.TryParse(filename.Substring(0, dashPos), out int firstId))
                                return firstId;
                            return 9999;
                        })
                        .ToArray();
                    foundPaths.AddRange(files);
                }
            }

            spriteSheets = foundPaths
                .Select(AssetDatabase.LoadAssetAtPath<Texture2D>)
                .Where(t => t != null)
                .ToList();

            int targetCount = Mathf.Max(300, spriteSheets.Count * 25);
            if (catNames == null || catNames.Length != targetCount)
            {
                catNames = new string[targetCount];
                GenerateSequentialNames(namePrefix);
            }
        }

        private void GenerateSequentialNames(string prefix)
        {
            for (int i = 0; i < catNames.Length; i++)
            {
                catNames[i] = $"{prefix}{i + 1}";
            }
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("🐱 Cat Sprite Renamer (Batch Naming Window)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("고양이 스프라이트 시트 내 서브 스프라이트 이름을 커스텀 이름으로 일괄 변경합니다. Prefix 방식 또는 줄바꿈으로 붙여넣어 변경할 수 있습니다.", MessageType.Info);

            GUILayout.Space(10);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            // 1. Texture Fields
            EditorGUILayout.LabelField($"1. Target Sprite Sheets ({spriteSheets.Count} Sheets Detected)", EditorStyles.boldLabel);

            for (int i = 0; i < spriteSheets.Count; i++)
            {
                spriteSheets[i] = (Texture2D)EditorGUILayout.ObjectField($"Sheet {i + 1} ({spriteSheets[i]?.name ?? "Empty"})", spriteSheets[i], typeof(Texture2D), false);
            }

            if (GUILayout.Button("Find Cats Textures Automatically"))
            {
                InitDefaultValues();
            }

            GUILayout.Space(15);

            // 2. Naming Helper & Bulk Paste
            EditorGUILayout.LabelField("2. Sprite Naming Settings", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            namePrefix = EditorGUILayout.TextField("Prefix Format", namePrefix);
            if (GUILayout.Button($"Generate Prefix ({namePrefix}1 ~ {namePrefix}{catNames.Length})", GUILayout.Width(220)))
            {
                GenerateSequentialNames(namePrefix);
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);
            EditorGUILayout.LabelField("Bulk Paste Names (줄바꿈으로 구분된 이름 일괄 붙여넣기):");
            bulkPasteText = EditorGUILayout.TextArea(bulkPasteText, GUILayout.Height(60));
            if (GUILayout.Button($"Apply Bulk Pasted Names to Array (Max {catNames.Length})"))
            {
                if (!string.IsNullOrWhiteSpace(bulkPasteText))
                {
                    string[] lines = bulkPasteText.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < Mathf.Min(catNames.Length, lines.Length); i++)
                    {
                        catNames[i] = lines[i].Trim();
                    }
                    Debug.Log($"[CatSpriteRenamer] Applied {lines.Length} pasted names to catNames array.");
                }
            }

            GUILayout.Space(15);

            // 3. Names Array List
            showNameArray = EditorGUILayout.Foldout(showNameArray, $"3. Cat Names Array (Size: {catNames.Length})");
            if (showNameArray)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < catNames.Length; i++)
                {
                    catNames[i] = EditorGUILayout.TextField($"[{i + 1}] Cat Name", catNames[i]);
                }
                EditorGUI.indentLevel--;
            }

            GUILayout.Space(20);

            // 4. Action Button
            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.4f);
            if (GUILayout.Button($"🚀 Rename All Sprites Across {spriteSheets.Count} Sheets", GUILayout.Height(40)))
            {
                ApplySpriteNaming();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndScrollView();
        }

        public void ApplySpriteNaming()
        {
            if (spriteSheets == null || spriteSheets.Count == 0)
            {
                EditorUtility.DisplayDialog("Error", "스프라이트 시트 텍스처를 지정해주세요.", "OK");
                return;
            }

            int globalIndex = 0;
            int updatedTexturesCount = 0;
            int totalSubSpritesCount = 0;

            for (int t = 0; t < spriteSheets.Count; t++)
            {
                Texture2D texture = spriteSheets[t];
                if (texture == null) continue;

                string path = AssetDatabase.GetAssetPath(texture);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

                if (importer == null)
                {
                    Debug.LogError($"[CatSpriteRenamer] TextureImporter not found for {path}");
                    continue;
                }

                // Ensure sprite mode is Multiple
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Multiple;

#pragma warning disable 0618
                SpriteMetaData[] spritesheet = importer.spritesheet;
                if (spritesheet == null || spritesheet.Length == 0)
                {
                    Debug.LogWarning($"[CatSpriteRenamer] {texture.name} has no sliced sprites in TextureImporter. Please slice it first.");
                    continue;
                }

                // Sort sub-sprites by position: Top-to-Bottom (Y desc), then Left-to-Right (X asc)
                SpriteMetaData[] sortedSprites = spritesheet
                    .OrderByDescending(s => Mathf.RoundToInt(s.rect.y))
                    .ThenBy(s => Mathf.RoundToInt(s.rect.x))
                    .ToArray();

                for (int i = 0; i < sortedSprites.Length; i++)
                {
                    if (globalIndex < catNames.Length)
                    {
                        string newName = string.IsNullOrWhiteSpace(catNames[globalIndex]) 
                            ? $"cat_{globalIndex + 1}" 
                            : catNames[globalIndex];

                        sortedSprites[i].name = newName;
                        globalIndex++;
                        totalSubSpritesCount++;
                    }
                }

                importer.spritesheet = sortedSprites;
#pragma warning restore 0618
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
                updatedTexturesCount++;

                Debug.Log($"[CatSpriteRenamer] Updated {texture.name} with {sortedSprites.Length} renamed sprites.");
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Complete", $"성공적으로 {updatedTexturesCount}개의 시트 내 {totalSubSpritesCount}개 스프라이트 이름을 변경하였습니다!", "OK");
        }
    }
}
