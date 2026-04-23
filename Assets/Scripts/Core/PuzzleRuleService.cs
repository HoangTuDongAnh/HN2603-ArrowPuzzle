using System;
using System.Collections.Generic;
using ArrowPuzzle.Data;

namespace ArrowPuzzle.Core
{
    public sealed class PuzzleRuleService
    {
        public bool CanRemoveArrow(BoardState boardState, ArrowModel arrow)
        {
            if (boardState == null)
            {
                throw new ArgumentNullException(nameof(boardState));
            }

            if (arrow == null)
            {
                throw new ArgumentNullException(nameof(arrow));
            }

            Int2 step = arrow.HeadDirection.ToInt2();
            Int2 cursor = arrow.HeadCell + step;

            while (boardState.IsInside(cursor))
            {
                if (boardState.TryGetOccupantId(cursor, out string occupantId) && occupantId != arrow.Id)
                {
                    return false;
                }

                cursor += step;
            }

            return true;
        }

        public List<ArrowModel> GetRemovableArrows(BoardState boardState)
        {
            if (boardState == null)
            {
                throw new ArgumentNullException(nameof(boardState));
            }

            List<ArrowModel> result = new List<ArrowModel>();
            foreach (ArrowModel arrow in boardState.Arrows)
            {
                if (CanRemoveArrow(boardState, arrow))
                {
                    result.Add(arrow);
                }
            }

            return result;
        }
    }
}
