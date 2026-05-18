using UnityEngine;
using TMPro;

namespace Kanairo.Core
{
    public class CampaignHUD : MonoBehaviour
    {
        public TextMeshProUGUI playerSupportText;
        public TextMeshProUGUI rivalSupportText;
        public TextMeshProUGUI undecidedText;
        public TextMeshProUGUI timerText;

        [Header("Multiplayer Settings")]
        [Tooltip("If true, only player percentage is shown during campaign")]
        public bool privateCampaignMode = true;

        private void Update()
        {
            if (CampaignManager.Instance == null) return;

            var manager = CampaignManager.Instance;
            
            // Only show player percentage if private mode is on
            playerSupportText.text = $"My Support: {manager.playerSupportPercent:F1}%";
            
            if (privateCampaignMode)
            {
                rivalSupportText.gameObject.SetActive(false);
                undecidedText.gameObject.SetActive(false);
            }
            else
            {
                rivalSupportText.gameObject.SetActive(true);
                undecidedText.gameObject.SetActive(true);
                rivalSupportText.text = $"Opponents: {manager.rivalSupportPercent:F1}%";
                undecidedText.text = $"Undecided: {manager.undecidedPercent:F1}%";
            }
            
            float time = manager.GetRemainingTime();
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }
}