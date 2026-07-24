using System;
using System.Collections.Generic;
using UnityEngine;

namespace PocketRoguelike
{
    public class PartyManager : MonoBehaviour
    {
        public static PartyManager Instance { get; private set; }

        public const int MAX_PARTY_SIZE = 6;

        [SerializeField] private List<CatInstance> party = new List<CatInstance>();

        public IReadOnlyList<CatInstance> Party => party;
        public int Count => party.Count;
        public bool IsFull => party.Count >= MAX_PARTY_SIZE;

        public event Action OnPartyUpdated;
        public event Action<CatInstance> OnActiveCatChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        public CatInstance GetActiveCat()
        {
            if (party.Count == 0) return null;
            // First non-fainted cat in party
            return party.Find(c => !c.IsFainted) ?? party[0];
        }

        public bool AddCat(CatInstance newCat)
        {
            if (newCat == null) return false;

            if (IsFull)
            {
                Debug.LogWarning("[PartyManager] Party is full (6 cats max)!");
                return false;
            }

            party.Add(newCat);
            OnPartyUpdated?.Invoke();
            return true;
        }

        public bool SwapCat(int index, CatInstance newCat)
        {
            if (index < 0 || index >= party.Count || newCat == null) return false;

            party[index] = newCat;
            OnPartyUpdated?.Invoke();
            return true;
        }

        public bool ReleaseCat(int index)
        {
            if (index < 0 || index >= party.Count) return false;
            if (party.Count <= 1)
            {
                Debug.LogWarning("[PartyManager] Cannot release the last remaining cat!");
                return false;
            }

            party.RemoveAt(index);
            OnPartyUpdated?.Invoke();
            return true;
        }

        public void FullHealAll()
        {
            foreach (var cat in party)
            {
                cat.FullHeal();
            }
            OnPartyUpdated?.Invoke();
            Debug.Log("[PartyManager] All party cats fully healed!");
        }

        public bool IsAllFainted()
        {
            if (party.Count == 0) return true;
            return party.TrueForAll(c => c.IsFainted);
        }

        public void ClearParty()
        {
            party.Clear();
            OnPartyUpdated?.Invoke();
        }
    }
}
