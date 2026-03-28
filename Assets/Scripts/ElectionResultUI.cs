using UnityEngine;
using TMPro;

namespace Kanairo.Core
{
    public class ElectionResultUI : MonoBehaviour
    {
        public GameObject panel;
        public TextMeshProUGUI resultHeaderText;
        public TextMeshProUGUI finalStatsText;

        private void Update()
        {
            if (GameManager.Instance == null) return;

            var state = GameManager.Instance.CurrentState;
            if (state == GameState.Victory || state == GameState.Defeat)
            {
                ShowResult(state == GameState.Victory);
            }
        }

        private void ShowResult(bool victory)
        {
            panel.SetActive(true);
            resultHeaderText.text = victory ? "YOU WON THE ELECTION!" : "ELECTION DEFEAT";
            
            var manager = CampaignManager.Instance;
            finalStatsText.text = $"Final Support: {manager.playerSupportPercent:F1}%\n" +
                                 $"Rival Support: {manager.rivalSupportPercent:F1}%";
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
}