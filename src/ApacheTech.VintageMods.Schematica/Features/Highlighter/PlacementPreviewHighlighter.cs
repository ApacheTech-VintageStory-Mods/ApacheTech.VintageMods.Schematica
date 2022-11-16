using System.Drawing;

namespace ApacheTech.VintageMods.Schematica.Features.Highlighter
{
    public class PlacementPreviewHighlighter
    {
        private readonly Dictionary<string, ArbitraryBlockHighlighter> _highlighters;

        public PlacementPreviewHighlighter(IBlockHighlighter highlighter)
        {
            _highlighters = new Dictionary<string, ArbitraryBlockHighlighter>
            {
                { "Blue", new(highlighter, Color.FromArgb(0x40, Color.CornflowerBlue)) },
                { "Orange", new(highlighter, Color.FromArgb(0x40, Color.Orange)) },
                { "Purple", new(highlighter, Color.FromArgb(0x40, Color.Purple)) },
                { "Red", new(highlighter, Color.FromArgb(0x40, Color.Red)) }
            };
        }

        public void ClearHighlights()
        {
            foreach (var highlighter in _highlighters.Values)
            {
                highlighter.Clear();
            }
        }

        public void RefreshHighlights()
        {
            foreach (var highlighter in _highlighters.Values)
            {
                highlighter.Highlight();
            }
        }

        public void AddBlueHighlight(BlockPos position)
            => _highlighters["Blue"].AddPosition(position);

        public void AddOrangeHighlight(BlockPos position)
            => _highlighters["Orange"].AddPosition(position);

        public void AddPurpleHighlight(BlockPos position)
            => _highlighters["Purple"].AddPosition(position);

        public void AddRedHighlight(BlockPos position)
            => _highlighters["Red"].AddPosition(position);
    }
}