using Vintagestory.GameContent;

namespace ApacheTech.VintageMods.Schematica.Core.Extensions
{
    public static class BlockExtensions
    {
        public static bool IsAirBlock(this Block block)
        {
            block ??= ApiEx.Current.World.GetBlock(0);
            return block.Id == 0;
        }

        public static bool IsNonCulled(this Block block)
        {
            return IsTypeNonCulled(block) || block.BlockBehaviors.Any(IsTypeNonCulled);
        }

        private static bool IsTypeNonCulled(object obj)
        {
            return NonCulledTypes.Contains(obj.GetType()) || NonCulledTypes.Contains(obj.GetType().BaseType);
        }

        private static readonly List<Type> NonCulledTypes = new()
        {
            typeof(BlockFernTree),
            typeof(BlockPlant),
            typeof(BlockVines),
            typeof(BlockLeaves),
            typeof(BlockSeaweed)
        };
    }
}