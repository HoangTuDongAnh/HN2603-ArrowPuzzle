namespace ArrowPuzzle.LevelEditor
{
    [System.Serializable]
    public class EditorSelection
    {
        public string SelectedArrowId { get; private set; }

        public bool HasSelection => !string.IsNullOrEmpty(SelectedArrowId);

        public void Select(string arrowId)
        {
            SelectedArrowId = arrowId;
        }

        public void Clear()
        {
            SelectedArrowId = string.Empty;
        }

        public bool IsSelected(string arrowId)
        {
            return !string.IsNullOrEmpty(arrowId) && SelectedArrowId == arrowId;
        }
    }
}
