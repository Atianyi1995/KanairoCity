using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Kanairo.Core;

namespace Kanairo.UI
{
    public class NPCWorldUI : MonoBehaviour
    {
        [Header("References")]
        public NPCVotingProfile targetProfile;
        public Image progressBarFill;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI statusText;
        public CanvasGroup canvasGroup;
        public GameObject bribeButton;
        public TextMeshProUGUI bribeCostText;

        [Header("Settings")]
        public float fadeDistance = 10f;
        public float smoothSpeed = 5f;
        public float bribeBaseCost = 50000f;

        [Header("Colors")]
        public Color opposedColor = Color.red;
        public Color neutralColor = Color.gray;
        public Color leaningColor = Color.yellow;
        public Color supporterColor = Color.green;

        private float currentDisplayFill = 0f;
        public Transform playerTransform;

        private void Start()
        {
            if (targetProfile == null) targetProfile = GetComponentInParent<NPCVotingProfile>();
            if (targetProfile != null) nameText.text = targetProfile.npcName;
            
            canvasGroup.alpha = 0;
            if (bribeButton != null) bribeButton.SetActive(false);
        }

        private void Update()
        {
            if (targetProfile == null) return;

            if (playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) playerTransform = player.transform;
                else if (Camera.main != null) playerTransform = Camera.main.transform;
            }

            if (playerTransform != null)
            {
                float dist = Vector3.Distance(transform.position, playerTransform.position);
                bool inRange = dist < fadeDistance;
                float targetAlpha = inRange ? 1f : 0f;
                
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.deltaTime * smoothSpeed);
                    canvasGroup.blocksRaycasts = (canvasGroup.alpha > 0.1f);
                    canvasGroup.interactable = (canvasGroup.alpha > 0.1f);
                }

                if (bribeButton != null)
                {
                    bool canBribe = inRange && GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.InOffice;
                    if (targetProfile.trustInPlayer >= 75f) canBribe = false;

                    bribeButton.SetActive(canBribe);
                    if (canBribe && bribeCostText != null)
                    {
                        bribeCostText.text = $"Bribe: ${bribeBaseCost:N0}";
                    }
                }
            }
            else
            {
                if (canvasGroup != null) canvasGroup.alpha = 1f;
            }

            float targetFill = Mathf.Clamp01(targetProfile.trustInPlayer / 100f);
            // Faster lerp for more immediate feedback
            currentDisplayFill = Mathf.Lerp(currentDisplayFill, targetFill, Time.deltaTime * 10f); 
            if (progressBarFill != null) progressBarFill.fillAmount = currentDisplayFill;

            UpdateStatus(targetProfile.trustInPlayer);
        }

        private void UpdateStatus(float trust)
        {
            if (trust >= 75f)
            {
                statusText.text = "Supporter";
                statusText.color = supporterColor;
                progressBarFill.color = supporterColor;
            }
            else if (trust >= 50f)
            {
                statusText.text = "Leaning Player";
                statusText.color = leaningColor;
                progressBarFill.color = leaningColor;
            }
            else if (trust >= 25f)
            {
                statusText.text = "Neutral";
                statusText.color = neutralColor;
                progressBarFill.color = neutralColor;
            }
            else
            {
                statusText.text = "Opposed";
                statusText.color = opposedColor;
                progressBarFill.color = opposedColor;
            }
        }

        public void OnBribeClick()
        {
            if (OfficeBudgetManager.Instance != null)
            {
                if (OfficeBudgetManager.Instance.TryBribe(targetProfile, bribeBaseCost))
                {
                    Debug.Log($"Bribe successful for {targetProfile.npcName}");
                }
            }
        }
    }
}
