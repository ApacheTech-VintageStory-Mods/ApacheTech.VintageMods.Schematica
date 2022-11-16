using ApacheTech.VintageMods.Schematica.Core.Extensions;
using ApacheTech.VintageMods.Schematica.Features.Highlighter;
using ApacheTech.VintageMods.Schematica.Services;
using Vintagestory.API.Common.Entities;

namespace ApacheTech.VintageMods.Schematica.Features.Placements.Renderer
{
    [UsedImplicitly]
    public class SchematicPlacementRenderer : IRenderer
    {
        private readonly ICoreClientAPI _capi;

        private readonly PlacementPreviewHighlighter _highlighter;
        private readonly SchematicMeshGenerator _schematicMeshGenerator;
        private readonly LoadedSchematic _schematic;
        private readonly Matrixf _matrix = new();

        private Dictionary<BlockPos, BlockEntity> _blockEntityPositions;
        private Dictionary<BlockPos, Block> _blockPositions;
        private Dictionary<BlockPos, Block> _fluidBlockPositions;
        private Dictionary<BlockPos, Block[]> _decorPositions;
        private Dictionary<BlockPos, List<Entity>> _entityPositions;
        private IEnumerable<BlockPos> _allPositions;

        public SchematicPlacementRenderer(
            ICoreClientAPI capi,
            LoadedSchematic schematic,
            PlacementPreviewHighlighter highlighter)
        {
            _capi = capi;
            _schematic = schematic;
            _highlighter = highlighter;
            _schematicMeshGenerator = new SchematicMeshGenerator(_capi);
            SetPlacementPositions(_schematic.StartPos);
        }

        private bool _redrawing;
        private IStandardShaderProgram _prog;

        public void SetPlacementPositions(BlockPos startPos)
        {
            if (_redrawing) return;
            _redrawing = true;
            
            _highlighter.ClearHighlights();
            _schematicMeshGenerator.ClearCache();

            _schematic.Schematic.Unpack(startPos);

            _allPositions = new Cuboidi(startPos, startPos.AddCopy(_schematic.Schematic.Size())).Flatten();

            _blockPositions = _schematic.Schematic.BlocksInPosition(startPos);
            _fluidBlockPositions = _schematic.Schematic.FluidBlocksInPosition(startPos);
            _blockEntityPositions = _schematic.Schematic.BlockEntitiesInPosition(startPos);
            _decorPositions = _schematic.Schematic.DecorInPosition(startPos);
            _entityPositions = _schematic.Schematic.EntitiesInPosition(startPos);

            _schematic.RequiresRedraw = _redrawing = false;
        }

        private Vec3d CameraPosition => _capi.World.Player.Entity.CameraPos;

        public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
            if (!_schematic.Enabled) return;
            if (_schematic.Schematic is null) return;
            if (_schematic.RequiresRedraw || _allPositions.First() != _schematic.StartPos)
            {
                SetPlacementPositions(_schematic.StartPos);
            }

            // For EVERY block position.
            _highlighter.ClearHighlights();
            foreach (var position in _allPositions)
            {
                if (!ShouldRenderLayer(position)) continue;
                RenderBlockPos(_capi.World.BlockAccessor, position);
            }
            _highlighter.RefreshHighlights();
        }

        private bool ShouldRenderLayer(BlockPos position)
        {
            var layer = position.Y - _schematic.StartPos.Y;
            return _schematic.RenderLayers switch
            {
                SchematicLayerType.None => false,
                SchematicLayerType.SingleLayer => _schematic.Layer == layer,
                SchematicLayerType.AllBelow => _schematic.Layer >= layer,
                SchematicLayerType.AllAbove => _schematic.Layer <= layer,
                SchematicLayerType.All => true,
                _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, "Layer out of bounds, for the given schematic.")
            };
        }

        public double RenderOrder => 0.5f;
        public int RenderRange => 24;

        public void Dispose()
        {
            _blockPositions?.Clear();
            _blockEntityPositions?.Clear();
            _highlighter?.ClearHighlights();
            _schematicMeshGenerator?.Dispose();
        }

        private void RenderBlockPos(IBlockAccessor blockAccessor, BlockPos position)
        {
            RenderFluids(blockAccessor, position);
            RenderSolidBlocks(blockAccessor, position);
            RenderDecor(blockAccessor, position);
            RenderEntities(blockAccessor, position);
        }

        private void RenderSolidBlocks(IBlockAccessor blockAccessor, BlockPos position)
        {
            if (!_schematic.ShowSolidBlocks) return;

            _blockEntityPositions.TryGetValue(position, out var schematicBlockEntity);

            if (!_blockPositions.TryGetValue(position, out var schematicBlock))
            {
                schematicBlock = blockAccessor.GetBlock(0);
            }

            RenderSchematicBlock(blockAccessor, position, schematicBlock, schematicBlockEntity, BlockRenderType.Solid, -1);
        }

        private void RenderFluids(IBlockAccessor blockAccessor, BlockPos position)
        {
            if (!_schematic.ShowLiquids) return;
            if (!_fluidBlockPositions.TryGetValue(position, out var schematicFluidBlock))
            {
                schematicFluidBlock = blockAccessor.GetBlock(0);
            }

            RenderSchematicBlock(blockAccessor, position, schematicFluidBlock, null, BlockRenderType.Fluid, -1);
        }

        private void RenderDecor(IBlockAccessor blockAccessor, BlockPos position)
        {
            if (!_schematic.ShowDecor) return;
            if (!_decorPositions.TryGetValue(position, out var schematicDecorBlocks))
            {
                schematicDecorBlocks = new Block[6];
            }

            for (var i = 0; i < 6; i++)
            {
                var decorBlock = schematicDecorBlocks[i];
                if (decorBlock is null) continue;
                RenderSchematicBlock(blockAccessor, position, decorBlock, null, BlockRenderType.Decor, i);
            }
        }

        private void RenderEntities(IBlockAccessor blockAccessor, BlockPos position)
        {
            if (!_schematic.ShowEntities) return;
            if (!_entityPositions.TryGetValue(position, out var entities)) return;
            foreach (var entity in entities)
            {
                // TODO: Render Entity, in place.
            }
        }
        
        private void RenderSchematicBlock(IBlockAccessor blockAccessor, BlockPos position, Block schematicBlock, BlockEntity schematicBlockEntity, BlockRenderType type, int faceIndex)
        {
            var placedBlock = type switch
            {
                BlockRenderType.Solid => blockAccessor.GetBlock(position, BlockLayersAccess.MostSolid),
                BlockRenderType.Fluid => blockAccessor.GetBlock(position, BlockLayersAccess.Fluid),
                BlockRenderType.Decor => blockAccessor.GetDecor(position, faceIndex),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
            placedBlock ??= blockAccessor.GetBlock(0);
            var placedBlockEntity = blockAccessor.GetBlockEntity(position);


            if (schematicBlock.IsAirBlock())
            {
                // If schematic is AIR and current is AIR then [CONTINUE]
                if (placedBlock.IsAirBlock()) return;
                if (type is BlockRenderType.Fluid) return;

                // If schematic is AIR and current is BLOCK then [RENDER PURPLE CUBE ALPHA .25]
                _highlighter.AddPurpleHighlight(position);
                return;
            }

            if (placedBlock.IsAirBlock())
            {
                // If schematic is BLOCK and current is AIR then [RENDER GHOST BLOCK WITH BLUE HUE ALPHA .75]
                if (_schematic.HighlightGhostBlocks) _highlighter.AddBlueHighlight(position);

                if (!_schematic.ShowGhostBlocks) return;
                var offset = GetDecorOffset(faceIndex);
                RenderGhostBlock(position, schematicBlock, schematicBlockEntity, offset);
                return;
            }

            if (placedBlock.Code.FirstCodePart() == schematicBlock.Code.FirstCodePart())
            {
                // If schematic is BLOCK and current is SAME BLOCK with DIFFERENT ATTRIBUTES then [RENDER ORANGE CUBE ALPHA .25]
                if (schematicBlockEntity is not null && placedBlockEntity is not null)
                {
                    if (schematicBlockEntity.IsSameAs(placedBlockEntity)) return;
                    _highlighter.AddOrangeHighlight(position);
                    return;
                }

                // If schematic is BLOCK and current is SAME BLOCK with SAME PATH then [CONTINUE]
                if (schematicBlock.Code.Path.Equals(placedBlock.Code.Path)) return;

                // If schematic is BLOCK and current is SAME BLOCK with DIFFERENT PATH then [RENDER ORANGE CUBE ALPHA .25]
                _highlighter.AddOrangeHighlight(position);
                return;
            }

            // If schematic is BLOCK and current is DIFFERENT BLOCK then [RENDER RED CUBE ALPHA .25]
            _highlighter.AddRedHighlight(position);
        }

        private static Vec3f GetDecorOffset(int faceIndex)
        {
            return faceIndex switch
            {
                BlockFacing.indexUP => new Vec3f(0, 1.0001f, 0),
                BlockFacing.indexDOWN => new Vec3f(0, -0.0001f, 0),
                BlockFacing.indexNORTH => new Vec3f(0, 0, -0.0001f),
                BlockFacing.indexSOUTH => new Vec3f(0, 0, 1.0001f),
                BlockFacing.indexEAST => new Vec3f(1.0001f, 0, 0),
                BlockFacing.indexWEST => new Vec3f(-0.0001f, 0, 0),
                _ => new Vec3f(0, 0, 0)
            };
        }

        private void RenderGhostBlock(BlockPos position, Block schematicBlock, BlockEntity blockEntity, Vec3f decorOffset)
        {
            if (schematicBlock.IsAirBlock() || _redrawing) return;
            var meshRef = _schematicMeshGenerator.GenerateMeshRef(schematicBlock, blockEntity);
            if (meshRef.Disposed) return;

            _capi.Render.GlToggleBlend(true);
            if (schematicBlock.IsNonCulled()) _capi.Render.GlDisableCullFace();

            var offset = position.GetOffset(schematicBlock);
            _prog = _capi.Render.PreparedStandardShader(position.X, position.Y, position.Z);
            
            _prog.Tex2D = _capi.BlockTextureAtlas.AtlasTextureIds[0];
            _prog.ModelMatrix = _matrix
                .Identity()
                .Translate(
                    position.X - CameraPosition.X,
                    position.Y - CameraPosition.Y,
                    position.Z - CameraPosition.Z)
                .Translate(offset.X, 0, offset.Y)
                .Translate(decorOffset.X, decorOffset.Y, decorOffset.Z)
                .Values;

            _prog.ViewMatrix = _capi.Render.CameraMatrixOriginf;
            _prog.ProjectionMatrix = _capi.Render.CurrentProjectionMatrix;

            _prog.SsaoAttn = 0;
            _prog.AlphaTest = 0.05f;
            _prog.RgbaTint = new Vec4f(1.0f, 1.0f, 1.0f, 0.75f);
            _prog.RgbaGlowIn = new Vec4f(1.0f, 1.0f, 1.0f, 1.0f);
            _prog.ExtraGlow = 255 / (int)(_capi.World.Calendar.SunLightStrength * 64.0f);
            _capi.Render.RenderMesh(meshRef);
            _prog.Stop();
            _capi.Render.GlEnableCullFace();
        }
    }
}