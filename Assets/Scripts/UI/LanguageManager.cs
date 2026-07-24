using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace PocketRoguelike
{
    public enum GameLanguage { Korean, English }

    public class LanguageManager : MonoBehaviour
    {
        private const string PreferenceKey = "PocketRoguelike.Language";
        private static GameLanguage currentLanguage = GameLanguage.Korean;
        public static event Action<GameLanguage> OnLanguageChanged;
        public static GameLanguage CurrentLanguage => currentLanguage;
        public static bool IsKorean => currentLanguage == GameLanguage.Korean;

        private static readonly Dictionary<string, (string ko, string en)> Texts = new Dictionary<string, (string, string)>
        {
            { "language_toggle", ("한국어 / ENG", "ENG / 한국어") },
            { "starter_title", ("포켓 로그라이크 스타팅 선택", "POCKETROGUELIKE STARTER SELECTION") },
            { "starter_budget", ("스타팅 비용: {0} / 10 포인트 (선택: {1} / 6)", "Starter Cost: {0} / 10 Points (Selected: {1} / 6)") },
            { "starter_card", ("<b>{0}</b>\n체력 {4:N0} | 공격력 {5:N0}\n비용: {1} 포인트 | {2}\n스킬: {3}", "<b>{0}</b>\nHP {4:N0} | ATK {5:N0}\nCost: {1} Pt | {2}\nSkill: {3}") },
            { "start_run", ("게임 시작", "START RUN") },
            { "ball_count", ("포켓볼 x {0}", "POKE BALL x {0}") },
            { "potion_count", ("회복약 x {0}", "POTION x {0}") },
            { "no_balls", ("포켓볼이 없습니다!", "NO POKE BALLS!") },
            { "throw_ball", ("포켓볼을 던졌다!", "THREW A POKE BALL!") },
            { "catch_failed", ("포획 실패! 적이 빠져나왔다!", "CAPTURE FAILED! IT BROKE FREE!") },
            { "catch_success", ("{0} 포획 성공!", "CAUGHT {0}!") },
            { "party_manage", ("파티 관리", "PARTY MANAGEMENT") },
            { "close", ("닫기", "CLOSE") },
            { "release", ("방출", "RELEASE") },
            { "replace", ("교체", "SWITCH") },
            { "victory", ("승리!", "VICTORY!") },
            { "defeat", ("패배!", "DEFEAT!") },
            { "play_again", ("다시 하기", "PLAY AGAIN") },
            { "battle_prompt", ("[SPACE]: 포켓볼 던지기  |  [H]: 회복약 사용(50%)  |  [P]: 파티 관리", "[SPACE]: Throw Poke Ball  |  [H]: Use Potion (50%)  |  [P]: Party Management") },
            { "battle_start", ("전투 시작! 자동 전투를 준비하세요!", "Battle Started! Prepare for Auto Battle!") },
            { "attack_log", ("{0}의 {1}! {2}에게 {3:N0} 피해!", "{0} used {1}! {3:N0} damage to {2}!") },
            { "stats_line", ("체력 {0:N0}/{1:N0} | 공격력 {2:N0}", "HP {0:N0}/{1:N0} | ATK {2:N0}") },
            { "skill_line", ("스킬: {0}", "SKILL: {0}") },
            { "boss_stage", ("보스 스테이지", "BOSS STAGE") },
            { "final_boss", ("최종 보스", "FINAL BOSS") },
            { "stage", ("스테이지 {0} / 100", "STAGE {0} / 100") },
            { "level", ("레벨 {0}", "Lv. {0}") },
            { "hp", ("체력: {0}/{1}", "HP: {0}/{1}") },
            { "game_over", ("게임 오버", "GAME OVER") },
            { "victory_description", ("축하합니다! 포켓 로그라이크 100개 스테이지를 모두 클리어했습니다!", "Congratulations! You cleared all 100 stages of PocketRoguelike!") },
            { "defeat_description", ("스테이지 {0}에서 파티가 전멸했습니다. 다음 런에 다시 도전하세요!", "Your party fainted on Stage {0}. Better luck next run!") },
            { "party_full_replace", ("파티가 가득 찼습니다! {0}와(과) 교체할 고양이를 선택하세요.", "Party Full (6/6)! Replace a Cat with {0}:") },
            { "party_hint", ("파티 관리 (ESC: 돌아가기)", "Party Management (Press [ESC] to Return)") },
            { "party_switch_turn_cost", ("교체할 고양이를 선택하세요. 교체하면 내 공격 1회를 소모합니다.", "Choose a Cat. Switching consumes your next attack.") },
            { "forced_switch", ("고양이가 쓰러졌습니다! 살아있는 고양이로 교체하세요.", "Your Cat fainted! Choose a surviving Cat.") },
            { "stage_clear", ("스테이지 클리어!", "STAGE CLEAR!") },
            { "stage_clear_description", ("{0}을(를) 쓰러뜨렸습니다.", "Defeated {0}.") },
            { "reward_none", ("획득한 아이템이 없습니다.", "No item dropped.") },
            { "reward_ball", ("포켓볼 1개 획득!", "Poke Ball x1 obtained!") },
            { "reward_potion", ("회복약 1개 획득!", "Potion x1 obtained!") },
            { "reward_both", ("포켓볼 1개와 회복약 1개 획득!", "Poke Ball x1 and Potion x1 obtained!") },
            { "confirm_next", ("확인 / 다음 스테이지", "CONFIRM / NEXT STAGE") },
            { "player_cat", ("플레이어 고양이", "PLAYER CAT") },
            { "wild_cat", ("야생 고양이", "WILD CAT") },
            { "rarity_basic", ("기본", "Basic") },
            { "rarity_ex", ("EX", "EX") },
            { "rarity_rare", ("레어", "Rare") },
            { "rarity_unique", ("유니크", "Unique") },
            { "rarity_epic", ("에픽", "Epic") },
            { "rarity_legend", ("레전드", "Legend") }
        };

        private void Awake()
        {
            currentLanguage = (GameLanguage)PlayerPrefs.GetInt(PreferenceKey, (int)GameLanguage.Korean);
            DontDestroyOnLoad(gameObject);
        }

        private void Start() => OnLanguageChanged?.Invoke(currentLanguage);
        public void ToggleLanguage() => SetLanguage(IsKorean ? GameLanguage.English : GameLanguage.Korean);

        public void SetLanguage(GameLanguage language)
        {
            currentLanguage = language;
            PlayerPrefs.SetInt(PreferenceKey, (int)currentLanguage);
            PlayerPrefs.Save();
            OnLanguageChanged?.Invoke(currentLanguage);
        }

        public static string Get(string key)
        {
            if (!Texts.TryGetValue(key, out var text)) return key;
            return IsKorean ? text.ko : text.en;
        }

        public static string Format(string key, params object[] args) => string.Format(Get(key), args);

        public static string CatName(CatDataSO data)
        {
            if (data == null) return IsKorean ? "고양이" : "Cat";
            string localized = IsKorean ? data.catNameKorean : data.catNameEnglish;
            if (!string.IsNullOrWhiteSpace(localized)) return localized;
            if (IsKorean) return $"고양이 #{data.dexNo}";
            return string.IsNullOrWhiteSpace(data.catName) ? $"Cat #{data.dexNo}" : data.catName;
        }

        public static string SkillName(CatDataSO data)
        {
            if (data == null) return IsKorean ? "기본 공격" : "Basic Attack";
            string localized = IsKorean ? data.skillNameKorean : data.skillNameEnglish;
            if (!string.IsNullOrWhiteSpace(localized)) return localized;
            return IsKorean ? "기본 공격" : "Basic Attack";
        }

        public static string Rarity(CatRarity rarity)
        {
            switch (rarity)
            {
                case CatRarity.Basic: return Get("rarity_basic");
                case CatRarity.EX: return Get("rarity_ex");
                case CatRarity.Rare: return Get("rarity_rare");
                case CatRarity.Unique: return Get("rarity_unique");
                case CatRarity.Epic: return Get("rarity_epic");
                case CatRarity.Legend: return Get("rarity_legend");
                default: return rarity.ToString();
            }
        }
    }

}
