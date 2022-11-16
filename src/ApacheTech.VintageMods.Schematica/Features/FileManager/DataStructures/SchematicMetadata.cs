namespace ApacheTech.VintageMods.Schematica.Features.FileManager.DataStructures
{
    public class SchematicMetadata
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public Vec3i Size { get; set; }
        public DateTime DateCreated { get; set; }
    }
}