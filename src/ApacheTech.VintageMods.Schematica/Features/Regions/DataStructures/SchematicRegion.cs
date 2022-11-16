using System.Drawing;
using ApacheTech.VintageMods.Schematica.Core.DataStructures;
using ApacheTech.VintageMods.Schematica.Core.Extensions;
using ApacheTech.VintageMods.Schematica.Features.Highlighter;

namespace ApacheTech.VintageMods.Schematica.Features.Regions.DataStructures
{
    /// <summary>
    ///     Represents an area, in which a schematic is placed.
    /// </summary>
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class SchematicRegion
    {
        private readonly IBlockHighlighter _blockHighlighter;

        private readonly int _areaHighlightId;
        private readonly int _endBlockHighlightId;
        private readonly int _startBlockHighlightId;
        public Cuboidi Cuboid => new(StartPos, EndPos);

        /// <summary>
        ///     Initialises a new instance of the <see cref="SchematicRegion" /> class.
        /// </summary>
        public SchematicRegion(IBlockHighlighter highlighter)
        {
            _blockHighlighter = highlighter;
            StartPos = ClampedBlockPos.WorldSpawn();
            EndPos = ClampedBlockPos.WorldSpawn();

            _startBlockHighlightId = _blockHighlighter.AddHighlight(Color.FromArgb(128, Color.Lime));
            _endBlockHighlightId = _blockHighlighter.AddHighlight(Color.FromArgb(128, Color.Red));
            _areaHighlightId = _blockHighlighter.AddHighlight(Color.FromArgb(128, Color.Gray));
        }

        public Vec3i Size()
        {
            // dX = Max(X) - Min(X)
            var max = Cuboid.ExclusiveUpperBounds();
            var min = Cuboid.LowerBounds();
            return max.Sub(min).ToVec3i();
        }

        /// <summary>
        ///     Determines whether or not to highlight the blocks within the selected area.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the area should be highlighted; otherwise, <c>false</c>.
        /// </value>
        public bool IsHighlighted { get; set; } = true;

        /// <summary>
        ///     Gets the specified position, at one corner of the selected cuboid area.
        /// </summary>
        /// <value>
        ///     The position at one corner of the cuboid area for the schematic, diametrically opposite to the
        ///     <see cref="EndPos" />.
        /// </value>
        public ClampedBlockPos StartPos { get; }

        /// <summary>
        ///     Gets the specified position, at one corner of the selected cuboid area.
        /// </summary>
        /// <value>
        ///     The position at one corner of the cuboid area for the schematic, diametrically opposite to the
        ///     <see cref="StartPos" />.
        /// </value>
        public ClampedBlockPos EndPos { get; }

        /// <summary>
        ///     Gets the origin position of the selected area; that being the minimum XYZ values, giving the bottom, north-west
        ///     corner of the cuboid area.
        /// </summary>
        public Vec3i OriginPos => new(
            Math.Min(StartPos.X, EndPos.X),
            Math.Min(StartPos.Y, EndPos.Y),
            Math.Min(StartPos.Z, EndPos.Z));

        /// <summary>
        ///     Gets the start position, relative to world spawn. This should only be used for display purposes.
        /// </summary>
        public Vec3i RelativeStartPos => StartPos.RelativeToSpawn().ToVec3i();

        /// <summary>
        ///     Gets the end position, relative to world spawn. This should only be used for display purposes.
        /// </summary>
        public Vec3i RelativeEndPos => EndPos.RelativeToSpawn().ToVec3i();

        /// <summary>
        ///     Gets the <see cref="BlockPos" /> at the specified relative position within the selection area.
        /// </summary>
        /// <value>
        ///     The <see cref="BlockPos" /> at the position given, relative to the origin position of the selected area.
        /// </value>
        /// <param name="relativePosition"></param>
        /// <returns></returns>
        public BlockPos this[Vec3i relativePosition] => Cuboid.GetPosition(relativePosition);

        /// <summary>
        ///     Gets the <see cref="BlockPos" /> at the specified relative position within the selection area.
        /// </summary>
        /// <value>
        ///     The <see cref="BlockPos" /> at the position given, relative to the origin position of the selected area.
        /// </value>
        /// <param name="x">The distance from the origin, on the X axis.</param>
        /// <param name="y">The distance from the origin, on the Y axis.</param>
        /// <param name="z">The distance from the origin, on the Z axis.</param>
        /// <returns></returns>
        public BlockPos this[int x, int y, int z] => Cuboid.GetPosition(new Vec3i(x, y, z));

        /// <summary>
        ///     Gets the <see cref="BlockPos" /> at the specified relative position within the selection area.
        /// </summary>
        /// <value>
        ///     The <see cref="BlockPos" /> at the position given, relative to the origin position of the selected area.
        /// </value>
        /// <param name="index"></param>
        /// <returns></returns>
        public BlockPos this[int index] => Cuboid.GetPosition(index);

        /// <summary>
        ///     Refreshes the preview highlighting for the schematic area.
        /// </summary>
        public void RefreshHighlights()
        {
            ClearHighlights();
            if (!IsHighlighted) return;
            Highlight();
        }

        public void InvokeAll(Action<BlockPos> operation) => Cuboid.InvokeAll(operation);

        private void Highlight()
        {
            _blockHighlighter.Highlight(_startBlockHighlightId, StartPos);
            _blockHighlighter.Highlight(_endBlockHighlightId, EndPos);
            _blockHighlighter.HighlightArea(_areaHighlightId, Cuboid);
        }

        private void ClearHighlights()
        {
            _blockHighlighter.ClearHighlighting(_startBlockHighlightId);
            _blockHighlighter.ClearHighlighting(_endBlockHighlightId);
            _blockHighlighter.ClearHighlighting(_areaHighlightId);
        }
    }
}