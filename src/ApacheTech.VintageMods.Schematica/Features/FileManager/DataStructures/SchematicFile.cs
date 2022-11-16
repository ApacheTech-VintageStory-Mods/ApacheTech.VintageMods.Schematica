namespace ApacheTech.VintageMods.Schematica.Features.FileManager.DataStructures
{
    public class SchematicFile
    {
        public SchematicMetadata Metadata { get; set; }
        public MaterialsList Materials { get; set; }
        public BlockSchematic Schematic { get; set; }
    }
}