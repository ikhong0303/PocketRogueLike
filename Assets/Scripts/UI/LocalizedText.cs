using TMPro;
using UnityEngine;

namespace PocketRoguelike
{
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] private string key;
        private TMP_Text label;

        public string Key => key;
        public void SetKey(string value) { key = value; Refresh(); }
        private void Awake() => label = GetComponent<TMP_Text>();
        private void OnEnable() { LanguageManager.OnLanguageChanged += HandleLanguageChanged; Refresh(); }
        private void OnDisable() => LanguageManager.OnLanguageChanged -= HandleLanguageChanged;
        private void HandleLanguageChanged(GameLanguage _) => Refresh();
        private void Refresh()
        {
            if (label == null) label = GetComponent<TMP_Text>();
            if (label != null && !string.IsNullOrEmpty(key)) label.text = LanguageManager.Get(key);
        }
    }
}
