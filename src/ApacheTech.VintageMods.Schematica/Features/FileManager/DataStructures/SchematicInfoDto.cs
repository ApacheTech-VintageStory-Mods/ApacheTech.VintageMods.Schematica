namespace ApacheTech.VintageMods.Schematica.Features.FileManager.DataStructures
{
    public class SchematicInfoDto
    {
        public FileInfo File { get; set; }
        public SchematicMetadata Metadata { get; set; }
        public MaterialsList Materials { get; set; }
    }
}