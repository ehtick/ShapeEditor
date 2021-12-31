﻿#if UNITY_EDITOR

// contains source code from https://github.com/Genbox/VelcroPhysics (see Licenses/VelcroPhysics.txt).
// contains source code from https://github.com/sabresaurus/SabreCSG (see Licenses/SabreCSG.txt).

using System.Collections.Generic;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>A collection of vertex points that make up a 2D/3D polygon.</summary>
    public partial class Polygon : List<Vertex>
    {
        /// <summary>
        /// [3D] After calling <see cref="RecalculatePlane"/> a plane that approximately resembles the polygon.
        /// </summary>
        public Plane plane;

        /// <summary>[3D] Calculates a plane that approximately resembles the polygon.</summary>
        public Plane RecalculatePlane()
        {
            int count = Count;
            Debug.Assert(count >= 3, "Attempted to calculate plane of a 3D polygon with less than 3 vertices.");

            plane = new Plane(this[0].position, this[1].position, this[2].position);

            // hack: if the plane's normal is zero and there's more than 3 vertices,
            // try using alternative vertices to construct the plane.
            if (plane.normal == Vector3.zero && count > 3)
            {
                // iterate through the available vertices.
                for (int i = 1; i < count; i++)
                {
                    if (i + 1 >= count || i + 2 >= count) break;

                    // use the next 3 vertices construct a new plane.
                    plane = new Plane(this[i].position, this[i + 1].position, this[i + 2].position);
                    // stop once we found a valid normal.
                    if (plane.normal != Vector3.zero)
                        break;
                }
            }

            Debug.Assert(plane.normal != Vector3.zero, "Attempted to calculate the plane of a 3D polygon but got a zero normal.");
            return plane;
        }

        /// <summary>[3D] Gets a flipped copy of the polygon by reversing the winding order.</summary>
        public Polygon flipped
        {
            get
            {
                var polygon = new Polygon(this);
                polygon.Reverse();
                return polygon;
            }
        }

        /// <summary>[3D] Rotates this polygon by the specified amount.</summary>
        /// <param name="rotation">The rotation to rotate all vertices by.</param>
        public void Rotate(Quaternion rotation)
        {
            int count = Count;
            for (int i = 0; i < count; i++)
                this[i] = new Vertex(rotation * this[i].position, this[i].uv0);
        }

        /// <summary>[3D] Generates UV0 coordinates using the AutoUV algorithm of SabreCSG.</summary>
        /// <param name="offset">The offset to be added to the UV coordinates.</param>
        public void ApplySabreCSGAutoUV0(Vector2 offset)
        {
            int count = Count;
            RecalculatePlane();

            var cancellingRotation = Quaternion.Inverse(Quaternion.LookRotation(plane.normal));
            // sets the uv at each point to the position on the plane.
            for (int i = 0; i < count; i++)
            {
                Vector2 uv = (cancellingRotation * this[i].position);
                this[i] = new Vertex(this[i].position, offset + uv);
            }
        }

        /// <summary>
        /// Extrudes this polygon by the specified distance along its normal and returns the
        /// extruded polygons.
        /// </summary>
        /// <param name="distance">The distance to extrude by.</param>
        /// <returns>The extruded polygons.</returns>
        public List<Polygon> Extrude(float distance)
        {
            int count = Count;
            var results = new List<Polygon>(count);

            // attempted to extrude a 3D polygon with less than 3 vertices.
            if (count < 3)
                return results;

            RecalculatePlane();
            var normal = plane.normal;

            for (int i = 0; i < count - 1; i++)
            {
                results.Add(new Polygon(new Vertex[] {
                    this[i],
                    new Vertex(this[i].position + normal * distance, this[i].uv0),
                    new Vertex(this[i + 1].position + normal * distance, this[i + 1].uv0),
                    this[i + 1],
                }));
            }

            // one more face that wraps around to index 0.
            results.Add(new Polygon(new Vertex[] {
                this[count - 1],
                new Vertex(this[count - 1].position + normal * distance, this[count - 1].uv0),
                new Vertex(this[0].position + normal * distance, this[0].uv0),
                this[0],
            }));

            return results;
        }
    }
}

#endif