using Vintagestory.GameContent;

namespace ApacheTech.VintageMods.Schematica.Services
{
    public static class Extensions
    {
        public static Vec2f GetOffset(this BlockPos pos, Block block)
        {
            float offX = 0, offZ = 0;

            if (block.RandomDrawOffset <= 0) return new Vec2f(offX, offZ);
            offX = GameMath.oaatHash(pos.X, 0, pos.Z) % 12 / 36f;
            offZ = GameMath.oaatHash(pos.X, 1, pos.Z) % 12 / 36f;
            return new Vec2f(offX, offZ);
        }

        public static Vec4f Add(this Vec4f a, Vec4f b)
        {
            a.X += b.X;
            a.Y += b.Y;
            a.Z += b.Z;
            return a;
        }

        public static float GetRotY(this Block block, Vec3d entityPos, BlockSelection blockSel)
        {
            var targetPos = blockSel.DidOffset
                ? blockSel.Position.AddCopy(blockSel.Face.Opposite)
                : blockSel.Position;

            if (targetPos is null) return 0;

            const float oneEighthPi = GameMath.PIHALF / 4;

            var dx = entityPos.X - (targetPos.X + blockSel.HitPosition.X);
            var dz = entityPos.Z - (targetPos.Z + blockSel.HitPosition.Z);
            var angleHor = (float)Math.Atan2(dx, dz);

            switch (block)
            {
                case BlockAnvil:
                {
                    return (int)Math.Round(angleHor / oneEighthPi) * oneEighthPi;
                }
                case BlockBucket:
                {
                    return (int)Math.Round(angleHor / oneEighthPi) * oneEighthPi;
                }
                case BlockGenericTypedContainer:
                {
                    var strAngle = block.Attributes?["rotatatableInterval"]["normal-generic"]?.AsString("22.5deg") ?? "22.5deg";
                    if (strAngle == "22.5degnot45deg")
                    {
                        var num3 = (int)Math.Round(angleHor / GameMath.PIHALF) * GameMath.PIHALF;
                        return (Math.Abs(angleHor - num3) < (double)oneEighthPi
                            ? num3
                            : num3 + oneEighthPi * Math.Sign(angleHor - num3)) + GameMath.DEG2RAD * 90;
                    }
                    if (strAngle != "22.5deg") return 0;

                    var num6 = (int)Math.Round(angleHor / (double)oneEighthPi) * oneEighthPi;
                    return num6 + GameMath.DEG2RAD * 90;
                }
            }

            return 0;
        }

        public static BlockEntity GetBlockEntity(this BlockPos pos, IWorldAccessor world)
            => world.BlockAccessor.GetBlockEntity(pos);

        public static BlockEntity GetBlockEntity(this BlockPos pos, ICoreAPI api)
            => pos.GetBlockEntity(api.World);

        public static Block GetBlock(this BlockPos pos, IWorldAccessor world)
            => world.BlockAccessor.GetBlock(pos);

        public static Block GetBlock(this BlockPos pos, ICoreAPI api)
            => pos.GetBlock(api.World);

        public static Block GetBlock(this AssetLocation asset, ICoreAPI api)
            => api.World.BlockAccessor.GetBlock(asset);

        public static Item GetItem(this AssetLocation asset, ICoreAPI Api)
            => Api.World.GetItem(asset);
    }
}