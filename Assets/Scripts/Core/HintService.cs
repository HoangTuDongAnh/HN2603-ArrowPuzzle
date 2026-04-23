using System;
using ArrowPuzzle.Data;

namespace ArrowPuzzle.Core
{
    public sealed class HintService
    {
        public string GetFirstRemovableArrowId(LevelRuntimeState runtimeState)
        {
            if (runtimeState == null)
            {
                throw new ArgumentNullException(nameof(runtimeState));
            }

            var removable = runtimeState.GetRemovableArrows();
            return removable.Count > 0 ? removable[0].Id : string.Empty;
        }

        public bool HasAnyHint(LevelRuntimeState runtimeState)
        {
            if (runtimeState == null)
            {
                throw new ArgumentNullException(nameof(runtimeState));
            }

            return runtimeState.GetRemovableArrows().Count > 0;
        }
    }
}
