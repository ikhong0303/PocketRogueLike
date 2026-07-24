using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PocketRoguelike.EditorTools
{
    public static class LocalizationValidator
    {
        [MenuItem("Tools/Localization/Validate Localization")]
        public static void Validate()
        {
            Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/MainGame.unity", OpenSceneMode.Single);
            TMP_FontAsset expectedFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Maplestory Bold SDF.asset");
            TMP_Text[] texts = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<TMP_Text>(true)).ToArray();
            Text[] legacyTexts = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Text>(true)).ToArray();
            if (texts.Length != 71) throw new Exception($"Expected 71 TMP texts, found {texts.Length}.");
            if (legacyTexts.Length != 0) throw new Exception($"Expected 0 legacy Text components, found {legacyTexts.Length}.");
            if (texts.Any(text => text.font != expectedFont)) throw new Exception("Not every TMP text uses Maplestory Bold SDF.");

            LocalizedText[] localized = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<LocalizedText>(true)).ToArray();
            string[] expectedKeys = { "starter_title", "start_run", "close", "play_again", "language_toggle" };
            string[] actualKeys = localized.Select(item => item.Key).OrderBy(key => key).ToArray();
            if (localized.Length != expectedKeys.Length || !actualKeys.SequenceEqual(expectedKeys.OrderBy(key => key)))
                throw new Exception($"Static localization keys mismatch: {string.Join(", ", actualKeys)}");

            Transform canvas = GameObject.Find("Canvas")?.transform;
            if (canvas == null || canvas.Find("BattlePanel/TopBar/PotionCountText") == null) throw new Exception("Potion inventory text is missing.");
            GameObject stageClearPanel = canvas.Find("StageClearPanel")?.gameObject;
            if (stageClearPanel == null || stageClearPanel.GetComponent<StageClearUI>() == null) throw new Exception("Stage clear panel is missing.");

            Button languageButton = GameObject.Find("Canvas/LanguageToggleButton")?.GetComponent<Button>();
            if (languageButton == null) throw new Exception("Language toggle button is missing.");
            if (languageButton.onClick.GetPersistentEventCount() != 1 || languageButton.onClick.GetPersistentMethodName(0) != "ToggleLanguage")
                throw new Exception("Language toggle persistent event is not wired exactly once.");

            CatDataSO[] cats = AssetDatabase.FindAssets("t:CatDataSO")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<CatDataSO>)
                .Where(cat => cat != null).ToArray();
            if (cats.Length != 301) throw new Exception($"Expected index 000 dummy plus 300 CatData assets, found {cats.Length}.");
            if (cats.Count(cat => cat.dexNo >= 1 && cat.dexNo <= 300) != 300) throw new Exception("Playable CatData IDs must cover 1 through 300 exactly.");
            if (cats.Any(cat => string.IsNullOrWhiteSpace(cat.catNameKorean) || string.IsNullOrWhiteSpace(cat.catNameEnglish)))
                throw new Exception("At least one CatData asset is missing a localized name.");

            LanguageManager manager = GameObject.Find("Managers")?.GetComponent<LanguageManager>();
            if (manager == null) throw new Exception("LanguageManager is missing.");
            manager.SetLanguage(GameLanguage.Korean);
            AssertEqual("게임 시작", LanguageManager.Get("start_run"), "Korean start button");
            AssertEqual(CatEncyclopediaTable.Get(1).KoreanName, LanguageManager.CatName(cats.First(cat => cat.dexNo == 1)), "Korean cat name");
            AssertEqual(CatEncyclopediaTable.Get(1).PrimarySkillKorean, LanguageManager.SkillName(cats.First(cat => cat.dexNo == 1)), "Korean skill name");
            AssertEqual("기본", LanguageManager.Rarity(CatRarity.Basic), "Korean rarity");
            manager.SetLanguage(GameLanguage.English);
            AssertEqual("START RUN", LanguageManager.Get("start_run"), "English start button");
            AssertEqual($"Cat #1 ({CatEncyclopediaTable.Get(1).KoreanName})", LanguageManager.CatName(cats.First(cat => cat.dexNo == 1)), "English cat name");
            AssertEqual("Basic", LanguageManager.Rarity(CatRarity.Basic), "English rarity");
            manager.SetLanguage(GameLanguage.Korean);

            Debug.Log($"[LocalizationValidation] PASS: {texts.Length} TMP, {legacyTexts.Length} legacy Text, {localized.Length} static keys, 300 localized cats plus index 000 dummy, language button wired.");
        }

        private static void AssertEqual(string expected, string actual, string label)
        {
            if (!string.Equals(expected, actual, StringComparison.Ordinal))
                throw new Exception($"{label}: expected '{expected}', got '{actual}'.");
        }
    }
}
