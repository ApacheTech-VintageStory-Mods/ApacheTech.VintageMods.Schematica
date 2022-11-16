namespace ApacheTech.VintageMods.Schematica.Features.FileManager.DataStructures
{
    public class MaterialsList
    {
        public Dictionary<int, int> Blocks { get; set; }
        public Dictionary<int, int> Decor { get; set; }
        public Dictionary<string, int> Items { get; set; }
        public Dictionary<string, int> Entities { get; set; }
    }
}
