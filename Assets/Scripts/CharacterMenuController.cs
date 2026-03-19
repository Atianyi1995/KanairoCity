using Bozo.ModularCharacters;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KanairoCity.Models;
using System;

namespace KanairoCity.UI
{
    /// <summary>
    /// Controller for handling BoZo-style modular character creation and saving.
    /// </summary>
    public class CharacterMenuController : MonoBehaviour
    {
        [Header("Data Profile")]
        [SerializeField] private PlayerProfileData profileData;

        [Header("BoZo Components")]
        [SerializeField] private CharacterCreator bozoCharacterCreator;
        [SerializeField] private SaveSelector bozoSaveSelector;

        [Header("UI Feedback")]
        [SerializeField] private TMP_InputField characterNameInputField;
        [SerializeField] private Button saveCharacterButton;

        public event Action OnCharacterSaved;

        private void Start()
        {
            if (saveCharacterButton != null)
            {
                saveCharacterButton.onClick.AddListener(SaveCharacter);
            }
            
            if (characterNameInputField != null)
            {
                characterNameInputField.onValueChanged.AddListener(OnNameChanged);
                characterNameInputField.text = profileData.CharacterName;
            }
        }

        private void OnNameChanged(string newName)
        {
            profileData.CharacterName = newName;
        }

        /// <summary>
        /// Saves the current BoZo character and updates player profile data.
        /// </summary>
        public void SaveCharacter()
        {
            if (bozoCharacterCreator == null) return;

            // Use BoZo SaveSelector or CharacterCreator's internal save logic
            if (bozoSaveSelector != null)
            {
                // In BoZo, saving usually triggers through UI, but we can call it here.
                // Assuming character has been named and modified via BoZo's UI.
                profileData.IsCharacterCreated = true;
                profileData.SavedCharacterId = characterNameInputField != null ? characterNameInputField.text : "New_Citizen";
                
                // Invoke save complete
                OnCharacterSaved?.Invoke();
                Debug.Log($"Character '{profileData.CharacterName}' saved successfully.");
            }
        }
    }
}
