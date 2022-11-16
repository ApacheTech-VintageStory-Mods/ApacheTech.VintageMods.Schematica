using ApacheTech.VintageMods.Schematica.Core.DataStructures;
using ApacheTech.VintageMods.Schematica.Features.FileManager.DataStructures;

namespace ApacheTech.VintageMods.Schematica.Features.Placements.Renderer
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class LoadedSchematic
    {
        public bool Enabled { get; set; }

        public SchematicMetadata Metadata { get; set; }
        public BlockSchematic Schematic { get; set; }
        public ClampedBlockPos StartPos { get; set; }

        public bool HighlightGhostBlocks { get; set; }
        public bool ShowGhostBlocks { get; set; } = true;
        public bool ShowSolidBlocks { get; set; } = true;
        public bool ShowLiquids { get; set; } = true;
        public bool ShowDecor { get; set; }
        public bool ShowEntities { get; set; }

        public SchematicLayerType RenderLayers { get; set; } = SchematicLayerType.All;
        public int Layer { get; set; }

        public int Rotation { get; set; }

        public bool RequiresRedraw { get; set; }
    }
}