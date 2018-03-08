using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Macaron.UVViewer.Editor.Internal;

namespace Macaron.UVViewer.Editor
{
    partial class UVViewerWindow
    {
        private enum DragObjectType
        {
            None,
            Mesh,
            MeshFilter,
            SkinnedMeshFilter,
            Texture2D
        }

        private const float _minViewHeight = 100.0f;
        private const double _minViewScale = 1.0;
        private const double _maxViewScale = 10000000.0;
        private const double _zoomIntensity = 0.2 / 3.0; // 윈도우 기본 휠 값은 3.
        private static readonly Vector2Double _defaultViewPivot = new Vector2Double(0.5, 0.5);

        // ViewGridGroup0: 0.0
        private static readonly Vector3[] _viewGridGroup0LineSegments = new[]
        {
            new Vector3(0.0f, 0.0f), new Vector3(1.0f, 0.0f),
            new Vector3(0.0f, 1.0f), new Vector3(0.0f, 0.0f),
        };
        private static readonly GridTextInfo[] _viewGridGroup0TextInfos = new[]
        {
            new GridTextInfo(0.0f, 0.0f, "0.0"),
        };
        // ViewGridGroup1: 1.0
        private static readonly Vector3[] _viewGridGroup1LineSegments = new[]
        {
            new Vector3(0.0f, 1.0f), new Vector3(1.0f, 1.0f),
            new Vector3(1.0f, 1.0f), new Vector3(1.0f, 0.0f)
        };
        private static readonly GridTextInfo[] _viewGridGroup1TextInfos = new[]
        {
            new GridTextInfo(0.0f, 1.0f, "1.0"),
            new GridTextInfo(1.0f, 0.0f, "1.0")
        };
        // ViewGridGroup2: 0.5
        private static readonly Vector3[] _viewGridGroup2LineSegments = new[]
        {
            new Vector3(0.0f, 0.5f), new Vector3(1.0f, 0.5f),
            new Vector3(0.5f, 1.0f), new Vector3(0.5f, 0.0f)
        };
        private static readonly GridTextInfo[] _viewGridGroup2TextInfos = new[]
        {
            new GridTextInfo(0.0f, 0.5f, "0.5"),
            new GridTextInfo(0.5f, 0.0f, "0.5")
        };
        // ViewGridGroup3: 0.1, 0.2, 0.3, 0.4, 0.6, 0.7, 0.8, 0.9
        private static readonly Vector3[] _viewGridGroup3LineSegments = new[]
        {
            new Vector3(0.0f, 0.9f), new Vector3(1.0f, 0.9f),
            new Vector3(0.0f, 0.8f), new Vector3(1.0f, 0.8f),
            new Vector3(0.0f, 0.7f), new Vector3(1.0f, 0.7f),
            new Vector3(0.0f, 0.6f), new Vector3(1.0f, 0.6f),
            new Vector3(0.0f, 0.4f), new Vector3(1.0f, 0.4f),
            new Vector3(0.0f, 0.3f), new Vector3(1.0f, 0.3f),
            new Vector3(0.0f, 0.2f), new Vector3(1.0f, 0.2f),
            new Vector3(0.0f, 0.1f), new Vector3(1.0f, 0.1f),
            new Vector3(0.1f, 1.0f), new Vector3(0.1f, 0.0f),
            new Vector3(0.2f, 1.0f), new Vector3(0.2f, 0.0f),
            new Vector3(0.3f, 1.0f), new Vector3(0.3f, 0.0f),
            new Vector3(0.4f, 1.0f), new Vector3(0.4f, 0.0f),
            new Vector3(0.6f, 1.0f), new Vector3(0.6f, 0.0f),
            new Vector3(0.7f, 1.0f), new Vector3(0.7f, 0.0f),
            new Vector3(0.8f, 1.0f), new Vector3(0.8f, 0.0f),
            new Vector3(0.9f, 1.0f), new Vector3(0.9f, 0.0f)
        };
        private static readonly GridTextInfo[] _viewGridGroup3TextInfos = new[]
        {
            new GridTextInfo(0.0f, 0.1f, "0.1"),
            new GridTextInfo(0.0f, 0.2f, "0.2"),
            new GridTextInfo(0.0f, 0.3f, "0.3"),
            new GridTextInfo(0.0f, 0.4f, "0.4"),
            new GridTextInfo(0.0f, 0.6f, "0.6"),
            new GridTextInfo(0.0f, 0.7f, "0.7"),
            new GridTextInfo(0.0f, 0.8f, "0.8"),
            new GridTextInfo(0.0f, 0.9f, "0.9"),
            new GridTextInfo(0.1f, 0.0f, "0.1"),
            new GridTextInfo(0.2f, 0.0f, "0.2"),
            new GridTextInfo(0.3f, 0.0f, "0.3"),
            new GridTextInfo(0.4f, 0.0f, "0.4"),
            new GridTextInfo(0.6f, 0.0f, "0.6"),
            new GridTextInfo(0.7f, 0.0f, "0.7"),
            new GridTextInfo(0.8f, 0.0f, "0.8"),
            new GridTextInfo(0.9f, 0.0f, "0.9")
        };

        [MenuItem("Window/UV Viewer", false, 2050)]
        public static UVViewerWindow ShowWindow()
        {
            return (UVViewerWindow)EditorWindow.GetWindow(typeof(UVViewerWindow));
        }

        private static void Swap(ref int a, ref int b)
        {
            int t = a;
            a = b;
            b = t;
        }

        private static void BuildSegmentIndices(
            Mesh mesh,
            out int[][] subMeshSegmentIndices,
            out HashSet<int>[] subMeshVertexIndices,
            out SubMeshInfo[] subMeshInfos)
        {
            int subMeshCount = mesh.subMeshCount;

            subMeshSegmentIndices = new int[subMeshCount][];
            subMeshVertexIndices = new HashSet<int>[subMeshCount];
            subMeshInfos = new SubMeshInfo[subMeshCount];

            var indexPairs = new HashSet<int>[mesh.vertexCount];

            for (int i = 0; i < subMeshCount; ++i)
            {
                MeshTopology topology = mesh.GetTopology(i);
                int[] indices = mesh.GetIndices(i);
                subMeshInfos[i] = new SubMeshInfo(topology, (uint)indices.Length);

                if (topology != MeshTopology.Triangles)
                {
                    subMeshSegmentIndices[i] = new int[0];
                    subMeshVertexIndices[i] = new HashSet<int>();
                    continue;
                }

                for (int ti = 0; ti < indices.Length; ti += 3)
                {
                    int i0 = indices[ti];
                    int i1 = indices[ti + 1];
                    int i2 = indices[ti + 2];

                    if (i1 < i0)
                    {
                        Swap(ref i0, ref i1);
                    }

                    if (i2 < i1)
                    {
                        Swap(ref i1, ref i2);

                        if (i1 < i0)
                        {
                            Swap(ref i0, ref i1);
                        }
                    }

                    var pair0 = indexPairs[i0] ?? (indexPairs[i0] = new HashSet<int>());
                    var pair1 = indexPairs[i1] ?? (indexPairs[i1] = new HashSet<int>());
                    var pair2 = indexPairs[i2] ?? (indexPairs[i2] = new HashSet<int>());

                    pair0.Add(i1);
                    pair1.Add(i2);
                    pair2.Add(i0);
                }

                var segmentIndices = new List<int>();
                var vertexIndices = new HashSet<int>();

                for (int startIndex = 0; startIndex < indexPairs.Length; ++startIndex)
                {
                    HashSet<int> pairs = indexPairs[startIndex];

                    if (pairs == null || pairs.Count == 0)
                    {
                        continue;
                    }

                    vertexIndices.Add(startIndex);

                    foreach (var endIndex in pairs)
                    {
                        segmentIndices.Add(startIndex);
                        segmentIndices.Add(endIndex);
                        vertexIndices.Add(endIndex);
                    }
                }

                Array.Clear(indexPairs, 0, indexPairs.Length);

                subMeshSegmentIndices[i] = segmentIndices.ToArray();
                subMeshVertexIndices[i] = vertexIndices;
            }
        }

        private static void SetViewMeshForGeometryShader(
            Vector2[] uv,
            Color32[] colors,
            int subMeshCount,
            int[][] subMeshSegmentIndices,
            HashSet<int>[] subMeshVertexIndices,
            Mesh lineMesh,
            Mesh vertexMesh)
        {
            if (uv.Length == 0)
            {
                return;
            }

            var uvVertices = uv.Select(point => (Vector3)point).ToArray();

            // 라인 메시.
            lineMesh.subMeshCount = subMeshCount;
            lineMesh.vertices = uvVertices;

            for (int i = 0; i < subMeshCount; ++i)
            {
                int[] segmentIndices = subMeshSegmentIndices[i];
                lineMesh.SetIndices(segmentIndices, MeshTopology.Lines, i, false);
            }

            // 버텍스 메시.
            vertexMesh.subMeshCount = subMeshCount;
            vertexMesh.vertices = uvVertices;
            vertexMesh.colors32 = colors;

            for (int i = 0; i < subMeshCount; ++i)
            {
                HashSet<int> vertexIndices = subMeshVertexIndices[i];
                vertexMesh.SetIndices(vertexIndices.ToArray(), MeshTopology.Points, i, false);
            }
        }

        private static void SetViewMesh(
            Vector2[] uv,
            Color32[] colors,
            int subMeshCount,
            int[][] subMeshSegmentIndices,
            HashSet<int>[] subMeshVertexIndices,
            Mesh lineMesh,
            Mesh vertexMesh)
        {
            if (uv.Length == 0)
            {
                return;
            }

            var uvVertices = uv.Select(point => (Vector3)point).ToArray();

            // 라인 메시.
            lineMesh.subMeshCount = subMeshCount;
            lineMesh.vertices = uvVertices;

            for (int i = 0; i < subMeshCount; ++i)
            {
                int[] segmentIndices = subMeshSegmentIndices[i];
                lineMesh.SetIndices(segmentIndices, MeshTopology.Lines, i, false);
            }

            // 버텍스 메시.
            vertexMesh.subMeshCount = subMeshCount;

            // 버텍스.
            var vertexVertices = new Vector3[uvVertices.Length * 4];

            for (int i = 0; i < uvVertices.Length; ++i)
            {
                int index = i * 4;

                vertexVertices[index + 0] = uvVertices[i];
                vertexVertices[index + 1] = uvVertices[i];
                vertexVertices[index + 2] = uvVertices[i];
                vertexVertices[index + 3] = uvVertices[i];
            }

            vertexMesh.vertices = vertexVertices;

            // 노멀.
            var baseNormals = new[]
            {
                new Vector3(-1.0f, 1.0f).normalized,
                new Vector3(1.0f, 1.0f).normalized,
                new Vector3(-1.0f, -1.0f).normalized,
                new Vector3(1.0f, -1.0f).normalized
            };
            var vertexNormals = new Vector3[uvVertices.Length * 4];

            for (int i = 0; i < vertexNormals.Length; i += 4)
            {
                vertexNormals[i + 0] = baseNormals[0];
                vertexNormals[i + 1] = baseNormals[1];
                vertexNormals[i + 2] = baseNormals[2];
                vertexNormals[i + 3] = baseNormals[3];
            }

            vertexMesh.normals = vertexNormals;

            // 컬러.
            vertexMesh.colors32 = colors;

            // 인덱스.
            var baseIndices = new[] { 0, 1, 2, 2, 1, 3 };

            for (int i = 0; i < subMeshCount; ++i)
            {
                HashSet<int> vertexIndices = subMeshVertexIndices[i];
                var triangles = new int[vertexIndices.Count * 6];
                int index = 0;

                foreach (var vertexIndex in vertexIndices)
                {
                    int offset = vertexIndex * 4;

                    triangles[index + 0] = baseIndices[0] + offset;
                    triangles[index + 1] = baseIndices[1] + offset;
                    triangles[index + 2] = baseIndices[2] + offset;

                    triangles[index + 3] = baseIndices[3] + offset;
                    triangles[index + 4] = baseIndices[4] + offset;
                    triangles[index + 5] = baseIndices[5] + offset;

                    index += 6;
                }

                vertexMesh.SetTriangles(triangles, i, false);
            }
        }
    }
}
