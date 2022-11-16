namespace ApacheTech.VintageMods.Schematica.Core.DataStructures
{
    public class CellEntry<T> : SavegameCellEntry
    {
        public T Model { get; set; }
    }
}