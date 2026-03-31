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
                string rivalName = manager.RivalCandidate != null ? manager.RivalCandidate.candidateName : "The Rival";
                defeatPlayerPercent.text = $"You lost to {rivalName} with only {manager.playerSupportPercent:F1}% support.";
                defeatOpponentPercent.text = "However, you have 30 minutes while they are in office to rebuild your public appeal for the next election!";
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

        /// <summary>
        /// General continue method that can be used for both Victory and Defeat buttons
        /// </summary>
        public void ContinueToPostElection()
        {
            GameState state = GameManager.Instance.CurrentState;
            if (state == GameState.Victory)
            {
                EnterOffice();
            }
            else if (state == GameState.Defeat)
            {
                AcceptDefeat();
            }
        }

        public void ExitGame()
        {
            Application.Quit();
        }
    }
}