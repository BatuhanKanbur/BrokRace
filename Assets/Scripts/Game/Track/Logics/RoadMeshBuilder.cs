using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static Game.Track.Constants.TrackConstants;

namespace Game.Track.Logics
{
    public static class RoadMeshBuilder
    {
        public static Mesh Build(IReadOnlyList<Vector3> centre, float width)
        {
            var half = width * 0.5f;
            var vertices = new List<Vector3>(centre.Count * VertsPerSection);
            var uvs = new List<Vector2>(centre.Count * VertsPerSection);
            var triangles = new List<int>(centre.Count * IndicesPerSection);
            var travelled = 0f;

            for (var index = 0; index < centre.Count; index++)
            {
                var forward = Forward(centre, index);
                var right = Vector3.Cross(Vector3.up, forward);
                var lift = Vector3.up * RoadSurfaceHeight;
                var drop = Vector3.up * (RoadSurfaceHeight - ShoulderDrop);
                if (index > 0) travelled += Vector3.Distance(centre[index - 1], centre[index]);

                vertices.Add(centre[index] - right * (half + ShoulderWidth) + drop);
                vertices.Add(centre[index] - right * half + lift);
                vertices.Add(centre[index] + right * half + lift);
                vertices.Add(centre[index] + right * (half + ShoulderWidth) + drop);

                var v = travelled / width;
                var shoulderU = ShoulderWidth / width;
                uvs.Add(new Vector2(-shoulderU, v));
                uvs.Add(new Vector2(0f, v));
                uvs.Add(new Vector2(1f, v));
                uvs.Add(new Vector2(1f + shoulderU, v));
            }

            for (var index = 0; index < centre.Count - 1; index++)
            {
                var row = index * VertsPerSection;
                for (var strip = 0; strip < StripsPerSection; strip++)
                {
                    var a = row + strip;
                    var b = a + 1;
                    var c = a + VertsPerSection;
                    var d = b + VertsPerSection;
                    triangles.Add(a);
                    triangles.Add(c);
                    triangles.Add(b);
                    triangles.Add(b);
                    triangles.Add(c);
                    triangles.Add(d);
                }
            }

            var mesh = new Mesh { name = RoadMeshName, indexFormat = IndexFormat.UInt32 };
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Vector3 Forward(IReadOnlyList<Vector3> centre, int index)
        {
            var from = centre[Mathf.Max(index - 1, 0)];
            var to = centre[Mathf.Min(index + 1, centre.Count - 1)];
            return (to - from).normalized;
        }
    }
}
