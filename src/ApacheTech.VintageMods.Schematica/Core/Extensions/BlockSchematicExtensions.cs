using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;

namespace ApacheTech.VintageMods.Schematica.Core.Extensions
{
    public static class BlockSchematicExtensions
    {
        public static Vec3i Size(this BlockSchematic schematic) 
            => new(schematic.SizeX, schematic.SizeY, schematic.SizeZ);

        public static Dictionary<int, int> ExtractBlocksList(this BlockSchematic schematic)
            => schematic.BlockIds.GroupBy(s => s).ToDictionary(g => g.Key, g => g.Count());
        public static Dictionary<int, int> ExtractDecorList(this BlockSchematic schematic)
            => schematic.DecorIds.GroupBy(s => s).ToDictionary(g => g.Key, g => g.Count());
        public static Dictionary<string, int> ExtractItemsList(this BlockSchematic schematic)
            => schematic.ItemCodes.GroupBy(s => s.Value).ToDictionary(g => g.Key.Path.ToString(), g => g.Count());
        public static Dictionary<string, int> ExtractEntitiesList(this BlockSchematic schematic)
            => schematic.Entities.GroupBy(s => s).ToDictionary(g => g.Key, g => g.Count());
        public static Dictionary<BlockPos, Block> BlocksInPosition(this BlockSchematic schematic, BlockPos startPos)
            => schematic.BlocksUnpacked.ToDictionary(
                kvp => kvp.Key.AddCopy(startPos.ToVec3i()),
                kvp => schematic.GetBlock(kvp.Value));
        public static Dictionary<BlockPos, Block> FluidBlocksInPosition(this BlockSchematic schematic, BlockPos startPos)
            => schematic.FluidsLayerUnpacked.ToDictionary(
                kvp => kvp.Key.AddCopy(startPos.ToVec3i()),
                kvp => schematic.GetBlock(kvp.Value));
        public static Dictionary<BlockPos, BlockEntity> BlockEntitiesInPosition(this BlockSchematic schematic, BlockPos startPos)
            => schematic.BlockEntitiesUnpacked.ToDictionary(
                kvp => kvp.Key.AddCopy(startPos.ToVec3i()),
                kvp => schematic.GetBlockEntity(kvp.Key, startPos));
        public static Dictionary<BlockPos, Block[]> DecorInPosition(this BlockSchematic schematic, BlockPos startPos)
            => schematic.DecorsUnpacked.ToDictionary(
                kvp => kvp.Key.AddCopy(startPos.ToVec3i()),
                kvp => kvp.Value);
        public static Dictionary<BlockPos, List<Entity>> EntitiesInPosition(this BlockSchematic schematic, BlockPos startPos)
        {
            var dict = new Dictionary<BlockPos, List<Entity>>();
            foreach (var entity in schematic.EntitiesUnpacked)
            {
                if (!dict.ContainsKey(entity.Pos.AsBlockPos))
                {
                    dict.Add(entity.Pos.AsBlockPos, new List<Entity>());
                }
                dict[entity.Pos.AsBlockPos].Add(entity);
            }
            return dict;
        }

        private static Block GetBlock(this BlockSchematic schematic, int blockId) 
            => ApiEx.Current.World.BlockAccessor.GetBlock(schematic.BlockCodes[blockId]);

        private static BlockEntity GetBlockEntity(this BlockSchematic schematic, BlockPos position, BlockPos startPos)
        {
            try
            {
                var capi = ApiEx.Client;
                var storedBlockId = schematic.BlocksUnpacked[position];
                var code = schematic.BlockCodes[storedBlockId];
                var block = capi.World.BlockAccessor.GetBlock(code);
                var schematicSeed = capi.World.Rand.Next();

                var blockEntity = capi.ClassRegistry.CreateBlockEntity(block.EntityClass);
                blockEntity.Block = block;
                blockEntity.CreateBehaviors(block, capi.World);
                blockEntity.Initialize(capi);

                var data = schematic.BlockEntitiesUnpacked[position];
                position.Add(startPos.ToVec3i());
                ITreeAttribute tree = schematic.DecodeBlockEntityData(data);
                tree.SetInt("posx", position.X);
                tree.SetInt("posy", position.Y);
                tree.SetInt("posz", position.Z);
                blockEntity.FromTreeAttributes(tree, capi.World);
                blockEntity.OnLoadCollectibleMappings(capi.World, schematic.BlockCodes, schematic.ItemCodes, schematicSeed);
                return blockEntity;
            }
            catch
            {
                return null;
            }
        }

        public static void Unpack(this BlockSchematic schematic, BlockPos startPos)
        {
            schematic.BlocksUnpacked.Clear();
            schematic.BlockEntitiesUnpacked.Clear();
            schematic.FluidsLayerUnpacked.Clear();
            schematic.DecorsUnpacked.Clear();
            schematic.EntitiesUnpacked.Clear();

            for (var i = 0; i < schematic.Indices.Count; i++)
            {
                // Relative BlockPos
                var num = schematic.Indices[i];
                var x = (int)(num & 511U);
                var y = (int)(num >> 20 & 511U);
                var z = (int)(num >> 10 & 511U);
                var position = new BlockPos(x, y, z);

                // Blocks
                var storedBlockId = schematic.BlockIds[i];
                var block = schematic.GetBlock(storedBlockId);

                block.ForFluidsLayer.IfElse(
                    () =>
                    {
                        schematic.FluidsLayerUnpacked.Add(position, storedBlockId);
                    },
                    () =>
                    {
                        schematic.BlocksUnpacked.Add(position, storedBlockId);
                    });

                // Block Entities
                if (!schematic.BlockEntities.ContainsKey(num)) continue;
                var storedBlockEntityData = schematic.BlockEntities[num];
                schematic.BlockEntitiesUnpacked.Add(position, storedBlockEntityData);
            }

            // Entities
            foreach (var encoded in schematic.Entities)
            {
                using var ms = new MemoryStream(Ascii85.Decode(encoded));
                using var reader = new BinaryReader(ms);
                var className = reader.ReadString();
                var entity = ApiEx.Client.ClassRegistry.CreateEntity(className);
                var schematicSeed = ApiEx.Client.World.Rand.Next();

                entity.FromBytes(reader, false);
                entity.DidImportOrExport(startPos);
                entity.OnLoadCollectibleMappings(ApiEx.ClientMain, schematic.BlockCodes, schematic.ItemCodes, schematicSeed);
                schematic.EntitiesUnpacked.Add(entity);
            }

            // Decor
            for (var j = 0; j < schematic.DecorIndices.Count; j++)
            {
                var num = schematic.DecorIndices[j];
                var x = (int)(num & 511U);
                var y = (int)(num >> 20 & 511U);
                var z = (int)(num >> 10 & 511U);
                var position = new BlockPos(x, y, z);
                schematic.DecorsUnpacked.AddIfNotPresent(position, new Block[6]);

                var storedDecorBlockId = schematic.DecorIds[j];
                var faceIndex = (byte)(storedDecorBlockId >> 24);
                if (faceIndex > 5) continue;
                storedDecorBlockId &= 16777215;
                var decorBlock = schematic.GetBlock(storedDecorBlockId);
                schematic.DecorsUnpacked[position][faceIndex] = decorBlock;
            }
        }
    }
}