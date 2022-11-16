namespace ApacheTech.VintageMods.Schematica.Core.Extensions
{
    public static class CuboidExtensions
    {
        public static IEnumerable<T> MapAll<T>(this Cuboidi @this, System.Func<BlockPos, T> f)
        {
            for (var y = @this.MinY; y <= @this.MaxY; y++)
                for (var x = @this.MinX; x <= @this.MaxX; x++)
                    for (var z = @this.MinZ; z <= @this.MaxZ; z++)
                        yield return f(new BlockPos(x, y, z));
        }
        public static void InvokeAll(this Cuboidi @this, Action<BlockPos> f)
        {
            for (var y = @this.MinY; y <= @this.MaxY; y++)
                for (var x = @this.MinX; x <= @this.MaxX; x++)
                    for (var z = @this.MinZ; z <= @this.MaxZ; z++)
                        f(new BlockPos(x, y, z));
        }

        public static BlockPos GetPosition(this Cuboidi @this, Vec3i relativePosition) 
            => @this.LowerBounds().AddCopy(relativePosition);

        public static BlockPos GetPosition(this Cuboidi @this, int index)
        {
            var z = index;
            var y = z / @this.SizeXZ;
            z -= y * @this.SizeXZ;
            var x = z / @this.SizeZ;
            z -= x * @this.SizeZ;

            return @this.GetPosition(new Vec3i(x, y, z));
        }

        public static IEnumerable<BlockPos> Flatten(this Cuboidi cuboid)
        {
            for (var y = cuboid.MinY; y <= cuboid.MaxY; y++)
                for (var x = cuboid.MinX; x <= cuboid.MaxX; x++)
                    for (var z = cuboid.MinZ; z <= cuboid.MaxZ; z++)
                        yield return new BlockPos(x, y, z);
        }

        public static BlockPos ExclusiveLowerBounds(this Cuboidi cuboid)
            => new(cuboid.MinX - 1, cuboid.MinY - 1, cuboid.MinZ - 1);

        public static BlockPos LowerBounds(this Cuboidi cuboid)
            => new(cuboid.MinX, cuboid.MinY, cuboid.MinZ);

        public static BlockPos UpperBounds(this Cuboidi cuboid)
            => new(cuboid.MaxX, cuboid.MaxY, cuboid.MaxZ);

        public static BlockPos ExclusiveUpperBounds(this Cuboidi cuboid)
            => new(cuboid.MaxX + 1, cuboid.MaxY + 1, cuboid.MaxZ + 1);
    }

    public static class RegionEx
    {
        public static BlockPos ExclusiveLowerBounds(BlockPos startPos, BlockPos endPos) 
            => new Cuboidi(startPos, endPos).ExclusiveLowerBounds();

        public static BlockPos LowerBounds(BlockPos startPos, BlockPos endPos) 
            => new Cuboidi(startPos, endPos).LowerBounds();

        public static BlockPos UpperBounds(BlockPos startPos, BlockPos endPos) 
            => new Cuboidi(startPos, endPos).UpperBounds();

        public static BlockPos ExclusiveUpperBounds(BlockPos startPos, BlockPos endPos) 
            => new Cuboidi(startPos, endPos).ExclusiveUpperBounds();
    }
}