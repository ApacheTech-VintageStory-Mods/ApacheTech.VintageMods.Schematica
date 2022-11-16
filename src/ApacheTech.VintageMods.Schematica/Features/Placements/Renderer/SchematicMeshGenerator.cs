#define CACHE_MESHES

namespace ApacheTech.VintageMods.Schematica.Features.Placements.Renderer
{
    public class SchematicMeshGenerator : ITerrainMeshPool
#if CACHE_MESHES
        , IDisposable
#endif
    {
        private readonly Dictionary<int, MeshRef> _blockMeshRefCache = new();
        private readonly ICoreClientAPI _capi;
        private MeshData _meshData = new();

        public SchematicMeshGenerator(ICoreClientAPI capi)
            => _capi = capi;

        public void AddMeshData(MeshData data, int lodLevel = 1)
            => _meshData = data.Clone();

        public void AddMeshData(MeshData data, ColorMapData colorMapData, int lodLevel = 1)
            => _meshData = data.Clone();

        public MeshRef GenerateMeshRef(Block block, BlockEntity blockEntity)
        {
#if CACHE_MESHES
            var hashCode = blockEntity?.GetHashCode() ?? block.GetHashCode();
        if (_blockMeshRefCache.ContainsKey(hashCode)) return _blockMeshRefCache[hashCode];
#endif
            MeshData meshData;
            try
            {
                _meshData.Clear();
                _capi.Tesselator.TesselateBlock(block, out meshData);
                if (blockEntity?.OnTesselation(this, _capi.Tesselator) ?? false)
                {
                    meshData = _meshData.Clone();
                }
            }
            catch (NullReferenceException)
            {
                meshData = _capi.TesselatorManager.To<ShapeTesselatorManager>().unknownBlockModelData;
            }

            meshData ??= _capi.TesselatorManager.To<ShapeTesselatorManager>().unknownBlockModelData;
            var meshRef = _capi.Render.UploadMesh(meshData.Clone());
#if CACHE_MESHES
            _blockMeshRefCache.Add(hashCode, meshRef);
#endif
            return meshRef;
        }

#if CACHE_MESHES
        public void Dispose()
        {
            ClearCache();
        }

        public void ClearCache()
        {
            foreach (var meshRef in _blockMeshRefCache.Values)
            {
                _capi.Render.DeleteMesh(meshRef);
            }
            _blockMeshRefCache.Clear();
        }
#endif
    }
}