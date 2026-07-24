using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PocketRoguelike.EditorTools
{
    public static class LocalizationSceneMigrator
    {
        private const string ScenePath = "Assets/Scenes/MainGame.unity";
        private const string FontPath = "Assets/TextMesh Pro/Maplestory Bold SDF.asset";

        [MenuItem("Tools/Localization/Apply Korean-English Localization")]
        public static void Apply()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);

            GameObject managers = GameObject.Find("Managers");
            if (managers == null) managers = new GameObject("Managers");
            LanguageManager languageManager = managers.GetComponent<LanguageManager>();
            if (languageManager == null) languageManager = managers.AddComponent<LanguageManager>();

            Transform canvas = GameObject.Find("Canvas")?.transform;
            if (canvas == null) throw new System.InvalidOperationException("Canvas was not found in MainGame scene.");
            EnsureLanguageToggle(canvas, languageManager, font);
            EnsureBattleEnhancementUi(canvas, font);
            EnsureStarterCollectionUi(canvas);

            int removedLocalizationCount = RemoveAllLocalization(scene);

            SetStaticKey(canvas, "StarterSelectPanel/Title", "starter_title");
            SetStaticKey(canvas, "StarterSelectPanel/StartRunButton/Text", "start_run");
            SetStaticKey(canvas, "PartyManageModalPanel/CloseButton/Text", "close");
            SetStaticKey(canvas, "ResultPanel/RestartButton/Text", "play_again");
            SetStaticKey(canvas, "LanguageToggleButton/Text", "language_toggle");

            RemoveDynamicLocalization(canvas, "BattlePanel/BottomPanel/ActionPromptText");
            RemoveDynamicLocalization(canvas, "BattlePanel/TopBar/BossTagText");
            RemoveDynamicLocalization(canvas, "ResultPanel/ResultTitleText");

            int textCount = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
                {
                    if (font != null) text.font = font;
                    EditorUtility.SetDirty(text);
                    textCount++;
                }
            }

            int catCount = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:CatDataSO"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                CatDataSO cat = AssetDatabase.LoadAssetAtPath<CatDataSO>(path);
                if (cat == null) continue;
                if (string.IsNullOrWhiteSpace(cat.catName)) cat.catName = $"Cat #{cat.dexNo}";
                if (string.IsNullOrWhiteSpace(cat.catNameEnglish)) cat.catNameEnglish = cat.catName;
                if (string.IsNullOrWhiteSpace(cat.catNameKorean)) cat.catNameKorean = $"고양이 #{cat.dexNo}";
                EditorUtility.SetDirty(cat);
                catCount++;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new System.InvalidOperationException("Failed to save MainGame scene.");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Localization] Applied to {textCount} TMP texts and {catCount} CatData assets; rebuilt localization components after removing {removedLocalizationCount}.");
        }

        private static void EnsureBattleEnhancementUi(Transform canvas, TMP_FontAsset font)
        {
            Transform topBar = canvas.Find("BattlePanel/TopBar");
            if (topBar == null) throw new System.InvalidOperationException("Battle TopBar is missing.");
            TMP_Text potion = EnsureText(topBar, "PotionCountText", "회복약 x 0", font, 24f, TextAlignmentOptions.Right);
            SetRect(potion.rectTransform, 0.84f, 0.2f, 0.99f, 0.8f);
            potion.color = new Color(0.4f, 1f, 0.55f);

            Transform ball = topBar.Find("BallCountText");
            if (ball != null)
            {
                TMP_Text ballText = ball.GetComponent<TMP_Text>();
                if (ballText != null) ballText.alignment = TextAlignmentOptions.Right;
                SetRect(ball.GetComponent<RectTransform>(), 0.67f, 0.2f, 0.83f, 0.8f);
            }

            Transform bossTag = topBar.Find("BossTagText");
            if (bossTag != null)
            {
                TMP_Text bossText = bossTag.GetComponent<TMP_Text>();
                if (bossText != null) bossText.alignment = TextAlignmentOptions.Left;
                SetRect(bossTag.GetComponent<RectTransform>(), 0.02f, 0.2f, 0.28f, 0.8f);
            }

            Transform battlePanel = canvas.Find("BattlePanel");
            Transform playerView = canvas.Find("BattlePanel/PlayerView");
            Transform enemyView = canvas.Find("BattlePanel/EnemyView");
            if (battlePanel == null || playerView == null || enemyView == null)
                throw new System.InvalidOperationException("Battle participant views are missing.");

            Slider playerSlider = playerView.Find("PlayerHPSlider")?.GetComponent<Slider>();
            Slider enemySlider = enemyView.Find("EnemyHPSlider")?.GetComponent<Slider>();
            if (playerSlider != null) SetRect(playerSlider.GetComponent<RectTransform>(), 0.1f, 0.12f, 0.9f, 0.16f);
            if (enemySlider != null) SetRect(enemySlider.GetComponent<RectTransform>(), 0.1f, 0.12f, 0.9f, 0.16f);

            TMP_Text playerHp = EnsureText(playerView, "PlayerHPText", "HP 0/0 | ATK 0", font, 18f, TextAlignmentOptions.Left);
            SetRect(playerHp.rectTransform, 0.1f, 0.065f, 0.9f, 0.12f);
            TMP_Text playerSkill = EnsureText(playerView, "PlayerSkillText", "스킬", font, 18f, TextAlignmentOptions.Left);
            SetRect(playerSkill.rectTransform, 0.1f, 0.005f, 0.9f, 0.065f);
            TMP_Text enemyHp = EnsureText(enemyView, "EnemyHPText", "HP 0/0 | ATK 0", font, 18f, TextAlignmentOptions.Left);
            SetRect(enemyHp.rectTransform, 0.1f, 0.065f, 0.9f, 0.12f);
            TMP_Text enemySkill = EnsureText(enemyView, "EnemySkillText", "스킬", font, 18f, TextAlignmentOptions.Left);
            SetRect(enemySkill.rectTransform, 0.1f, 0.005f, 0.9f, 0.065f);

            BattleUI battleUi = battlePanel.GetComponent<BattleUI>();
            if (battleUi == null) battleUi = battlePanel.gameObject.AddComponent<BattleUI>();
            SerializedObject serializedBattleUi = new SerializedObject(battleUi);
            serializedBattleUi.FindProperty("playerHpText").objectReferenceValue = playerHp;
            serializedBattleUi.FindProperty("playerSkillText").objectReferenceValue = playerSkill;
            serializedBattleUi.FindProperty("enemyHpText").objectReferenceValue = enemyHp;
            serializedBattleUi.FindProperty("enemySkillText").objectReferenceValue = enemySkill;
            serializedBattleUi.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(battleUi);

            Transform existingPanel = canvas.Find("StageClearPanel");
            GameObject panel = existingPanel != null ? existingPanel.gameObject : new GameObject("StageClearPanel", typeof(RectTransform), typeof(Image));
            if (existingPanel == null) panel.transform.SetParent(canvas, false);
            SetFullStretch(panel.GetComponent<RectTransform>());
            panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.72f);

            Transform existingWindow = panel.transform.Find("Window");
            GameObject window = existingWindow != null ? existingWindow.gameObject : new GameObject("Window", typeof(RectTransform), typeof(Image));
            if (existingWindow == null) window.transform.SetParent(panel.transform, false);
            SetRect(window.GetComponent<RectTransform>(), 0.27f, 0.24f, 0.73f, 0.76f);
            window.GetComponent<Image>().color = new Color(0.09f, 0.13f, 0.2f, 0.98f);

            TMP_Text title = EnsureText(window.transform, "Title", "스테이지 클리어!", font, 42f, TextAlignmentOptions.Center);
            SetRect(title.rectTransform, 0.08f, 0.72f, 0.92f, 0.94f);
            title.color = new Color(1f, 0.85f, 0.2f);
            TMP_Text description = EnsureText(window.transform, "Description", "적을 쓰러뜨렸습니다.", font, 27f, TextAlignmentOptions.Center);
            SetRect(description.rectTransform, 0.08f, 0.50f, 0.92f, 0.70f);
            TMP_Text reward = EnsureText(window.transform, "Reward", "획득한 아이템이 없습니다.", font, 25f, TextAlignmentOptions.Center);
            SetRect(reward.rectTransform, 0.08f, 0.30f, 0.92f, 0.49f);
            reward.color = new Color(0.45f, 1f, 0.55f);

            Transform existingButton = window.transform.Find("ConfirmButton");
            GameObject buttonObject = existingButton != null ? existingButton.gameObject : new GameObject("ConfirmButton", typeof(RectTransform), typeof(Image), typeof(Button));
            if (existingButton == null) buttonObject.transform.SetParent(window.transform, false);
            SetRect(buttonObject.GetComponent<RectTransform>(), 0.25f, 0.08f, 0.75f, 0.24f);
            buttonObject.GetComponent<Image>().color = new Color(0.2f, 0.62f, 0.92f);
            TMP_Text buttonText = EnsureText(buttonObject.transform, "Text", "확인 / 다음 스테이지", font, 24f, TextAlignmentOptions.Center);
            SetFullStretch(buttonText.rectTransform);

            if (panel.GetComponent<StageClearUI>() == null) panel.AddComponent<StageClearUI>();
            panel.SetActive(false);
            Transform languageButton = canvas.Find("LanguageToggleButton");
            if (languageButton != null) languageButton.SetAsLastSibling();
        }

        private static void EnsureStarterCollectionUi(Transform canvas)
        {
            Transform starterPanel = canvas.Find("StarterSelectPanel");
            if (starterPanel == null) throw new System.InvalidOperationException("StarterSelectPanel is missing.");

            Transform scrollTransform = starterPanel.Find("StarterScrollView");
            GameObject scrollObject = scrollTransform != null ? scrollTransform.gameObject : new GameObject("StarterScrollView", typeof(RectTransform), typeof(ScrollRect));
            if (scrollTransform == null) scrollObject.transform.SetParent(starterPanel, false);
            SetRect(scrollObject.GetComponent<RectTransform>(), 0.05f, 0.18f, 0.95f, 0.70f);

            Transform viewportTransform = scrollObject.transform.Find("Viewport");
            GameObject viewportObject = viewportTransform != null ? viewportTransform.gameObject : new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            if (viewportTransform == null) viewportObject.transform.SetParent(scrollObject.transform, false);
            RectTransform viewportRect = viewportObject.GetComponent<RectTransform>();
            SetFullStretch(viewportRect);
            Image viewportImage = viewportObject.GetComponent<Image>();
            viewportImage.color = new Color(0f, 0f, 0f, 0.01f);

            Transform contentTransform = viewportObject.transform.Find("Content");
            GameObject contentObject = contentTransform != null ? contentTransform.gameObject : new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
            if (contentTransform == null) contentObject.transform.SetParent(viewportObject.transform, false);
            RectTransform contentRect = contentObject.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = Vector2.zero;

            GridLayoutGroup grid = contentObject.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(230f, 280f);
            grid.spacing = new Vector2(20f, 20f);
            grid.padding = new RectOffset(10, 10, 10, 10);
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 6;
            ContentSizeFitter fitter = contentObject.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect scroll = scrollObject.GetComponent<ScrollRect>();
            scroll.viewport = viewportRect;
            scroll.content = contentRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 35f;

            StarterSelectUI starterUi = starterPanel.GetComponent<StarterSelectUI>();
            if (starterUi == null) starterUi = starterPanel.gameObject.AddComponent<StarterSelectUI>();
            SerializedObject serializedUi = new SerializedObject(starterUi);
            serializedUi.FindProperty("slotsContainer").objectReferenceValue = contentObject.transform;
            serializedUi.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(starterUi);
        }

        private static TMP_Text EnsureText(Transform parent, string name, string value, TMP_FontAsset font, float size, TextAlignmentOptions alignment)
        {
            Transform existing = parent.Find(name);
            GameObject go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            if (existing == null) go.transform.SetParent(parent, false);
            TMP_Text text = go.GetComponent<TMP_Text>();
            if (text == null) text = go.AddComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = Color.white;
            if (font != null) text.font = font;
            return text;
        }

        private static void SetFullStretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetRect(RectTransform rect, float minX, float minY, float maxX, float maxY)
        {
            rect.anchorMin = new Vector2(minX, minY);
            rect.anchorMax = new Vector2(maxX, maxY);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static int RemoveAllLocalization(Scene scene)
        {
            int removed = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                LocalizedText[] components = root.GetComponentsInChildren<LocalizedText>(true);
                foreach (LocalizedText component in components)
                {
                    Object.DestroyImmediate(component, true);
                    removed++;
                }
            }
            return removed;
        }

        private static void EnsureLanguageToggle(Transform canvas, LanguageManager manager, TMP_FontAsset font)
        {
            Transform existing = canvas.Find("LanguageToggleButton");
            GameObject go = existing != null ? existing.gameObject : new GameObject("LanguageToggleButton", typeof(RectTransform), typeof(Image), typeof(Button));
            if (existing == null) go.transform.SetParent(canvas, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.01f, 0.01f);
            rect.anchorMax = new Vector2(0.105f, 0.055f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image image = go.GetComponent<Image>();
            if (image == null) image = go.AddComponent<Image>();
            image.color = new Color(0.12f, 0.18f, 0.28f, 0.92f);
            Button button = go.GetComponent<Button>();
            if (button == null) button = go.AddComponent<Button>();

            Transform labelTransform = go.transform.Find("Text");
            GameObject labelObject = labelTransform != null ? labelTransform.gameObject : new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            if (labelTransform == null) labelObject.transform.SetParent(go.transform, false);
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            TMP_Text label = labelObject.GetComponent<TMP_Text>();
            label.text = "한국어 / ENG";
            label.fontSize = 18f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            if (font != null) label.font = font;

            button.onClick = new Button.ButtonClickedEvent();
            UnityEventTools.AddPersistentListener(button.onClick, manager.ToggleLanguage);
            go.transform.SetAsLastSibling();
            EditorUtility.SetDirty(button);
        }

        private static void SetStaticKey(Transform canvas, string path, string key)
        {
            Transform target = canvas.Find(path);
            if (target == null) throw new System.InvalidOperationException($"Localization target missing: Canvas/{path}");
            TMP_Text text = target.GetComponent<TMP_Text>();
            if (text == null) throw new System.InvalidOperationException($"TMP_Text missing: Canvas/{path}");
            LocalizedText[] components = target.GetComponents<LocalizedText>();
            LocalizedText localized = components.Length > 0 ? components[0] : target.gameObject.AddComponent<LocalizedText>();
            for (int i = 1; i < components.Length; i++) Object.DestroyImmediate(components[i], true);
            localized.SetKey(key);
            EditorUtility.SetDirty(localized);
        }

        private static void RemoveDynamicLocalization(Transform canvas, string path)
        {
            Transform target = canvas.Find(path);
            if (target == null) return;
            LocalizedText localized = target.GetComponent<LocalizedText>();
            if (localized != null) Object.DestroyImmediate(localized, true);
        }
    }
}
