using ApacheTech.Common.Extensions.Harmony;
using Vintagestory.API.Datastructures;

namespace ApacheTech.VintageMods.Schematica.Core.Extensions
{
    public static class BlockEntityExtensions
    {
        public static void FromEncodedTreeAttributes(this BlockEntity blockEntity, string data, IWorldAccessor worldAccessor)
        {
            var buffer = Ascii85.Decode(data);
            var tree = new TreeAttribute();
            using var ms = new MemoryStream(buffer);
            using var reader = new BinaryReader(ms);
            tree.FromBytes(reader);
            blockEntity.FromTreeAttributes(tree, worldAccessor);
        }

        public static TreeAttribute Attributes(this BlockEntity blockEntity)
        {
            var tree = new TreeAttribute();
            blockEntity.ToTreeAttributes(tree);
            return tree;
        }

        public static bool IsSameAs(this BlockEntity @this, BlockEntity other)
        {
            var ignoredPaths = new[] { "posx", "posy", "posz" };
            var thisAttributes = @this.Attributes();
            var otherAttributes = other.Attributes();
            return thisAttributes.Equals(ApiEx.ClientMain, otherAttributes, ignoredPaths);
        }
    }
}