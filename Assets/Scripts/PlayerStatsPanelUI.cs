using UnityEngine;
using TMPro;
using KanairoCity.Models;

namespace KanairoCity.UI
{
    /// <summary>
    /// Displays the player stats in the menu panel.
    /// </summary>
    public class PlayerStatsPanelUI : MonoBehaviour
    {
        [Header("Data Profile")]
        [SerializeField] private PlayerProfileData profileData;

        [Header("Stat Text Displays")]
        [SerializeField] private TMP_Text professionText;
        [SerializeField] private TMP_Text moneyText;
        [SerializeField] private TMP_Text reputationText;
        [SerializeField] private TMP_Text popularityText;
        [SerializeField] private TMP_Text policeRespectText;
        [SerializeField] private TMP_Text vendorSkillText;
        [SerializeField] private TMP_Text drivingSkillText;

        private void OnEnable()
        {
            UpdateStatsDisplay();
        }

        /// <summary>
        /// Updates all TMP text fields with values from profile data.
        /// </summary>
        public void UpdateStatsDisplay()
        {
            if (profileData == null) return;

            if (professionText != null) professionText.text = $"Profession: {profileData.SelectedProfession}";
            if (moneyText != null) moneyText.text = $"Money: KES {profileData.Money:N0}";
            if (reputationText != null) reputationText.text = $"Reputation: {profileData.Reputation}";
            if (popularityText != null) popularityText.text = $"Popularity: {profileData.Popularity}";
            if (policeRespectText != null) policeRespectText.text = $"Police Respect: {profileData.PoliceRespect}%";
            if (vendorSkillText != null) vendorSkillText.text = $"Vendor Skill: {profileData.VendorSkill}";
            if (drivingSkillText != null) drivingSkillText.text = $"Driving Skill: {profileData.DrivingSkill}";
        }
    }
}
