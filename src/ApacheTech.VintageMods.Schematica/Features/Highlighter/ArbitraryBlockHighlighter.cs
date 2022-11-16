using System.Drawing;

namespace ApacheTech.VintageMods.Schematica.Features.Highlighter
{
    public class ArbitraryBlockHighlighter
    {
        private readonly IBlockHighlighter _highlighter;
        private readonly int _id;
        private readonly List<BlockPos> _positions = new();

        public ArbitraryBlockHighlighter(IBlockHighlighter highlighter, Color colour)
        {
            _highlighter = highlighter;
            _id = highlighter.AddHighlight(colour);
        }

        public void Clear()
        {
            _highlighter.ClearHighlighting(_id);
            _positions.Clear();
        }

        public void AddPosition(BlockPos position)
            => _positions.AddIfNotPresent(position);

        public void Highlight()
            => _highlighter.Highlight(_id, _positions);
    }
}