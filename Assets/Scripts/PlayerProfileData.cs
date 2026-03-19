using UnityEngine;

namespace KanairoCity.Models
{
    /// <summary>
    /// Supported professions in the Nairobi life simulator.
    /// </summary>
    public enum Profession
    {
        Unemployed,
        MatatuDriver,
        StreetVendor,
        BodaBodaRider,
        TechEntrepreneur,
        Artist,
        Officer
    }

    /// <summary>
    /// Supported game modes.
    /// </summary>
    public enum GameMode
    {
        SinglePlayer,
        Multiplayer
    }

    [CreateAssetMenu(fileName = "PlayerProfileData", menuName = "KanairoCity/PlayerProfileData")]
    public class PlayerProfileData : ScriptableObject
    {
        [Header("Identity")]
        public string CharacterName = "Citizen";
        public Profession SelectedProfession = Profession.Unemployed;
        public string SavedCharacterId; // Reference to BoZo character save

        [Header("Stats")]
        public float Money = 1000f;
        public float Reputation = 0f;
        public float Popularity = 0f;
        public float PoliceRespect = 50f;
        public float VendorSkill = 0f;
        public float DrivingSkill = 0f;

        [Header("Persistence")]
        public GameMode LastSelectedMode = GameMode.SinglePlayer;
        public bool IsCharacterCreated = false;

        /// <summary>
        /// Resets stats to starting values based on profession.
        /// </summary>
        public void ResetProfile()
        {
            Money = 1000f;
            Reputation = 0f;
            Popularity = 0f;
            PoliceRespect = 50f;
            VendorSkill = 0f;
            DrivingSkill = 0f;
            IsCharacterCreated = false;
            SavedCharacterId = string.Empty;
        }
    }
}
