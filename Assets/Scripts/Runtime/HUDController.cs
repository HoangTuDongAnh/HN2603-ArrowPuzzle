using ArrowPuzzle.Core;
using TMPro;
using UnityEngine;

namespace ArrowPuzzle.Runtime
{
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private TMP_Text remainingArrowText;
        [SerializeField] private TMP_Text removableArrowText;
        [SerializeField] private TMP_Text hintArrowText;
        [SerializeField] private TMP_Text stateText;

        public void Refresh(LevelRuntimeState runtimeState, string hintArrowId, int removableCount)
        {
            if (runtimeState == null)
            {
                SetText(remainingArrowText, "Remaining: -");
                SetText(removableArrowText, "Removable: -");
                SetText(hintArrowText, "Hint: -");
                SetText(stateText, "State: Not Started");
                return;
            }

            SetText(remainingArrowText, $"Remaining: {runtimeState.BoardState.ArrowCount}");
            SetText(removableArrowText, $"Removable: {removableCount}");
            SetText(hintArrowText, $"Hint: {(string.IsNullOrEmpty(hintArrowId) ? "(none)" : hintArrowId)}");
            SetText(stateText, $"State: {runtimeState.PlayState}");
        }

        private static void SetText(TMP_Text target, string value)
        {
            if (target != null)
            {
                target.text = value;
            }
        }
    }
}
