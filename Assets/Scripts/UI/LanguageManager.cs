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
            { "reward_revive", ("기력의 조각 1개 획득!", "Revive Piece x1 obtained!") },
            { "reward_both", ("포켓볼 1개와 회복약 1개 획득!", "Poke Ball x1 and Potion x1 obtained!") },
            { "revive", ("부활", "REVIVE") },
            { "revive_count", ("기력의 조각 x {0}", "REVIVE x {0}") },
            { "revive_count_info", ("기력의 조각: {0}개", "Revives: {0}") },
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

        public static bool ContainsKorean(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            foreach (char c in text)
            {
                if ((c >= 0xAC00 && c <= 0xD7A3) || (c >= 0x1100 && c <= 0x11FF) || (c >= 0x3130 && c <= 0x318F))
                    return true;
            }
            return false;
        }

        private static readonly Dictionary<string, string> SkillTranslationMap = new Dictionary<string, string>
        {
            { "기본 공격", "Basic Attack" },
            { "단일 근접 냥코 펀치", "Single Melee Cat Punch" },
            { "블록 밀착 타격", "Block Shield Strike" },
            { "빨간 적 특수 베기", "Anti-Red Special Slash" },
            { "원거리 하이킥", "Long-Range High Kick" },
            { "초속공 돌진 헤딩", "Super Speed Charge Headbutt" },
            { "단사거리 광역 새 쪼기", "Close Area Peck" },
            { "빨간 적 특화 바이트", "Anti-Red Special Bite" },
            { "초원거리 불꽃 브레스", "Long Flame Breath" },
            { "광역 냥코 펀치", "Area Cat Punch" },
            { "삼바 댄스 연타", "Samba Dance Flurry" },
            { "3연속 합동 검술", "Trio Cross Slash" },
            { "표창 연속 투척", "Shuriken Toss" },
            { "빨간 적 특효 참격", "Anti-Red Slash" },
            { "밀어내기 손바닥 치기", "Pushing Palm Strike" },
            { "속공 큐트 펀치", "Speed Cute Punch" },
            { "배리어 브레이커 킥", "Barrier Breaker Kick" },
            { "팬츠 휩쓸기 킥", "Pants Sweep Kick" },
            { "에일리언 특화 빔", "Anti-Alien Beam" },
            { "검은 적 전용 악마 펀치", "Anti-Black Demon Punch" },
            { "유령 투령 기습", "Phantom Strike" },
            { "좀비 킬러 매장 베기", "Zombie Slayer Burial" },
            { "레전드 파동검", "Legend Wave Blade" },
            { "신속 발키리 창", "Swift Valkyrie Spear" },
            { "각성 폭화 일격", "Awakened Fire Strike" },
            { "원거리 태풍 사격", "Typhoon Ranged Shot" },
            { "바주카 원거리 포격", "Bazooka Cannon Shell" },
            { "꼬마 냥코 펀치", "Mini Cat Punch" },
            { "꼬마 블록 장막", "Mini Block Shield" },
            { "꼬마 빨간 적 특공", "Mini Anti-Red Assault" },
            { "꼬마 파동 발사", "Mini Wave Shot" },
            { "꼬마 초속공 돌진", "Mini Speed Rush" },
            { "꼬마 공중 광역 포격", "Mini Aerial Bombardment" },
            { "꼬마 물고기 크리티컬", "Mini Fish Crit Bite" },
            { "꼬마 불꽃 원거리 사격", "Mini Flame Shot" },
            { "꼬마 거인 고양이", "Mini Giant Cat" },
            { "꼬마 지진 펀치", "Mini Earthquake Punch" },
            { "배리어 브레이커 시약 포격", "Reagent Barrier Breaker" },
            { "초고체력 1회성 돌진", "Ultra Tank Charge" },
            { "선인 전용 광역 파동", "Hermit Area Shockwave" },
            { "신들의 황혼 신벌 포격", "Godly Judgment Cannon" },
            { "모밀 크리티컬 일격", "Soba Noodle Crit Strike" },
            { "카레 원거리 광역 투척", "Curry Ranged Bomb" },
            { "사망시 자폭 대형 파동", "Death Explosion Wave" },
            { "초수인 특공 포자 포격", "Spore Beast Cannon" },
            { "원거리 조준 스나이핑", "Sniper Precision Shot" },
            { "붉은 적 특화 대포 사격", "Anti-Red Cannon" },
            { "레슬링 드롭킥", "Wrestling Dropkick" },
            { "유틸리티 기계 빔", "Tech Utility Beam" },
            { "강림 특화 대장 포격", "Commander Descent Cannon" },
            { "고대종/악 마 특공 참격", "Ancient Demon Slash" },
            { "양산형 범용 원거리 범위 사격", "Mass Production Beam" }
        };

        public static string TranslateSkillNameToEnglish(string koreanSkill, int dexNo)
        {
            if (string.IsNullOrWhiteSpace(koreanSkill)) return dexNo > 0 ? $"Cat #{dexNo} Attack" : "Basic Attack";
            string trimmed = koreanSkill.Trim();
            if (SkillTranslationMap.TryGetValue(trimmed, out string exact)) return exact;

            string translated = trimmed;
            translated = translated.Replace("냥코 펀치", "Cat Punch");
            translated = translated.Replace("펀치", "Punch");
            translated = translated.Replace("타격", "Strike");
            translated = translated.Replace("참격", "Slash");
            translated = translated.Replace("베기", "Slash");
            translated = translated.Replace("하이킥", "High Kick");
            translated = translated.Replace("헤딩", "Headbutt");
            translated = translated.Replace("쪼기", "Peck");
            translated = translated.Replace("바이트", "Bite");
            translated = translated.Replace("물기", "Bite");
            translated = translated.Replace("브레스", "Breath");
            translated = translated.Replace("파동검", "Wave Blade");
            translated = translated.Replace("파동", "Shockwave");
            translated = translated.Replace("포격", "Cannon Shell");
            translated = translated.Replace("대포", "Cat Cannon");
            translated = translated.Replace("사격", "Ranged Shot");
            translated = translated.Replace("크리티컬", "Critical Strike");
            translated = translated.Replace("자폭", "Self-Destruct");
            translated = translated.Replace("드롭킥", "Dropkick");
            translated = translated.Replace("스나이핑", "Sniper Shot");
            translated = translated.Replace("빔", "Beam");
            translated = translated.Replace("일격", "Strike");
            translated = translated.Replace("단일", "Single");
            translated = translated.Replace("광역", "Area");
            translated = translated.Replace("원거리", "Long Range");
            translated = translated.Replace("근접", "Melee");
            translated = translated.Replace("빨간 적", "Anti-Red");
            translated = translated.Replace("붉은 적", "Anti-Red");
            translated = translated.Replace("검은 적", "Anti-Black");
            translated = translated.Replace("천사", "Anti-Angel");
            translated = translated.Replace("에일리언", "Anti-Alien");
            translated = translated.Replace("좀비", "Anti-Zombie");
            translated = translated.Replace("메탈", "Anti-Metal");
            translated = translated.Replace("고대종", "Anti-Ancient");
            translated = translated.Replace("악마", "Anti-Demon");
            translated = translated.Replace("기본 공격", "Basic Attack");

            if (ContainsKorean(translated))
            {
                return dexNo > 0 ? $"Cat #{dexNo} Attack" : "Basic Attack";
            }
            return translated;
        }

        public static string CatName(CatDataSO data)
        {
            if (data == null) return IsKorean ? "고양이" : "Cat";
            if (IsKorean)
            {
                if (!string.IsNullOrWhiteSpace(data.catNameKorean)) return data.catNameKorean;
                if (!string.IsNullOrWhiteSpace(data.catName)) return data.catName;
                return $"고양이 #{data.dexNo}";
            }
            else
            {
                string en = data.catNameEnglish;
                if (!string.IsNullOrWhiteSpace(en) && !ContainsKorean(en)) return en;
                if (!string.IsNullOrWhiteSpace(data.catName) && !ContainsKorean(data.catName)) return data.catName;
                return $"Cat #{data.dexNo}";
            }
        }

        public static string SkillName(CatDataSO data)
        {
            if (data == null) return IsKorean ? "기본 공격" : "Basic Attack";
            if (IsKorean)
            {
                if (!string.IsNullOrWhiteSpace(data.skillNameKorean)) return data.skillNameKorean;
                return "기본 공격";
            }
            else
            {
                string en = data.skillNameEnglish;
                if (!string.IsNullOrWhiteSpace(en) && !ContainsKorean(en)) return en;
                return TranslateSkillNameToEnglish(data.skillNameKorean, data.dexNo);
            }
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
