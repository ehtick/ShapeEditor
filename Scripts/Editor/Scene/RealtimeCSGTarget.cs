﻿#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class RealtimeCSGTarget : MonoBehaviour, IShapeEditorTarget
    {
        /// <summary>The shape editor project that last assigned polygons to this target.</summary>
        [SerializeField]
        internal Project project;

        /// <summary>The convex polygons set by the shape editor.</summary>
        private List<Polygon> convexPolygons2D;

        /// <summary>The operating mode.</summary>
        [SerializeField]
        internal RealtimeCSGTargetMode targetMode = RealtimeCSGTargetMode.FixedExtrude;

        public void OnShapeEditorUpdateProject(Project project)
        {
            convexPolygons2D = null;

            this.project = project.Clone();

            Rebuild();
        }

        public void Rebuild()
        {
            // get the convex project polygons.
            if (convexPolygons2D == null)
            {
                // ensure the project data is ready.
                project.Validate();

                convexPolygons2D = project.GenerateConvexPolygons();
            }

            switch (targetMode)
            {
                case RealtimeCSGTargetMode.FixedExtrude:
                    FixedExtrude_Rebuild();
                    break;
            }
        }

        private Transform CleanAndGetBrushParent()
        {
            var parent = transform.Find("Brushes");
            if (parent) DestroyImmediate(parent.gameObject);

            parent = new GameObject("Brushes").transform;
            parent.SetParent(transform, false);
            return parent;
        }
    }
}

#endif