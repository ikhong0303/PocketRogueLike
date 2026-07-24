using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PocketRoguelike
{
    public class BattleUI : MonoBehaviour
    {
        [Header("Stage Info")]
        [SerializeField] private TMP_Text stageText;
        [SerializeField] private TMP_Text bossTagText;

        [Header("Player Cat Display")]
        [SerializeField] private Image playerCatImage;
        [SerializeField] private TMP_Text playerCatNameText;
        [SerializeField] private TMP_Text playerCatLevelText;
        [SerializeField] private Slider playerHpSlider;
        [SerializeField] private TMP_Text playerHpText;
        [SerializeField] private TMP_Text playerSkillText;

        [Header("Enemy Cat Display")]
        [SerializeField] private Image enemyCatImage;
        [SerializeField] private TMP_Text enemyCatNameText;
        [SerializeField] private TMP_Text enemyCatLevelText;
        [SerializeField] private TMP_Text enemyRarityText;
        [SerializeField] private Slider enemyHpSlider;
        [SerializeField] private TMP_Text enemyHpText;
        [SerializeField] private TMP_Text enemySkillText;

        [Header("Battle Log & Prompts")]
        [SerializeField] private TMP_Text battleLogText;
        [SerializeField] private TMP_Text actionPromptText;
        [SerializeField] private TMP_Text ballCountText;
        [SerializeField] private TMP_Text potionCountText;
        [SerializeField] private TMP_Text captureFeedbackText;

        [Header("Combat Feedback")]
        [SerializeField, Min(0.05f)] private float lungeDuration = 0.12f;
        [SerializeField, Min(0.05f)] private float returnDuration = 0.16f;
        [SerializeField, Range(0.5f, 0.9f)] private float lungeDistance = 0.72f;
        [SerializeField, Min(0.2f)] private float floatingTextDuration = 0.75f;
        [SerializeField, Min(0.1f)] private float hpDrainDuration = 0.55f;
        [SerializeField, Min(0.1f)] private float faintSquashDuration = 0.4f;

        private CatInstance lastAttacker;
        private CatInstance lastDefender;
        private CatInstance captureTarget;
        private int lastDamage;
        private int lastShake;
        private bool lastShakePassed;
        private BattleMessage message = BattleMessage.Start;

        private enum BattleMessage { Start, Attack, Throw, CaptureSuccess, CaptureFailed, Captured }

        private void Awake()
        {
            EnsureUIComponents();
            DisableDynamicLocalizedText(actionPromptText);
            DisableDynamicLocalizedText(bossTagText);
        }

        private void OnEnable()
        {
            EnsureUIComponents();
            LanguageManager.OnLanguageChanged -= HandleLanguageChanged;
            LanguageManager.OnLanguageChanged += HandleLanguageChanged;

            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.OnBattleStarted -= UpdateBattleView;
                BattleManager.Instance.OnAttackExecuted -= OnAttackExecuted;
                BattleManager.Instance.OnEnemyCaptured -= OnEnemyCaptured;
                BattleManager.Instance.OnBattleStarted += UpdateBattleView;
                BattleManager.Instance.OnAttackExecuted += OnAttackExecuted;
                BattleManager.Instance.OnEnemyCaptured += OnEnemyCaptured;
                if (BattleManager.Instance.PlayerCat != null)
                    RefreshLocalizedBattleView(BattleManager.Instance.PlayerCat, BattleManager.Instance.EnemyCat);
            }

            SubscribeToCatchManager();
            RefreshLocalizedMessages();
        }

        private void OnDisable()
        {
            LanguageManager.OnLanguageChanged -= HandleLanguageChanged;
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.OnBattleStarted -= UpdateBattleView;
                BattleManager.Instance.OnAttackExecuted -= OnAttackExecuted;
                BattleManager.Instance.OnEnemyCaptured -= OnEnemyCaptured;
            }
            if (CatchManager.Instance != null)
            {
                CatchManager.Instance.OnBallCountChanged -= RefreshBallCount;
                CatchManager.Instance.OnPotionCountChanged -= RefreshPotionCount;
                CatchManager.Instance.OnCaptureStarted -= OnCaptureStarted;
                CatchManager.Instance.OnCaptureShake -= OnCaptureShake;
                CatchManager.Instance.OnCatchResult -= OnCaptureResolved;
            }
            if (BattleManager.Instance != null && BattleManager.Instance.IsPresentingAttack)
                BattleManager.Instance.CompleteAttackPresentation();
        }

        private void EnsureUIComponents()
        {
            if (stageText == null) stageText = transform.Find("TopBar/StageText")?.GetComponent<TMP_Text>();
            if (bossTagText == null) bossTagText = transform.Find("TopBar/BossTagText")?.GetComponent<TMP_Text>();
            if (playerCatImage == null) playerCatImage = transform.Find("PlayerView/PlayerCatSprite")?.GetComponent<Image>();
            if (playerCatNameText == null) playerCatNameText = transform.Find("PlayerView/PlayerNameText")?.GetComponent<TMP_Text>();
            if (playerCatLevelText == null) playerCatLevelText = transform.Find("PlayerView/PlayerLevelText")?.GetComponent<TMP_Text>();
            if (playerHpSlider == null) playerHpSlider = transform.Find("PlayerView/PlayerHPSlider")?.GetComponent<Slider>();
            if (playerHpText == null) playerHpText = transform.Find("PlayerView/PlayerHPText")?.GetComponent<TMP_Text>();
            if (playerSkillText == null) playerSkillText = transform.Find("PlayerView/PlayerSkillText")?.GetComponent<TMP_Text>();
            if (enemyCatImage == null) enemyCatImage = transform.Find("EnemyView/EnemyCatSprite")?.GetComponent<Image>();
            if (enemyCatNameText == null) enemyCatNameText = transform.Find("EnemyView/EnemyNameText")?.GetComponent<TMP_Text>();
            if (enemyCatLevelText == null) enemyCatLevelText = transform.Find("EnemyView/EnemyLevelText")?.GetComponent<TMP_Text>();
            if (enemyRarityText == null) enemyRarityText = transform.Find("EnemyView/EnemyRarityText")?.GetComponent<TMP_Text>();
            if (enemyHpSlider == null) enemyHpSlider = transform.Find("EnemyView/EnemyHPSlider")?.GetComponent<Slider>();
            if (enemyHpText == null) enemyHpText = transform.Find("EnemyView/EnemyHPText")?.GetComponent<TMP_Text>();
            if (enemySkillText == null) enemySkillText = transform.Find("EnemyView/EnemySkillText")?.GetComponent<TMP_Text>();
            if (battleLogText == null) battleLogText = transform.Find("BottomPanel/BattleLogText")?.GetComponent<TMP_Text>();
            if (actionPromptText == null) actionPromptText = transform.Find("BottomPanel/ActionPromptText")?.GetComponent<TMP_Text>();
            if (ballCountText == null) ballCountText = transform.Find("TopBar/BallCountText")?.GetComponent<TMP_Text>();
            if (potionCountText == null) potionCountText = transform.Find("TopBar/PotionCountText")?.GetComponent<TMP_Text>();
            if (captureFeedbackText == null) captureFeedbackText = transform.Find("CaptureFeedbackText")?.GetComponent<TMP_Text>();
        }

        private static void DisableDynamicLocalizedText(TMP_Text text)
        {
            LocalizedText localized = text != null ? text.GetComponent<LocalizedText>() : null;
            if (localized != null) localized.enabled = false;
        }

        private void SubscribeToCatchManager()
        {
            if (CatchManager.Instance == null) return;
            CatchManager.Instance.OnBallCountChanged -= RefreshBallCount;
            CatchManager.Instance.OnPotionCountChanged -= RefreshPotionCount;
            CatchManager.Instance.OnCaptureStarted -= OnCaptureStarted;
            CatchManager.Instance.OnCaptureShake -= OnCaptureShake;
            CatchManager.Instance.OnCatchResult -= OnCaptureResolved;
            CatchManager.Instance.OnBallCountChanged += RefreshBallCount;
            CatchManager.Instance.OnPotionCountChanged += RefreshPotionCount;
            CatchManager.Instance.OnCaptureStarted += OnCaptureStarted;
            CatchManager.Instance.OnCaptureShake += OnCaptureShake;
            CatchManager.Instance.OnCatchResult += OnCaptureResolved;
            RefreshBallCount(CatchManager.Instance.BallCount);
            RefreshPotionCount(CatchManager.Instance.PotionCount);
        }

        private void HandleLanguageChanged(GameLanguage _)
        {
            CatInstance player = BattleManager.Instance != null ? BattleManager.Instance.PlayerCat : null;
            CatInstance enemy = BattleManager.Instance != null ? BattleManager.Instance.EnemyCat : null;
            RefreshLocalizedBattleView(player, enemy);
            RefreshLocalizedMessages();
        }

        public void UpdateBattleView(CatInstance player, CatInstance enemy)
        {
            captureTarget = null;
            lastShake = 0;
            message = BattleMessage.Start;
            if (captureFeedbackText != null) captureFeedbackText.text = string.Empty;
            RefreshLocalizedBattleView(player, enemy);
            RefreshLocalizedMessages();
        }

        private void RefreshLocalizedBattleView(CatInstance player, CatInstance enemy)
        {
            if (StageManager.Instance != null)
            {
                if (stageText != null) stageText.text = LanguageManager.Format("stage", StageManager.Instance.CurrentStage);
                if (bossTagText != null)
                {
                    bossTagText.gameObject.SetActive(StageManager.Instance.IsBossStage);
                    bossTagText.text = LanguageManager.Get(StageManager.Instance.IsFinalBoss ? "final_boss" : "boss_stage");
                }
            }

            if (player != null && player.Data != null)
            {
                if (playerCatImage != null) { playerCatImage.gameObject.SetActive(true); playerCatImage.rectTransform.localScale = new Vector3(-1f, 1f, 1f); playerCatImage.sprite = player.Data.sprite; }
                if (playerCatNameText != null) playerCatNameText.text = LanguageManager.CatName(player.Data);
                if (playerCatLevelText != null) playerCatLevelText.text = LanguageManager.Format("level", player.Level);
                if (playerSkillText != null) playerSkillText.text = LanguageManager.Format("skill_line", LanguageManager.SkillName(player.Data));
                UpdateHp(player, playerHpSlider, playerHpText);
            }

            if (enemy != null && enemy.Data != null)
            {
                if (enemyCatImage != null) { enemyCatImage.gameObject.SetActive(true); enemyCatImage.rectTransform.localScale = Vector3.one; enemyCatImage.sprite = enemy.Data.sprite; }
                if (enemyHpSlider != null) enemyHpSlider.gameObject.SetActive(true);
                if (enemyCatNameText != null) enemyCatNameText.text = LanguageManager.CatName(enemy.Data);
                if (enemyCatLevelText != null) enemyCatLevelText.text = LanguageManager.Format("level", enemy.Level);
                if (enemyRarityText != null)
                {
                    enemyRarityText.text = LanguageManager.Rarity(enemy.Data.rarity);
                    enemyRarityText.color = enemy.Data.rarity.GetRarityColor();
                }
                if (enemySkillText != null) enemySkillText.text = LanguageManager.Format("skill_line", LanguageManager.SkillName(enemy.Data));
                UpdateHp(enemy, enemyHpSlider, enemyHpText);
            }

            RefreshBallCount(CatchManager.Instance != null ? CatchManager.Instance.BallCount : 0);
            RefreshPotionCount(CatchManager.Instance != null ? CatchManager.Instance.PotionCount : 0);
        }

        private void RefreshLocalizedMessages()
        {
            if (battleLogText != null)
            {
                switch (message)
                {
                    case BattleMessage.Attack:
                        battleLogText.text = LanguageManager.Format("attack_log", LanguageManager.CatName(lastAttacker?.Data), LanguageManager.SkillName(lastAttacker?.Data), LanguageManager.CatName(lastDefender?.Data), lastDamage);
                        break;
                    case BattleMessage.Throw:
                        battleLogText.text = LanguageManager.Get("throw_ball");
                        break;
                    case BattleMessage.CaptureSuccess:
                    case BattleMessage.Captured:
                        battleLogText.text = LanguageManager.Format("catch_success", LanguageManager.CatName(captureTarget?.Data));
                        break;
                    case BattleMessage.CaptureFailed:
                        battleLogText.text = LanguageManager.Get("catch_failed");
                        break;
                    default:
                        battleLogText.text = LanguageManager.Get("battle_start");
                        break;
                }
            }

            if (captureFeedbackText == null || captureTarget == null) return;
            if (message == BattleMessage.Throw)
            {
                string[] marks = { "[ ]", "[ ]", "[ ]" };
                for (int i = 0; i < lastShake - 1; i++) marks[i] = "[O]";
                if (lastShake > 0) marks[lastShake - 1] = lastShakePassed ? "[O]" : "[X]";
                captureFeedbackText.text = lastShake == 0
                    ? $"{LanguageManager.Get("throw_ball")}\n[ ]  [ ]  [ ]"
                    : $"{marks[0]}  {marks[1]}  {marks[2]}";
            }
            else if (message == BattleMessage.CaptureSuccess || message == BattleMessage.Captured)
                captureFeedbackText.text = $"[O]  [O]  [O]\n{LanguageManager.Format("catch_success", LanguageManager.CatName(captureTarget.Data))}";
            else if (message == BattleMessage.CaptureFailed)
                captureFeedbackText.text = $"{BuildShakeMarks()}\n{LanguageManager.Get("catch_failed")}";
        }

        private string BuildShakeMarks()
        {
            string[] marks = { "[ ]", "[ ]", "[ ]" };
            for (int i = 0; i < lastShake - 1; i++) marks[i] = "[O]";
            if (lastShake > 0) marks[lastShake - 1] = lastShakePassed ? "[O]" : "[X]";
            return $"{marks[0]}  {marks[1]}  {marks[2]}";
        }

        private void RefreshBallCount(int count)
        {
            if (ballCountText != null) ballCountText.text = LanguageManager.Format("ball_count", count);
            if (actionPromptText != null)
            {
                actionPromptText.text = LanguageManager.Get(count > 0 ? "battle_prompt" : "no_balls");
                actionPromptText.color = count > 0 ? Color.yellow : new Color(1f, 0.35f, 0.35f);
            }
        }

        private void RefreshPotionCount(int count)
        {
            if (potionCountText != null) potionCountText.text = LanguageManager.Format("potion_count", count);
        }

        private void OnCaptureStarted(CatInstance target)
        {
            captureTarget = target;
            lastShake = 0;
            message = BattleMessage.Throw;
            if (captureFeedbackText != null) captureFeedbackText.color = new Color(0.4f, 0.9f, 1f);
            RefreshLocalizedMessages();
        }

        private void OnCaptureShake(int shake, bool passed)
        {
            lastShake = shake;
            lastShakePassed = passed;
            message = BattleMessage.Throw;
            if (captureFeedbackText != null) captureFeedbackText.color = passed ? new Color(0.4f, 0.9f, 1f) : new Color(1f, 0.3f, 0.3f);
            RefreshLocalizedMessages();
        }

        private void OnCaptureResolved(bool success, CatInstance target)
        {
            captureTarget = target;
            message = success ? BattleMessage.CaptureSuccess : BattleMessage.CaptureFailed;
            if (captureFeedbackText != null) captureFeedbackText.color = success ? new Color(0.35f, 1f, 0.45f) : new Color(1f, 0.3f, 0.3f);
            RefreshLocalizedMessages();
        }

        private void OnAttackExecuted(CatInstance attacker, CatInstance defender, int damage, string _)
        {
            lastAttacker = attacker;
            lastDefender = defender;
            lastDamage = damage;
            message = BattleMessage.Attack;
            RefreshLocalizedMessages();

            CatInstance player = BattleManager.Instance.PlayerCat;
            Image attackerImage = attacker == player ? playerCatImage : enemyCatImage;
            Image defenderImage = defender == player ? playerCatImage : enemyCatImage;
            Slider defenderSlider = defender == player ? playerHpSlider : enemyHpSlider;
            TMP_Text defenderHpText = defender == player ? playerHpText : enemyHpText;
            StartCoroutine(PlayAttackFeedback(attackerImage, defenderImage, defenderSlider, defenderHpText, defender, damage, defender == player));
        }

        private void OnEnemyCaptured(CatInstance capturedEnemy)
        {
            captureTarget = capturedEnemy;
            message = BattleMessage.Captured;
            if (enemyCatImage != null) enemyCatImage.gameObject.SetActive(false);
            if (enemyCatNameText != null) enemyCatNameText.text = string.Empty;
            if (enemyCatLevelText != null) enemyCatLevelText.text = string.Empty;
            if (enemyRarityText != null) enemyRarityText.text = string.Empty;
            if (enemyHpSlider != null) enemyHpSlider.gameObject.SetActive(false);
            if (enemyHpText != null) enemyHpText.text = string.Empty;
            RefreshLocalizedMessages();
        }

        private static void UpdateHp(CatInstance cat, Slider slider, TMP_Text label)
        {
            if (slider != null) { slider.maxValue = cat.MaxHp; slider.value = cat.CurrentHp; }
            if (label != null) label.text = LanguageManager.Format("stats_line", cat.CurrentHp, cat.MaxHp, cat.Atk);
        }

        private IEnumerator PlayAttackFeedback(Image attackerImage, Image defenderImage, Slider hpSlider, TMP_Text hpLabel, CatInstance defenderCat, int damage, bool playerWasHit)
        {
            RectTransform attacker = attackerImage != null ? attackerImage.rectTransform : null;
            RectTransform defender = defenderImage != null ? defenderImage.rectTransform : null;

            SoundManager.Instance?.PlayAttackSfx();

            if (attacker != null && lastAttacker?.Data != null)
                StartCoroutine(ShowSkillCallout(attacker, LanguageManager.SkillName(lastAttacker.Data), playerWasHit));

            if (attacker != null && defender != null)
            {
                Vector3 start = attacker.position;
                Vector3 impact = Vector3.Lerp(start, defender.position, lungeDistance);
                int siblingIndex = attacker.GetSiblingIndex();
                attacker.SetAsLastSibling();
                yield return MoveRect(attacker, start, impact, lungeDuration, true);
                StartCoroutine(ShowFloatingDamage(defender, damage, playerWasHit));
                yield return MoveRect(attacker, impact, start, returnDuration, false);
                attacker.position = start;
                attacker.SetSiblingIndex(Mathf.Min(siblingIndex, attacker.parent.childCount - 1));
            }

            SoundManager.Instance?.PlayHurtSfx();
            yield return AnimateHpDrain(defenderCat, hpSlider, hpLabel);
            if (defenderCat.IsFainted && defender != null)
                yield return SquashFaintedSprite(defender);

            BattleManager.Instance?.CompleteAttackPresentation();
        }

        private IEnumerator AnimateHpDrain(CatInstance cat, Slider slider, TMP_Text label)
        {
            float startValue = slider != null ? slider.value : Mathf.Min(cat.MaxHp, cat.CurrentHp + lastDamage);
            float targetValue = cat.CurrentHp;
            float elapsed = 0f;
            while (elapsed < hpDrainDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / hpDrainDuration);
                float eased = 1f - (1f - t) * (1f - t);
                float value = Mathf.Lerp(startValue, targetValue, eased);
                if (slider != null) slider.value = value;
                if (label != null) label.text = LanguageManager.Format("stats_line", Mathf.RoundToInt(value), cat.MaxHp, cat.Atk);
                yield return null;
            }
            if (slider != null) slider.value = targetValue;
            if (label != null) label.text = LanguageManager.Format("stats_line", cat.CurrentHp, cat.MaxHp, cat.Atk);
        }

        private IEnumerator SquashFaintedSprite(RectTransform sprite)
        {
            Vector3 startScale = sprite.localScale;
            float elapsed = 0f;
            while (elapsed < faintSquashDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / faintSquashDuration);
                float eased = t * t;
                sprite.localScale = new Vector3(startScale.x, Mathf.Lerp(startScale.y, 0f, eased), startScale.z);
                yield return null;
            }
            sprite.localScale = new Vector3(startScale.x, 0f, startScale.z);
        }

        private static IEnumerator MoveRect(RectTransform rect, Vector3 from, Vector3 to, float duration, bool easeIn)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = easeIn ? t * t : 1f - (1f - t) * (1f - t);
                rect.position = Vector3.LerpUnclamped(from, to, eased);
                yield return null;
            }
            rect.position = to;
        }

        private IEnumerator ShowSkillCallout(RectTransform attacker, string skillName, bool enemyAttack)
        {
            if (attacker == null || string.IsNullOrWhiteSpace(skillName)) yield break;

            GameObject calloutObject = new GameObject("SkillCalloutText", typeof(RectTransform));
            calloutObject.transform.SetParent(transform, false);
            RectTransform rect = calloutObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(520f, 100f);
            rect.position = attacker.position + Vector3.up * 105f;

            TextMeshProUGUI label = calloutObject.AddComponent<TextMeshProUGUI>();
            label.text = $"{skillName}!";
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 38f;
            label.fontStyle = FontStyles.Bold;
            label.color = enemyAttack ? new Color(1f, 0.4f, 0.35f) : new Color(0.35f, 0.9f, 1f);
            label.outlineWidth = 0.3f;
            label.outlineColor = Color.black;
            if (battleLogText != null && battleLogText.font != null) label.font = battleLogText.font;

            CanvasGroup group = calloutObject.AddComponent<CanvasGroup>();
            Destroy(calloutObject, floatingTextDuration + 0.25f);
            Vector3 start = rect.position;
            Vector3 end = start + Vector3.up * 55f;
            float elapsed = 0f;
            while (elapsed < floatingTextDuration)
            {
                if (calloutObject == null || rect == null || group == null) yield break;
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / floatingTextDuration);
                rect.position = Vector3.Lerp(start, end, t);
                group.alpha = t < 0.65f ? 1f : 1f - Mathf.InverseLerp(0.65f, 1f, t);
                yield return null;
            }
            if (calloutObject != null) Destroy(calloutObject);
        }

        private IEnumerator ShowFloatingDamage(RectTransform defender, int damage, bool playerWasHit)
        {
            GameObject damageObject = new GameObject("DamageText", typeof(RectTransform));
            damageObject.transform.SetParent(transform, false);
            RectTransform rect = damageObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(260f, 90f);
            rect.position = defender.position + Vector3.up * 55f;
            TextMeshProUGUI label = damageObject.AddComponent<TextMeshProUGUI>();
            label.text = $"-{damage}";
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 52f;
            if (battleLogText != null) label.font = battleLogText.font;
            label.fontStyle = FontStyles.Bold;
            label.color = playerWasHit ? new Color(1f, 0.3f, 0.3f) : new Color(1f, 0.88f, 0.15f);
            label.outlineWidth = 0.25f;
            label.outlineColor = Color.black;
            if (battleLogText != null && battleLogText.font != null) label.font = battleLogText.font;
            CanvasGroup group = damageObject.AddComponent<CanvasGroup>();
            Vector3 start = rect.position;
            Vector3 end = start + Vector3.up * 120f;
            float elapsed = 0f;
            while (elapsed < floatingTextDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / floatingTextDuration);
                rect.position = Vector3.Lerp(start, end, 1f - (1f - t) * (1f - t));
                group.alpha = t < 0.55f ? 1f : 1f - Mathf.InverseLerp(0.55f, 1f, t);
                yield return null;
            }
            Destroy(damageObject);
        }
    }
}
