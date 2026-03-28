using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Kanairo.Core;

namespace Kanairo.UI
{
    public class ElectionResultUIController : MonoBehaviour
    {
        [Header("Panels")]
        public GameObject victoryPanel;
        public GameObject defeatPanel;

        [Header("Victory Text")]
        public TextMeshProUGUI victoryFinalPercent;

        [Header("Defeat Text")]
        public TextMeshProUGUI defeatPlayerPercent;
        public TextMeshProUGUI defeatOpponentPercent;

        private void OnEnable()
        {
            UpdateUI();
        }

        public void UpdateUI()
        {
            var manager = CampaignManager.Instance;
            if (manager == null) return;

            victoryPanel.SetActive(false);
            defeatPanel.SetActive(false);

            GameState state = GameManager.Instance.CurrentState;

            if (state == GameState.Victory)
            {
                victoryPanel.SetActive(true);
                victoryFinalPercent.text = $"Final Vote: {manager.playerSupportPercent:F1}%";
            }
            else if (state == GameState.Defeat)
            {
                defeatPanel.SetActive(true);
                defeatPlayerPercent.text = "You lost due to lack of support and funding. The rival has taken office.";
                defeatOpponentPercent.text = "However, you have 30 minutes until the next election cycle begins. Use this time to build public appeal!";
            }
        }

        public void AcceptDefeat()
        {
            GameManager.Instance.ChangeState(GameState.OpponentTerm);
        }

        public void EnterOffice()
        {
            GameManager.Instance.ChangeState(GameState.InOffice);
        }

        public void ExitGame()
        {
            Application.Quit();
        }
    }
}