using UnityEngine;
using TMPro;
using KanairoCity.Models;
using System;

namespace KanairoCity.UI
{
    /// <summary>
    /// UI component for selecting and saving a player's profession.
    /// </summary>
    public class ProfessionSelectorUI : MonoBehaviour
    {
        [Header("Data Profile")]
        [SerializeField] private PlayerProfileData profileData;

        [Header("UI Feedback")]
        [SerializeField] private TMP_Dropdown professionDropdown;
        [SerializeField] private TMP_Text selectedProfessionDescription;

        public event Action OnProfessionUpdated;

        private void Start()
        {
            if (professionDropdown != null)
            {
                PopulateDropdown();
                professionDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
            }
            else
            {
                Debug.LogWarning("ProfessionSelectorUI: professionDropdown is not assigned.");
            }
        }

        private void PopulateDropdown()
        {
            if (professionDropdown == null) return;

            professionDropdown.ClearOptions();
            var options = new System.Collections.Generic.List<string>(Enum.GetNames(typeof(Profession)));
            professionDropdown.AddOptions(options);

            // Set current selection
            professionDropdown.value = (int)profileData.SelectedProfession;
        }

        private void OnDropdownValueChanged(int index)
        {
            Profession selected = (Profession)index;
            profileData.SelectedProfession = selected;
            UpdateDescription(selected);
            OnProfessionUpdated?.Invoke();
        }

        private void UpdateDescription(Profession profession)
        {
            if (selectedProfessionDescription == null) return;

            string desc = "";
            switch (profession)
            {
                case Profession.MatatuDriver:
                    desc = "Master of Nairobi streets. Navigates traffic with ease.";
                    break;
                case Profession.StreetVendor:
                    desc = "The heart of the economy. Knows every customer's name.";
                    break;
                case Profession.BodaBodaRider:
                    desc = "Fast, agile, and always on the move.";
                    break;
                case Profession.TechEntrepreneur:
                    desc = "Silicon Savannah pioneer. Innovation at its peak.";
                    break;
                default:
                    desc = "Choose your path in Kanairo.";
                    break;
            }
            selectedProfessionDescription.text = desc;
        }
    }
}
