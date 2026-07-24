using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PocketRoguelike.EditorTools
{
    public static class MainGameSceneBuilder
    {
        [MenuItem("Tools/Build MainGame Scene")]
        public static void BuildMainGameScene()
        {
            Debug.Log("[MainGameSceneBuilder] Building complete MainGame.unity scene...");

            // 1. Generate all 300 PDF-backed CatData assets first
            CatDataAutoGenerator.Generate300CatData();

            // 2. Create new scene
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 3. Create EventSystem
            GameObject esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<InputSystemUIInputModule>();

            // 4. Create Main Camera
            GameObject camGO = new GameObject("Main Camera");
            Camera cam = camGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.14f, 0.18f); // Dark background
            camGO.AddComponent<AudioListener>();

            // 5. Create Core Managers GameObject
            GameObject managersGO = new GameObject("Managers");
            GameManager gameMgr = managersGO.AddComponent<GameManager>();
            StageManager stageMgr = managersGO.AddComponent<StageManager>();
            PartyManager partyMgr = managersGO.AddComponent<PartyManager>();
            BattleManager battleMgr = managersGO.AddComponent<BattleManager>();
            CatchManager catchMgr = managersGO.AddComponent<CatchManager>();
            SoundManager soundMgr = managersGO.AddComponent<SoundManager>();
            soundMgr.ConfigureGameAudio(
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/BGM.mp3"),
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/slap.mp3"),
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/ouch.mp3"));
            UIManager uiMgr = managersGO.AddComponent<UIManager>();
            LanguageManager languageMgr = managersGO.AddComponent<LanguageManager>();

            // Connect Database to GameManager
            CatDatabaseSO catDb = AssetDatabase.LoadAssetAtPath<CatDatabaseSO>("Assets/Resources/CatDatabase.asset");
            if (catDb != null)
            {
                gameMgr.SetCatDatabase(catDb);
            }

            // 6. Create Canvas & Panels
            GameObject canvasGO = new GameObject("Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGO.AddComponent<GraphicRaycaster>();

            // --- Panel 1: BattlePanel ---
            GameObject battlePanel = CreateUIObject("BattlePanel", canvasGO.transform);
            RectTransform battleRect = battlePanel.GetComponent<RectTransform>();
            SetFullStretch(battleRect);

            // Top Bar: Stage & Boss Tag
            GameObject topBar = CreateUIObject("TopBar", battlePanel.transform);
            SetRect(topBar.GetComponent<RectTransform>(), 0f, 0.9f, 1f, 1f);
            TMP_Text stageTxt = CreateTMPText("StageText", topBar.transform, "STAGE 1 / 100", 36, TextAlignmentOptions.Center);
            SetFullStretch(stageTxt.GetComponent<RectTransform>());

            TMP_Text ballCountTxt = CreateTMPText("BallCountText", topBar.transform, "포켓볼 x 5", 26, TextAlignmentOptions.Right);
            ballCountTxt.color = new Color(0.4f, 0.85f, 1f);
            SetRect(ballCountTxt.GetComponent<RectTransform>(), 0.67f, 0.2f, 0.83f, 0.8f);

            TMP_Text potionCountTxt = CreateTMPText("PotionCountText", topBar.transform, "회복약 x 0", 24, TextAlignmentOptions.Right);
            potionCountTxt.color = new Color(0.4f, 1f, 0.55f);
            SetRect(potionCountTxt.GetComponent<RectTransform>(), 0.84f, 0.2f, 0.99f, 0.8f);

            TMP_Text bossTagTxt = CreateTMPText("BossTagText", topBar.transform, "BOSS STAGE", 28, TextAlignmentOptions.Left);
            bossTagTxt.color = Color.red;
            SetRect(bossTagTxt.GetComponent<RectTransform>(), 0.02f, 0.2f, 0.28f, 0.8f);

            // Player Standing View (Left Side)
            GameObject playerView = CreateUIObject("PlayerView", battlePanel.transform);
            SetRect(playerView.GetComponent<RectTransform>(), 0.1f, 0.35f, 0.45f, 0.85f);

            Image playerImg = CreateUIImage("PlayerCatSprite", playerView.transform);
            SetRect(playerImg.GetComponent<RectTransform>(), 0.1f, 0.3f, 0.9f, 0.95f);

            TMP_Text playerTxt = CreateTMPText("PlayerNameText", playerView.transform, "Player Cat", 28, TextAlignmentOptions.Left);
            SetRect(playerTxt.GetComponent<RectTransform>(), 0.1f, 0.18f, 0.6f, 0.28f);

            TMP_Text playerLvTxt = CreateTMPText("PlayerLevelText", playerView.transform, "Lv. 5", 24, TextAlignmentOptions.Right);
            SetRect(playerLvTxt.GetComponent<RectTransform>(), 0.65f, 0.18f, 0.9f, 0.28f);

            Slider playerHpSlider = CreateUISlider("PlayerHPSlider", playerView.transform, Color.green);
            SetRect(playerHpSlider.GetComponent<RectTransform>(), 0.1f, 0.12f, 0.9f, 0.16f);
            TMP_Text playerHpTxt = CreateTMPText("PlayerHPText", playerView.transform, "HP 0/0 | ATK 0", 18, TextAlignmentOptions.Left);
            SetRect(playerHpTxt.GetComponent<RectTransform>(), 0.1f, 0.065f, 0.9f, 0.12f);
            TMP_Text playerSkillTxt = CreateTMPText("PlayerSkillText", playerView.transform, "SKILL", 18, TextAlignmentOptions.Left);
            SetRect(playerSkillTxt.GetComponent<RectTransform>(), 0.1f, 0.005f, 0.9f, 0.065f);

            // Enemy Standing View (Right Side)
            GameObject enemyView = CreateUIObject("EnemyView", battlePanel.transform);
            SetRect(enemyView.GetComponent<RectTransform>(), 0.55f, 0.35f, 0.9f, 0.85f);

            Image enemyImg = CreateUIImage("EnemyCatSprite", enemyView.transform);
            SetRect(enemyImg.GetComponent<RectTransform>(), 0.1f, 0.3f, 0.9f, 0.95f);

            TMP_Text enemyTxt = CreateTMPText("EnemyNameText", enemyView.transform, "Wild Cat", 28, TextAlignmentOptions.Left);
            SetRect(enemyTxt.GetComponent<RectTransform>(), 0.1f, 0.18f, 0.55f, 0.28f);

            TMP_Text enemyLvTxt = CreateTMPText("EnemyLevelText", enemyView.transform, "Lv. 5", 24, TextAlignmentOptions.Right);
            SetRect(enemyLvTxt.GetComponent<RectTransform>(), 0.6f, 0.18f, 0.75f, 0.28f);

            TMP_Text enemyRarityTxt = CreateTMPText("EnemyRarityText", enemyView.transform, "Basic", 22, TextAlignmentOptions.Right);
            SetRect(enemyRarityTxt.GetComponent<RectTransform>(), 0.78f, 0.18f, 0.95f, 0.28f);

            Slider enemyHpSlider = CreateUISlider("EnemyHPSlider", enemyView.transform, Color.red);
            SetRect(enemyHpSlider.GetComponent<RectTransform>(), 0.1f, 0.12f, 0.9f, 0.16f);
            TMP_Text enemyHpTxt = CreateTMPText("EnemyHPText", enemyView.transform, "HP 0/0 | ATK 0", 18, TextAlignmentOptions.Left);
            SetRect(enemyHpTxt.GetComponent<RectTransform>(), 0.1f, 0.065f, 0.9f, 0.12f);
            TMP_Text enemySkillTxt = CreateTMPText("EnemySkillText", enemyView.transform, "SKILL", 18, TextAlignmentOptions.Left);
            SetRect(enemySkillTxt.GetComponent<RectTransform>(), 0.1f, 0.005f, 0.9f, 0.065f);

            // Bottom Action Log & Prompts Panel
            GameObject bottomPanel = CreateUIObject("BottomPanel", battlePanel.transform);
            SetRect(bottomPanel.GetComponent<RectTransform>(), 0.05f, 0.02f, 0.95f, 0.28f);
            Image botBg = bottomPanel.AddComponent<Image>();
            botBg.color = new Color(0f, 0f, 0f, 0.6f);

            TMP_Text battleLogTxt = CreateTMPText("BattleLogText", bottomPanel.transform, "Battle Started! Prepare for Auto Battle!", 26, TextAlignmentOptions.Center);
            SetRect(battleLogTxt.GetComponent<RectTransform>(), 0.05f, 0.45f, 0.95f, 0.9f);

            TMP_Text promptTxt = CreateTMPText("ActionPromptText", bottomPanel.transform, "[SPACE] : Throw Monster Ball  |  [P] : Party Management", 24, TextAlignmentOptions.Center);
            promptTxt.color = Color.yellow;
            SetRect(promptTxt.GetComponent<RectTransform>(), 0.05f, 0.1f, 0.95f, 0.4f);

            TMP_Text captureFeedbackTxt = CreateTMPText("CaptureFeedbackText", battlePanel.transform, "", 40, TextAlignmentOptions.Center);
            captureFeedbackTxt.color = new Color(0.4f, 0.9f, 1f);
            SetRect(captureFeedbackTxt.GetComponent<RectTransform>(), 0.25f, 0.48f, 0.75f, 0.64f);

            BattleUI battleUIComponent = battlePanel.AddComponent<BattleUI>();
            SetField(battleUIComponent, "stageText", stageTxt);
            SetField(battleUIComponent, "bossTagText", bossTagTxt);
            SetField(battleUIComponent, "ballCountText", ballCountTxt);
            SetField(battleUIComponent, "potionCountText", potionCountTxt);
            SetField(battleUIComponent, "playerCatImage", playerImg);
            SetField(battleUIComponent, "playerCatNameText", playerTxt);
            SetField(battleUIComponent, "playerCatLevelText", playerLvTxt);
            SetField(battleUIComponent, "playerHpSlider", playerHpSlider);
            SetField(battleUIComponent, "playerHpText", playerHpTxt);
            SetField(battleUIComponent, "playerSkillText", playerSkillTxt);
            SetField(battleUIComponent, "enemyCatImage", enemyImg);
            SetField(battleUIComponent, "enemyCatNameText", enemyTxt);
            SetField(battleUIComponent, "enemyCatLevelText", enemyLvTxt);
            SetField(battleUIComponent, "enemyRarityText", enemyRarityTxt);
            SetField(battleUIComponent, "enemyHpSlider", enemyHpSlider);
            SetField(battleUIComponent, "enemyHpText", enemyHpTxt);
            SetField(battleUIComponent, "enemySkillText", enemySkillTxt);
            SetField(battleUIComponent, "battleLogText", battleLogTxt);
            SetField(battleUIComponent, "actionPromptText", promptTxt);
            SetField(battleUIComponent, "captureFeedbackText", captureFeedbackTxt);


            // --- Panel 3: PartyPanel (Top 6 Slots Bar) ---
            GameObject partyPanel = CreateUIObject("PartyPanel", canvasGO.transform);
            SetRect(partyPanel.GetComponent<RectTransform>(), 0.02f, 0.85f, 0.5f, 0.98f);
            PartyUI partyUIComponent = partyPanel.AddComponent<PartyUI>();
            List<PartyUI.PartySlotUI> partySlots = new List<PartyUI.PartySlotUI>(6);
            for (int i = 0; i < 6; i++)
            {
                GameObject slot = CreateUIObject($"PartySlot_{i + 1}", partyPanel.transform);
                SetRect(slot.GetComponent<RectTransform>(), i / 6f, 0f, (i + 1) / 6f, 1f);
                Image slotBg = slot.AddComponent<Image>();
                slotBg.color = new Color(0.04f, 0.06f, 0.1f, 0.9f);
                Image icon = CreateUIImage("Icon", slot.transform);
                SetRect(icon.GetComponent<RectTransform>(), 0.05f, 0.24f, 0.36f, 0.9f);
                TMP_Text name = CreateTMPText("Name", slot.transform, "Cat", 18, TextAlignmentOptions.Left);
                SetRect(name.GetComponent<RectTransform>(), 0.4f, 0.62f, 0.96f, 0.92f);
                TMP_Text level = CreateTMPText("Level", slot.transform, "Lv.5", 15, TextAlignmentOptions.Left);
                SetRect(level.GetComponent<RectTransform>(), 0.4f, 0.36f, 0.96f, 0.62f);
                Slider hp = CreateUISlider("HP", slot.transform, Color.green);
                SetRect(hp.GetComponent<RectTransform>(), 0.4f, 0.1f, 0.96f, 0.27f);
                partySlots.Add(new PartyUI.PartySlotUI { container = slot, iconImage = icon, nameText = name, levelText = level, hpSlider = hp });
            }
            SetField(partyUIComponent, "slots", partySlots);

            // --- Panel 4: PartyManageModalPanel ---
            GameObject partyModalPanel = CreateUIObject("PartyManageModalPanel", canvasGO.transform);
            SetFullStretch(partyModalPanel.GetComponent<RectTransform>());
            Image partyModalBg = partyModalPanel.AddComponent<Image>();
            partyModalBg.color = new Color(0f, 0f, 0f, 0.85f);
            PartyManageModalUI partyModalUIComponent = partyModalPanel.AddComponent<PartyManageModalUI>();
            TMP_Text partyHeader = CreateTMPText("Header", partyModalPanel.transform, "Party Management", 34, TextAlignmentOptions.Center);
            SetRect(partyHeader.GetComponent<RectTransform>(), 0.12f, 0.82f, 0.82f, 0.94f);
            Button closePartyButton = CreateUIButton("CloseButton", partyModalPanel.transform, "CLOSE");
            SetRect(closePartyButton.GetComponent<RectTransform>(), 0.83f, 0.84f, 0.93f, 0.92f);
            AddLocalizedText(closePartyButton.GetComponentInChildren<TMP_Text>(), "close");
            List<PartyManageModalUI.ModalSlot> modalSlots = new List<PartyManageModalUI.ModalSlot>(6);
            for (int i = 0; i < 6; i++)
            {
                float maxY = 0.78f - i * 0.1f;
                float minY = maxY - 0.085f;
                GameObject slot = CreateUIObject($"ModalSlot_{i + 1}", partyModalPanel.transform);
                SetRect(slot.GetComponent<RectTransform>(), 0.18f, minY, 0.82f, maxY);
                Image slotBg = slot.AddComponent<Image>();
                slotBg.color = new Color(0.12f, 0.16f, 0.24f, 1f);
                Image icon = CreateUIImage("Icon", slot.transform);
                SetRect(icon.GetComponent<RectTransform>(), 0.02f, 0.08f, 0.11f, 0.92f);
                TMP_Text name = CreateTMPText("Name", slot.transform, "Cat", 24, TextAlignmentOptions.Left);
                SetRect(name.GetComponent<RectTransform>(), 0.14f, 0.5f, 0.45f, 0.92f);
                TMP_Text level = CreateTMPText("Level", slot.transform, "Lv.5", 20, TextAlignmentOptions.Left);
                SetRect(level.GetComponent<RectTransform>(), 0.14f, 0.1f, 0.3f, 0.5f);
                TMP_Text hpText = CreateTMPText("HP", slot.transform, "HP: 0/0", 20, TextAlignmentOptions.Left);
                SetRect(hpText.GetComponent<RectTransform>(), 0.32f, 0.1f, 0.62f, 0.5f);
                Button replace = CreateUIButton("ReplaceButton", slot.transform, "SWITCH");
                SetRect(replace.GetComponent<RectTransform>(), 0.63f, 0.16f, 0.79f, 0.84f);
                Button release = CreateUIButton("ReleaseButton", slot.transform, "RELEASE");
                SetRect(release.GetComponent<RectTransform>(), 0.81f, 0.16f, 0.97f, 0.84f);
                modalSlots.Add(new PartyManageModalUI.ModalSlot { container = slot, iconImage = icon, nameText = name, levelText = level, hpText = hpText, replaceButton = replace, releaseButton = release });
            }
            SetField(partyModalUIComponent, "modalSlots", modalSlots);
            SetField(partyModalUIComponent, "closeButton", closePartyButton);
            SetField(partyModalUIComponent, "headerText", partyHeader);

            // --- Panel 5: StarterSelectPanel ---
            GameObject starterPanel = CreateUIObject("StarterSelectPanel", canvasGO.transform);
            SetFullStretch(starterPanel.GetComponent<RectTransform>());
            Image starterBg = starterPanel.AddComponent<Image>();
            starterBg.color = new Color(0.1f, 0.12f, 0.18f, 0.98f);

            TMP_Text starterTitle = CreateTMPText("Title", starterPanel.transform, "POCKETROGUELIKE STARTER SELECTION", 36, TextAlignmentOptions.Center);
            SetRect(starterTitle.GetComponent<RectTransform>(), 0.1f, 0.82f, 0.9f, 0.95f);
            AddLocalizedText(starterTitle, "starter_title");

            TMP_Text budgetTxt = CreateTMPText("BudgetText", starterPanel.transform, "Starter Cost Budget: 0 / 10 Points (Selected: 0 / 6)", 26, TextAlignmentOptions.Center);
            budgetTxt.color = Color.yellow;
            SetRect(budgetTxt.GetComponent<RectTransform>(), 0.1f, 0.72f, 0.9f, 0.8f);

            Transform starterSlots = CreateStarterScrollView(starterPanel.transform);

            Button startBtn = CreateUIButton("StartRunButton", starterPanel.transform, "START RUN");
            SetRect(startBtn.GetComponent<RectTransform>(), 0.35f, 0.05f, 0.65f, 0.15f);
            AddLocalizedText(startBtn.GetComponentInChildren<TMP_Text>(), "start_run");

            StarterSelectUI starterUIComponent = starterPanel.AddComponent<StarterSelectUI>();
            SetField(starterUIComponent, "budgetText", budgetTxt);
            SetField(starterUIComponent, "startRunButton", startBtn);
            SetField(starterUIComponent, "slotsContainer", starterSlots);
            UnityEventTools.AddPersistentListener(startBtn.onClick, starterUIComponent.StartRunFromUI);

            // --- Stage Clear Confirmation Overlay ---
            GameObject stageClearPanel = CreateUIObject("StageClearPanel", canvasGO.transform);
            SetFullStretch(stageClearPanel.GetComponent<RectTransform>());
            Image stageClearBg = stageClearPanel.AddComponent<Image>();
            stageClearBg.color = new Color(0f, 0f, 0f, 0.72f);
            GameObject stageClearWindow = CreateUIObject("Window", stageClearPanel.transform);
            SetRect(stageClearWindow.GetComponent<RectTransform>(), 0.27f, 0.24f, 0.73f, 0.76f);
            Image stageClearWindowBg = stageClearWindow.AddComponent<Image>();
            stageClearWindowBg.color = new Color(0.09f, 0.13f, 0.2f, 0.98f);
            TMP_Text stageClearTitle = CreateTMPText("Title", stageClearWindow.transform, "스테이지 클리어!", 42, TextAlignmentOptions.Center);
            SetRect(stageClearTitle.rectTransform, 0.08f, 0.72f, 0.92f, 0.94f);
            TMP_Text stageClearDescription = CreateTMPText("Description", stageClearWindow.transform, "적을 쓰러뜨렸습니다.", 27, TextAlignmentOptions.Center);
            SetRect(stageClearDescription.rectTransform, 0.08f, 0.5f, 0.92f, 0.7f);
            TMP_Text stageClearReward = CreateTMPText("Reward", stageClearWindow.transform, "획득한 아이템이 없습니다.", 25, TextAlignmentOptions.Center);
            SetRect(stageClearReward.rectTransform, 0.08f, 0.3f, 0.92f, 0.49f);
            Button stageClearConfirm = CreateUIButton("ConfirmButton", stageClearWindow.transform, "확인 / 다음 스테이지");
            SetRect(stageClearConfirm.GetComponent<RectTransform>(), 0.25f, 0.08f, 0.75f, 0.24f);
            stageClearPanel.AddComponent<StageClearUI>();

            // --- Panel 6: ResultPanel ---
            GameObject resultPanel = CreateUIObject("ResultPanel", canvasGO.transform);
            SetFullStretch(resultPanel.GetComponent<RectTransform>());
            Image resultBg = resultPanel.AddComponent<Image>();
            resultBg.color = new Color(0f, 0f, 0f, 0.9f);

            TMP_Text resultTitleTxt = CreateTMPText("ResultTitleText", resultPanel.transform, "VICTORY!", 48, TextAlignmentOptions.Center);
            SetRect(resultTitleTxt.GetComponent<RectTransform>(), 0.1f, 0.6f, 0.9f, 0.85f);

            TMP_Text resultDescTxt = CreateTMPText("ResultDescText", resultPanel.transform, "You cleared Stage 100!", 28, TextAlignmentOptions.Center);
            SetRect(resultDescTxt.GetComponent<RectTransform>(), 0.1f, 0.35f, 0.9f, 0.55f);

            Button restartBtn = CreateUIButton("RestartButton", resultPanel.transform, "PLAY AGAIN");
            SetRect(restartBtn.GetComponent<RectTransform>(), 0.35f, 0.12f, 0.65f, 0.25f);
            AddLocalizedText(restartBtn.GetComponentInChildren<TMP_Text>(), "play_again");

            ResultUI resultUIComponent = resultPanel.AddComponent<ResultUI>();
            SetField(resultUIComponent, "titleText", resultTitleTxt);
            SetField(resultUIComponent, "descriptionText", resultDescTxt);
            SetField(resultUIComponent, "restartButton", restartBtn);

            // Persistent language toggle, always rendered above gameplay panels.
            Button languageButton = CreateUIButton("LanguageToggleButton", canvasGO.transform, "한국어 / ENG");
            SetRect(languageButton.GetComponent<RectTransform>(), 0.01f, 0.01f, 0.105f, 0.055f);
            TMP_Text languageLabel = languageButton.GetComponentInChildren<TMP_Text>();
            languageLabel.fontSize = 18f;
            AddLocalizedText(languageLabel, "language_toggle");
            UnityEventTools.AddPersistentListener(languageButton.onClick, languageMgr.ToggleLanguage);
            languageButton.transform.SetAsLastSibling();

            // Connect UIManager Fields
            SetField(uiMgr, "starterSelectPanel", starterPanel);
            SetField(uiMgr, "battlePanel", battlePanel);
            SetField(uiMgr, "partyPanel", partyPanel);
            SetField(uiMgr, "partyManageModalPanel", partyModalPanel);
            SetField(uiMgr, "stageClearPanel", stageClearPanel);
            SetField(uiMgr, "resultPanel", resultPanel);

            // Start with no gameplay panel active. UIManager activates StarterSelect after GameManager initializes.
            battlePanel.SetActive(false);
            partyPanel.SetActive(false);
            partyModalPanel.SetActive(false);
            stageClearPanel.SetActive(false);
            starterPanel.SetActive(false);
            resultPanel.SetActive(false);

            // Ensure Assets/Scenes directory exists
            if (!Directory.Exists("Assets/Scenes")) Directory.CreateDirectory("Assets/Scenes");

            string scenePath = "Assets/Scenes/MainGame.unity";
            bool saved = EditorSceneManager.SaveScene(newScene, scenePath);

            if (saved)
            {
                Debug.Log($"[MainGameSceneBuilder] MainGame scene successfully saved to: {scenePath}");
            }
            else
            {
                Debug.LogError($"[MainGameSceneBuilder] Failed to save scene: {scenePath}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        // --- Helper Methods for UI Creation ---

        private static Transform CreateStarterScrollView(Transform parent)
        {
            GameObject scrollObject = CreateUIObject("StarterScrollView", parent);
            SetRect(scrollObject.GetComponent<RectTransform>(), 0.05f, 0.18f, 0.95f, 0.70f);
            ScrollRect scroll = scrollObject.AddComponent<ScrollRect>();

            GameObject viewportObject = CreateUIObject("Viewport", scrollObject.transform);
            RectTransform viewportRect = viewportObject.GetComponent<RectTransform>();
            SetFullStretch(viewportRect);
            Image viewportImage = viewportObject.AddComponent<Image>();
            viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
            viewportObject.AddComponent<RectMask2D>();

            GameObject contentObject = CreateUIObject("Content", viewportObject.transform);
            RectTransform contentRect = contentObject.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = Vector2.zero;
            GridLayoutGroup grid = contentObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(230f, 280f);
            grid.spacing = new Vector2(20f, 20f);
            grid.padding = new RectOffset(10, 10, 10, 10);
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 6;
            ContentSizeFitter fitter = contentObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewportRect;
            scroll.content = contentRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 35f;
            return contentObject.transform;
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void SetFullStretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void SetRect(RectTransform rt, float minX, float minY, float maxX, float maxY)
        {
            rt.anchorMin = new Vector2(minX, minY);
            rt.anchorMax = new Vector2(maxX, maxY);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static TMP_Text CreateTMPText(string name, Transform parent, string content, float fontSize, TextAlignmentOptions align)
        {
            GameObject go = CreateUIObject(name, parent);
            TMP_Text txt = go.AddComponent<TextMeshProUGUI>();
            txt.text = content;
            txt.fontSize = fontSize;
            txt.alignment = align;
            txt.color = Color.white;
            txt.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Maplestory Bold SDF.asset");
            return txt;
        }

        private static Image CreateUIImage(string name, Transform parent)
        {
            GameObject go = CreateUIObject(name, parent);
            Image img = go.AddComponent<Image>();
            img.preserveAspect = true;
            return img;
        }

        private static Slider CreateUISlider(string name, Transform parent, Color fillColor)
        {
            GameObject sliderGO = CreateUIObject(name, parent);
            Slider slider = sliderGO.AddComponent<Slider>();

            GameObject bgGO = CreateUIObject("Background", sliderGO.transform);
            Image bgImg = bgGO.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            SetFullStretch(bgGO.GetComponent<RectTransform>());

            GameObject fillArea = CreateUIObject("Fill Area", sliderGO.transform);
            SetFullStretch(fillArea.GetComponent<RectTransform>());

            GameObject fillGO = CreateUIObject("Fill", fillArea.transform);
            Image fillImg = fillGO.AddComponent<Image>();
            fillImg.color = fillColor;
            SetFullStretch(fillGO.GetComponent<RectTransform>());

            slider.fillRect = fillGO.GetComponent<RectTransform>();
            slider.targetGraphic = fillImg;

            return slider;
        }

        private static Button CreateUIButton(string name, Transform parent, string label)
        {
            GameObject btnGO = CreateUIObject(name, parent);
            Image bg = btnGO.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.6f, 0.9f);
            Button btn = btnGO.AddComponent<Button>();

            TMP_Text txt = CreateTMPText("Text", btnGO.transform, label, 24, TextAlignmentOptions.Center);
            SetFullStretch(txt.GetComponent<RectTransform>());

            return btn;
        }

        private static void AddLocalizedText(TMP_Text text, string key)
        {
            if (text == null) return;
            LocalizedText localized = text.GetComponent<LocalizedText>();
            if (localized == null) localized = text.gameObject.AddComponent<LocalizedText>();
            localized.SetKey(key);
        }

        private static void SetField(object target, string fieldName, object value)
        {
            System.Reflection.FieldInfo field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (field != null)
            {
                field.SetValue(target, value);
            }
        }
    }
}
