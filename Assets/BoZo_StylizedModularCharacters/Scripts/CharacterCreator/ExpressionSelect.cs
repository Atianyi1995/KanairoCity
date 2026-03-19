using UnityEngine;

namespace Bozo.ModularCharacters
{
    public class ExpressionSelect : MonoBehaviour
    {
        public OutfitSystem outfitSystem;
        public Animator animator;

        public string parameterID;

        private void OnEnable()
        {
            if (outfitSystem != null)
            {
                outfitSystem.OnOutfitChanged += GetHead;
            }
        }

        private void GetHead(Outfit outfit)
        {
            var head = outfitSystem.GetOutfit("Head");
            if (head) animator = head.GetComponentInChildren<Animator>();
        }

        private void OnDisable()
        {
            if (outfitSystem != null)
            {
                outfitSystem.OnOutfitChanged -= GetHead;
            }
        }

        public void SetExpression(float value)
        {
            animator.SetFloat(parameterID, value);
        }
    }
}