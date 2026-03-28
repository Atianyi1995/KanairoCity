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

        private void Update()
        {
            if (CampaignManager.Instance == null) return;

            var manager = CampaignManager.Instance;
            playerSupportText.text = $"Player: {manager.playerSupportPercent:F1}%";
            rivalSupportText.text = $"Rival: {manager.rivalSupportPercent:F1}%";
            undecidedText.text = $"Undecided: {manager.undecidedPercent:F1}%";
            
            float time = manager.GetRemainingTime();
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }
}