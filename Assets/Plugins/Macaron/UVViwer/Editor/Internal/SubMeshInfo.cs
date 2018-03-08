using System;
using UnityEngine;

namespace Macaron.UVViewer.Editor.Internal
{
    [Serializable]
    internal class SubMeshInfo
    {
        [SerializeField] private MeshTopology _topology;
        [SerializeField] private uint _indexCount;

        public SubMeshInfo(MeshTopology topology, uint indexCount)
        {
            _topology = topology;
            _indexCount = indexCount;
        }

        public MeshTopology Topology
        {
            get { return _topology; }
        }

        public uint IndexCount
        {
            get { return _indexCount; }
        }

        public uint ElementCount
        {
            get
            {
                switch (_topology)
                {
                case MeshTopology.Points: return _indexCount;
                case MeshTopology.Lines: return _indexCount / 2;
                case MeshTopology.LineStrip: return _indexCount -1;
                case MeshTopology.Triangles: return _indexCount / 3;
                case MeshTopology.Quads: return _indexCount / 4;
                }

                return 0;
            }
        }
    }
}
