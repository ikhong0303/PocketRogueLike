using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PocketRoguelike.EditorTools
{
    public class CatSpriteRenamerWindow : EditorWindow
    {
        [SerializeField] private Texture2D[] spriteSheets = new Texture2D[4];
        [SerializeField] private string[] catNames = new string[100];

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
            // Auto-load 4 textures from Assets/Image/Cats or Assets/cats if fields are null
            if (spriteSheets == null || spriteSheets.Length != 4 || spriteSheets[0] == null)
            {
                spriteSheets = new Texture2D[4];
                string[] searchFolders = new string[] { "Assets/Image/Cats", "Assets/cats", "Assets/Cats" };
                List<string> foundPaths = new List<string>();

                foreach (string folder in searchFolders)
                {
                    if (Directory.Exists(folder))
                    {
                        string[] files = Directory.GetFiles(folder, "*.png")
                            .OrderBy(f => f)
                            .ToArray();
                        foundPaths.AddRange(files);
                    }
                }

                for (int i = 0; i < Mathf.Min(4, foundPaths.Count); i++)
                {
                    spriteSheets[i] = AssetDatabase.LoadAssetAtPath<Texture2D>(foundPaths[i]);
                }
            }

            // Init 100 names if empty
            if (catNames == null || catNames.Length != 100)
            {
                catNames = new string[100];
                GenerateSequentialNames(namePrefix);
            }
        }

        private void GenerateSequentialNames(string prefix)
        {
            for (int i = 0; i < 100; i++)
            {
                catNames[i] = $"{prefix}{i + 1}";
            }
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("🐱 Cat Sprite Renamer (100 Cats Batch Naming)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("100개의 고양이 스프라이트 이름을 1부터 100까지 배열로 지정하여 4장의 스프라이트 시트 내 서브 스프라이트 네이밍을 일괄 변경합니다.", MessageType.Info);

            GUILayout.Space(10);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            // 1. Texture Fields
            EditorGUILayout.LabelField("1. Target Sprite Sheets (4 Sheets)", EditorStyles.boldLabel);
            if (spriteSheets == null || spriteSheets.Length != 4)
            {
                spriteSheets = new Texture2D[4];
            }

            for (int i = 0; i < 4; i++)
            {
                spriteSheets[i] = (Texture2D)EditorGUILayout.ObjectField($"Sheet {i + 1} (Cats {i * 25 + 1}~{(i + 1) * 25})", spriteSheets[i], typeof(Texture2D), false);
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
            if (GUILayout.Button("Generate Prefix (cat_1 ~ cat_100)", GUILayout.Width(200)))
            {
                GenerateSequentialNames(namePrefix);
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);
            EditorGUILayout.LabelField("Bulk Paste Names (줄바꿈으로 구분된 100개 이름 일괄 붙여넣기):");
            bulkPasteText = EditorGUILayout.TextArea(bulkPasteText, GUILayout.Height(60));
            if (GUILayout.Button("Apply Bulk Pasted Names to 100 Array"))
            {
                if (!string.IsNullOrWhiteSpace(bulkPasteText))
                {
                    string[] lines = bulkPasteText.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < Mathf.Min(100, lines.Length); i++)
                    {
                        catNames[i] = lines[i].Trim();
                    }
                    Debug.Log($"[CatSpriteRenamer] Applied {lines.Length} pasted names to catNames array.");
                }
            }

            GUILayout.Space(15);

            // 3. 100 Names Array List
            showNameArray = EditorGUILayout.Foldout(showNameArray, $"3. Cat Names Array (Size: {catNames.Length})");
            if (showNameArray)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < 100; i++)
                {
                    catNames[i] = EditorGUILayout.TextField($"[{i + 1}] Cat Name", catNames[i]);
                }
                EditorGUI.indentLevel--;
            }

            GUILayout.Space(20);

            // 4. Action Button
            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.4f);
            if (GUILayout.Button("🚀 Rename All 100 Sprites Across 4 Sheets", GUILayout.Height(40)))
            {
                ApplySpriteNaming();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndScrollView();
        }

        public void ApplySpriteNaming()
        {
            if (spriteSheets == null || spriteSheets.Length == 0)
            {
                EditorUtility.DisplayDialog("Error", "스프라이트 시트 텍스처를 지정해주세요.", "OK");
                return;
            }

            int globalIndex = 0;
            int updatedTexturesCount = 0;
            int totalSubSpritesCount = 0;

            for (int t = 0; t < spriteSheets.Length; t++)
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
