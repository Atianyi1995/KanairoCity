using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Kanairo.UI
{
    public class OfficeComputerUI : MonoBehaviour
    {
        [Header("Panels")]
        public GameObject homePanel;
        public GameObject budgetPanel;
        public GameObject partyPanel;

        [Header("References")]
        public Slider securitySlider;
        public Slider cleanlinessSlider;
        public Slider roadsSlider;

        [Header("Political Party System")]
        public TMP_InputField partyNameInput;
        public GameObject registerButton;
        public TextMeshProUGUI partyStatusText;

        [Header("BudgetTexts")]
        public TextMeshProUGUI totalBudgetText;
        public TextMeshProUGUI remainingBudgetText;
        public TextMeshProUGUI approvalText;
        public TextMeshProUGUI personalFundsText;
        public GameObject overspendWarning;

        private float total;
        private bool hasParty;

        private void OnEnable()
        {
            ShowHome();
            
            var manager = Core.OfficeBudgetManager.Instance;
            if (manager == null) return;

            total = manager.totalBudget;
            securitySlider.value = manager.securityFunding / total;
            cleanlinessSlider.value = manager.cleanlinessFunding / total;
            roadsSlider.value = manager.roadsFunding / total;

            // Load Party Data
            string savedParty = PlayerPrefs.GetString("PoliticalPartyName", "");
            hasParty = !string.IsNullOrEmpty(savedParty);
            UpdatePartyUI(savedParty);

            UpdateDisplay();
        }

        public void ShowHome()
        {
            if (homePanel) homePanel.SetActive(true);
            if (budgetPanel) budgetPanel.SetActive(false);
            if (partyPanel) partyPanel.SetActive(false);
        }

        public void ShowBudget()
        {
            if (homePanel) homePanel.SetActive(false);
            if (budgetPanel) budgetPanel.SetActive(true);
            if (partyPanel) partyPanel.SetActive(false);
        }

        public void ShowParty()
        {
            if (homePanel) homePanel.SetActive(false);
            if (budgetPanel) budgetPanel.SetActive(false);
            if (partyPanel) partyPanel.SetActive(true);
        }

        private void Update()
        {
            // Update display in real-time as sliders move or approval changes
            UpdateDisplay();
        }

        public void RegisterParty()
        {
            if (partyNameInput == null || string.IsNullOrEmpty(partyNameInput.text)) return;

            string name = partyNameInput.text;
            PlayerPrefs.SetString("PoliticalPartyName", name);
            PlayerPrefs.SetInt("HasPoliticalParty", 1);
            PlayerPrefs.SetFloat("PartyApprovalMultiplier", 1.2f); // Default for manual registration
            PlayerPrefs.Save();

            hasParty = true;
            UpdatePartyUI(name);
            Debug.Log($"[PARTY] Registered party: {name}. Negotiation boost (20%) active.");
        }

        private void UpdatePartyUI(string name)
        {
            if (hasParty)
            {
                if (partyStatusText != null) partyStatusText.text = $"Party: {name}\nStatus: Registered\nBonus: +20% Negotiation";
                if (registerButton != null) registerButton.SetActive(false);
                if (partyNameInput != null) partyNameInput.gameObject.SetActive(false);
            }
            else
            {
                if (partyStatusText != null) partyStatusText.text = "No Party Registered";
                if (registerButton != null) registerButton.SetActive(true);
                if (partyNameInput != null) partyNameInput.gameObject.SetActive(true);
            }
        }

        public void UpdateDisplay()
        {
            var manager = Core.OfficeBudgetManager.Instance;
            if (manager == null) return;

            float sec = securitySlider.value * total;
            float clean = cleanlinessSlider.value * total;
            float rd = roadsSlider.value * total;
            float spent = sec + clean + rd;
            
            totalBudgetText.text = $"Total: ${total:N0}";
            remainingBudgetText.text = $"Remaining: ${manager.remainingBudget:N0}";
            personalFundsText.text = $"Secret Funds: ${manager.personalFunds:N0}";
            overspendWarning.SetActive(manager.remainingBudget < 0);
            approvalText.text = $"Public Approval: {manager.publicApproval:F1}%";
        }

        public void OnApplyClick()
        {
            float sec = securitySlider.value * total;
            float clean = cleanlinessSlider.value * total;
            float rd = roadsSlider.value * total;

            Core.OfficeBudgetManager.Instance.ApplyBudget(sec, clean, rd);
        }

        public void OnSkimFundsClick()
        {
            // Skim 5% of remaining budget into personal funds
            var manager = Core.OfficeBudgetManager.Instance;
            if (manager != null)
            {
                float amount = manager.remainingBudget * 0.05f;
                manager.TransferToPersonal(amount);
            }
        }

        public void OnCloseClick()
        {
            gameObject.SetActive(false);
            // Re-enable player movement here if needed
        }
    }
}