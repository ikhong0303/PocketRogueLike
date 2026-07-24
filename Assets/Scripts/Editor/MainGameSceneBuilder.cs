using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PocketRoguelike.EditorTools
{
    [InitializeOnLoad]
    public static class MainGameSceneBuilder
    {
        static MainGameSceneBuilder()
        {
            EditorApplication.delayCall += BuildMainGameScene;
        }

        [MenuItem("Tools/Build MainGame Scene")]
        public static void BuildMainGameScene()
        {
            Debug.Log("[MainGameSceneBuilder] Building complete MainGame.unity scene...");

            // 1. Generate 100 Cat Data first
            CatDataAutoGenerator.Generate100CatData();

            // 2. Create new scene
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 3. Create EventSystem
            GameObject esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<StandaloneInputModule>();

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
            UIManager uiMgr = managersGO.AddComponent<UIManager>();

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

            TMP_Text bossTagTxt = CreateTMPText("BossTagText", topBar.transform, "★ BOSS STAGE ★", 28, TextAlignmentOptions.Right);
            bossTagTxt.color = Color.red;
            SetRect(bossTagTxt.GetComponent<RectTransform>(), 0.7f, 0.2f, 0.95f, 0.8f);

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
            SetRect(playerHpSlider.GetComponent<RectTransform>(), 0.1f, 0.05f, 0.9f, 0.15f);

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
            SetRect(enemyHpSlider.GetComponent<RectTransform>(), 0.1f, 0.05f, 0.9f, 0.15f);

            // Bottom Action Log & Prompts Panel
            GameObject bottomPanel = CreateUIObject("BottomPanel", battlePanel.transform);
            SetRect(bottomPanel.GetComponent<RectTransform>(), 0.05f, 0.02f, 0.95f, 0.28f);
            Image botBg = bottomPanel.AddComponent<Image>();
            botBg.color = new Color(0f, 0f, 0f, 0.6f);

            TMP_Text battleLogTxt = CreateTMPText("BattleLogText", bottomPanel.transform, "Battle Started! Prepare for Auto Battle!", 26, TextAlignmentOptions.Center);
            SetRect(battleLogTxt.GetComponent<RectTransform>(), 0.05f, 0.45f, 0.95f, 0.9f);

            TMP_Text promptTxt = CreateTMPText("ActionPromptText", bottomPanel.transform, "[SPACE] : Catch Attempt  |  [P] : Party Management", 24, TextAlignmentOptions.Center);
            promptTxt.color = Color.yellow;
            SetRect(promptTxt.GetComponent<RectTransform>(), 0.05f, 0.1f, 0.95f, 0.4f);

            BattleUI battleUIComponent = battlePanel.AddComponent<BattleUI>();
            SetField(battleUIComponent, "stageText", stageTxt);
            SetField(battleUIComponent, "bossTagText", bossTagTxt);
            SetField(battleUIComponent, "playerCatImage", playerImg);
            SetField(battleUIComponent, "playerCatNameText", playerTxt);
            SetField(battleUIComponent, "playerCatLevelText", playerLvTxt);
            SetField(battleUIComponent, "playerHpSlider", playerHpSlider);
            SetField(battleUIComponent, "enemyCatImage", enemyImg);
            SetField(battleUIComponent, "enemyCatNameText", enemyTxt);
            SetField(battleUIComponent, "enemyCatLevelText", enemyLvTxt);
            SetField(battleUIComponent, "enemyRarityText", enemyRarityTxt);
            SetField(battleUIComponent, "enemyHpSlider", enemyHpSlider);
            SetField(battleUIComponent, "battleLogText", battleLogTxt);
            SetField(battleUIComponent, "actionPromptText", promptTxt);

            // --- Panel 2: CatchTimingPanel ---
            GameObject catchPanel = CreateUIObject("CatchTimingPanel", canvasGO.transform);
            SetFullStretch(catchPanel.GetComponent<RectTransform>());

            GameObject catchBox = CreateUIObject("CatchBox", catchPanel.transform);
            SetRect(catchBox.GetComponent<RectTransform>(), 0.2f, 0.35f, 0.8f, 0.65f);
            Image catchBg = catchBox.AddComponent<Image>();
            catchBg.color = new Color(0.1f, 0.1f, 0.15f, 0.92f);

            TMP_Text catchInstructTxt = CreateTMPText("InstructionText", catchBox.transform, "Press [SPACE] when Indicator is in GREEN zone!", 26, TextAlignmentOptions.Center);
            SetRect(catchInstructTxt.GetComponent<RectTransform>(), 0.05f, 0.7f, 0.95f, 0.9f);

            Slider timingSlider = CreateUISlider("TimingGaugeSlider", catchBox.transform, Color.green);
            SetRect(timingSlider.GetComponent<RectTransform>(), 0.1f, 0.4f, 0.9f, 0.6f);

            TMP_Text catchResultTxt = CreateTMPText("ResultText", catchBox.transform, "", 28, TextAlignmentOptions.Center);
            SetRect(catchResultTxt.GetComponent<RectTransform>(), 0.05f, 0.1f, 0.95f, 0.35f);

            CatchTimingUI catchUIComponent = catchPanel.AddComponent<CatchTimingUI>();
            SetField(catchUIComponent, "gaugeSlider", timingSlider);
            SetField(catchUIComponent, "instructionText", catchInstructTxt);
            SetField(catchUIComponent, "resultText", catchResultTxt);

            // --- Panel 3: PartyPanel (Top 6 Slots Bar) ---
            GameObject partyPanel = CreateUIObject("PartyPanel", canvasGO.transform);
            SetRect(partyPanel.GetComponent<RectTransform>(), 0.02f, 0.85f, 0.5f, 0.98f);
            partyPanel.AddComponent<PartyUI>();

            // --- Panel 4: PartyManageModalPanel ---
            GameObject partyModalPanel = CreateUIObject("PartyManageModalPanel", canvasGO.transform);
            SetFullStretch(partyModalPanel.GetComponent<RectTransform>());
            Image partyModalBg = partyModalPanel.AddComponent<Image>();
            partyModalBg.color = new Color(0f, 0f, 0f, 0.85f);
            partyModalPanel.AddComponent<PartyManageModalUI>();

            // --- Panel 5: StarterSelectPanel ---
            GameObject starterPanel = CreateUIObject("StarterSelectPanel", canvasGO.transform);
            SetFullStretch(starterPanel.GetComponent<RectTransform>());
            Image starterBg = starterPanel.AddComponent<Image>();
            starterBg.color = new Color(0.1f, 0.12f, 0.18f, 0.98f);

            TMP_Text starterTitle = CreateTMPText("Title", starterPanel.transform, "🐱 POCKETROGUELIKE STARTER SELECTION 🐱", 36, TextAlignmentOptions.Center);
            SetRect(starterTitle.GetComponent<RectTransform>(), 0.1f, 0.82f, 0.9f, 0.95f);

            TMP_Text budgetTxt = CreateTMPText("BudgetText", starterPanel.transform, "Starter Cost Budget: 0 / 10 Points (Selected: 0 / 3)", 26, TextAlignmentOptions.Center);
            budgetTxt.color = Color.yellow;
            SetRect(budgetTxt.GetComponent<RectTransform>(), 0.1f, 0.72f, 0.9f, 0.8f);

            Button startBtn = CreateUIButton("StartRunButton", starterPanel.transform, "🚀 START RUN");
            SetRect(startBtn.GetComponent<RectTransform>(), 0.35f, 0.05f, 0.65f, 0.15f);

            StarterSelectUI starterUIComponent = starterPanel.AddComponent<StarterSelectUI>();
            SetField(starterUIComponent, "budgetText", budgetTxt);
            SetField(starterUIComponent, "startRunButton", startBtn);

            // --- Panel 6: ResultPanel ---
            GameObject resultPanel = CreateUIObject("ResultPanel", canvasGO.transform);
            SetFullStretch(resultPanel.GetComponent<RectTransform>());
            Image resultBg = resultPanel.AddComponent<Image>();
            resultBg.color = new Color(0f, 0f, 0f, 0.9f);

            TMP_Text resultTitleTxt = CreateTMPText("ResultTitleText", resultPanel.transform, "🏆 VICTORY! 🏆", 48, TextAlignmentOptions.Center);
            SetRect(resultTitleTxt.GetComponent<RectTransform>(), 0.1f, 0.6f, 0.9f, 0.85f);

            TMP_Text resultDescTxt = CreateTMPText("ResultDescText", resultPanel.transform, "You cleared Stage 100!", 28, TextAlignmentOptions.Center);
            SetRect(resultDescTxt.GetComponent<RectTransform>(), 0.1f, 0.35f, 0.9f, 0.55f);

            Button restartBtn = CreateUIButton("RestartButton", resultPanel.transform, "🔄 PLAY AGAIN");
            SetRect(restartBtn.GetComponent<RectTransform>(), 0.35f, 0.12f, 0.65f, 0.25f);

            ResultUI resultUIComponent = resultPanel.AddComponent<ResultUI>();
            SetField(resultUIComponent, "titleText", resultTitleTxt);
            SetField(resultUIComponent, "descriptionText", resultDescTxt);
            SetField(resultUIComponent, "restartButton", restartBtn);

            // Connect UIManager Fields
            SetField(uiMgr, "starterSelectPanel", starterPanel);
            SetField(uiMgr, "battlePanel", battlePanel);
            SetField(uiMgr, "catchTimingPanel", catchPanel);
            SetField(uiMgr, "partyPanel", partyPanel);
            SetField(uiMgr, "partyManageModalPanel", partyModalPanel);
            SetField(uiMgr, "resultPanel", resultPanel);

            // Hide overlay panels by default
            catchPanel.SetActive(false);
            partyModalPanel.SetActive(false);
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
