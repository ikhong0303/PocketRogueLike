using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PocketRoguelike.EditorTools
{
    public class CatSpriteMappingWindow : EditorWindow
    {
        private Sprite[] catSprites = new Sprite[300];
        private bool[] foldouts = new bool[12];
        private Vector2 scrollPos;

        [MenuItem("Tools/PocketRoguelike/Cat Sprite Direct Mapping Window")]
        [MenuItem("Tools/Cat Sprite Direct Mapping Window")]
        public static void ShowWindow()
        {
            CatSpriteMappingWindow window = GetWindow<CatSpriteMappingWindow>("Cat Sprite Mapping");
            window.minSize = new Vector2(500, 700);
            window.LoadFromCatData();
            window.Show();
        }

        private void OnEnable()
        {
            for (int i = 0; i < foldouts.Length; i++)
            {
                foldouts[i] = true; // Default expanded
            }
            LoadFromCatData();
        }

        public void LoadFromCatData()
        {
            for (int id = 1; id <= 300; id++)
            {
                string assetPath = $"Assets/Resources/CatData/CatData_{id}.asset";
                CatDataSO data = AssetDatabase.LoadAssetAtPath<CatDataSO>(assetPath);
                if (data != null && data.sprite != null)
                {
                    catSprites[id - 1] = data.sprite;
                }
            }
        }

        public void AutoFillFromSheets()
        {
            string folder = "Assets/Image/Cats";
            if (!Directory.Exists(folder)) return;

            string[] sheetFiles = Directory.GetFiles(folder, "*.png")
                .OrderBy(f =>
                {
                    string filename = Path.GetFileNameWithoutExtension(f);
                    int dashPos = filename.IndexOf('-');
                    if (dashPos > 0 && int.TryParse(filename.Substring(0, dashPos), out int firstId))
                        return firstId;
                    return 9999;
                })
                .ToArray();

            List<Sprite> allLoadedSprites = new List<Sprite>();
            foreach (string sheetPath in sheetFiles)
            {
                Sprite[] spritesInSheet = AssetDatabase.LoadAllAssetsAtPath(sheetPath)
                    .OfType<Sprite>()
                    .OrderByDescending(s => Mathf.RoundToInt(s.rect.y))
                    .ThenBy(s => Mathf.RoundToInt(s.rect.x))
                    .ToArray();
                allLoadedSprites.AddRange(spritesInSheet);
            }

            Dictionary<string, Sprite> spriteByName = new Dictionary<string, Sprite>();
            foreach (Sprite s in allLoadedSprites)
            {
                if (!spriteByName.ContainsKey(s.name))
                {
                    spriteByName[s.name] = s;
                }
            }

            int matchedByName = 0;
            for (int id = 1; id <= 300; id++)
            {
                string targetName = $"cat_{id}";
                if (spriteByName.TryGetValue(targetName, out Sprite matchedSprite))
                {
                    catSprites[id - 1] = matchedSprite;
                    matchedByName++;
                }
                else if (id - 1 < allLoadedSprites.Count)
                {
                    catSprites[id - 1] = allLoadedSprites[id - 1];
                }
            }

            Debug.Log($"[CatSpriteMappingWindow] Auto-filled {catSprites.Length} slots. Matched {matchedByName} sprites by exact 'cat_ID' name.");
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("🐱 고양이 스프라이트 300개 직관 배치 & 매핑 윈도우", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "1번부터 300번까지의 고양이 스프라이트를 아래 그룹별 슬롯에 자유롭게 드래그 앤 드롭하여 배치하세요.\n" +
                "배치가 끝난 후 하단의 [🚀 300개 스프라이트 CatData에 일괄 적용하기] 버튼을 누르면 모든 CatData 자산에 반영됩니다.",
                MessageType.Info);

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("📥 현재 CatData에서 불러오기", GUILayout.Height(30)))
            {
                LoadFromCatData();
            }
            if (GUILayout.Button("⚡ 스프라이트 시트 순서대로 자동 채우기", GUILayout.Height(30)))
            {
                AutoFillFromSheets();
            }
            if (GUILayout.Button("🗑️ 슬롯 전체 비우기", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("경고", "모든 스프라이트 슬롯을 비우시겠습니까?", "예", "아니오"))
                {
                    catSprites = new Sprite[300];
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(15);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            for (int section = 0; section < 12; section++)
            {
                int startId = section * 25 + 1;
                int endId = (section + 1) * 25;

                foldouts[section] = EditorGUILayout.Foldout(foldouts[section], $"Cats {startId:D3} ~ {endId:D3} ({section + 1}/12 Sheet)", true, EditorStyles.foldoutHeader);

                if (foldouts[section])
                {
                    EditorGUI.indentLevel++;
                    for (int id = startId; id <= endId; id++)
                    {
                        int index = id - 1;
                        EditorGUILayout.BeginHorizontal();

                        string nameText = $"Cat #{id:D3}";
                        if (CatEncyclopediaTable.Entries.Count >= id)
                        {
                            var entry = CatEncyclopediaTable.Get(id);
                            if (!string.IsNullOrEmpty(entry.KoreanName))
                            {
                                nameText = $"[{id:D3}] {entry.KoreanName}";
                            }
                        }

                        EditorGUILayout.LabelField(nameText, GUILayout.Width(180));
                        catSprites[index] = (Sprite)EditorGUILayout.ObjectField(catSprites[index], typeof(Sprite), false, GUILayout.Width(220));

                        // Preview thumbnail
                        if (catSprites[index] != null && catSprites[index].texture != null)
                        {
                            Texture2D t = catSprites[index].texture;
                            Rect r = catSprites[index].rect;
                            Rect texCoords = new Rect(r.x / t.width, r.y / t.height, r.width / t.width, r.height / t.height);
                            Rect displayRect = GUILayoutUtility.GetRect(24, 24, GUILayout.Width(24), GUILayout.Height(24));
                            GUI.DrawTextureWithTexCoords(displayRect, t, texCoords);
                        }
                        else
                        {
                            GUILayout.Label("(없음)", GUILayout.Width(50));
                        }

                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUI.indentLevel--;
                    GUILayout.Space(5);
                }
            }

            EditorGUILayout.EndScrollView();

            GUILayout.Space(10);
            GUI.backgroundColor = new Color(0.2f, 0.8f, 0.4f);
            if (GUILayout.Button("🚀 300개 스프라이트 CatData에 일괄 적용하기", GUILayout.Height(45)))
            {
                ApplySpritesToCatData();
            }
            GUI.backgroundColor = Color.white;
            GUILayout.Space(10);
        }

        public void ApplySpritesToCatData()
        {
            int updatedCount = 0;
            List<CatDataSO> cats = new List<CatDataSO>();

            // Dummy Cat 000
            CatDataSO dummy = AssetDatabase.LoadAssetAtPath<CatDataSO>("Assets/Resources/CatData/CatData_000.asset");
            if (dummy != null) cats.Add(dummy);

            for (int id = 1; id <= 300; id++)
            {
                string assetPath = $"Assets/Resources/CatData/CatData_{id}.asset";
                CatDataSO data = AssetDatabase.LoadAssetAtPath<CatDataSO>(assetPath);
                if (data != null)
                {
                    Sprite sprite = catSprites[id - 1];
                    if (sprite != null)
                    {
                        data.sprite = sprite;
                        EditorUtility.SetDirty(data);
                        updatedCount++;
                    }
                    cats.Add(data);
                }
            }

            CatDatabaseSO database = AssetDatabase.LoadAssetAtPath<CatDatabaseSO>("Assets/Resources/CatDatabase.asset");
            if (database != null)
            {
                database.SetCats(cats);
                EditorUtility.SetDirty(database);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            try
            {
                CatEncyclopediaImporter.Validate();
                EditorUtility.DisplayDialog("성공", $"총 {updatedCount}개 고양이 데이터에 스프라이트 할당을 완료하였으며 검증을 통과했습니다!", "확인");
            }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog("알림", $"스프라이트가 저장되었습니다. (검증 메시지: {ex.Message})", "확인");
            }
        }
    }
}
