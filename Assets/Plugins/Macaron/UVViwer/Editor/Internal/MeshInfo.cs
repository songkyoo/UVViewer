using System;
using System.Linq;
using UnityEngine;

namespace Macaron.UVViewer.Editor.Internal
{
    [Serializable]
    internal class MeshInfo
    {
        [SerializeField] private Mesh _mesh;
        [SerializeField] private Renderer _renderer;
        [SerializeField] private int _uvChannelFlag;
        [SerializeField] private SubMeshInfo[] _subMeshInfos;

        public MeshInfo()
        {
        }

        public MeshInfo(Mesh mesh, Renderer renderer, int uvChannelFlag, SubMeshInfo[] subMeshInfos)
        {
            if (subMeshInfos == null)
            {
                throw new ArgumentNullException("subMeshInfos");
            }

            _mesh = mesh;
            _renderer = renderer;
            _uvChannelFlag = uvChannelFlag;
            _subMeshInfos = subMeshInfos;
        }

        public Mesh Mesh
        {
            get { return _mesh; }
        }

        public Renderer Renderer
        {
            get { return _renderer; }
        }

        public int UVChannelFlag
        {
            get { return _uvChannelFlag; }
        }

        public SubMeshInfo[] SubMeshInfos
        {
            get { return _subMeshInfos; }
        }

        public int SubMeshCount
        {
            get { return _subMeshInfos.Length; }
        }

        public bool HasMesh
        {
            get { return _mesh != null && _mesh.vertexCount > 0; }
        }

        public bool HasUVChannel(int uvIndex)
        {
            switch (uvIndex)
            {
            case 0: return (_uvChannelFlag & 0x01) != 0;
            case 1: return (_uvChannelFlag & 0x02) != 0;
            case 2: return (_uvChannelFlag & 0x04) != 0;
            case 3: return (_uvChannelFlag & 0x08) != 0;
            }

            return false;
        }

        public Material[] BuildMaterials()
        {
            return _renderer != null
                ? _renderer.sharedMaterials.Distinct().Where(mat => mat != null && mat.shader != null).ToArray()
                : null;
        }
    }
}
