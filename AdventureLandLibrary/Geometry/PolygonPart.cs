using System;
using System.Collections.Generic;
using System.Text;
using TriangleNet.Geometry;


namespace AdventureLandLibrary.Geometry
{
    public class PolygonPart
    {
        public Point[] Vertices;
        public Contour contour;
        public bool IsHole;


        public PolygonPart(Point[] vertices)
        {
            this.Vertices = vertices;

            List<Vertex> vertexList = new List<Vertex>();
            for(var i = 0; i < vertices.Length; i++)
            {
                var vertex = vertices[i];
                vertexList.Add(new Vertex(vertex.X, vertex.Y));
            }

            contour = new Contour(vertexList);
        }

        public void Refresh()
        {
            List<Vertex> vertexList = new List<Vertex>();
            for (var i = 0; i < Vertices.Length; i++)
            {
                var vertex = Vertices[i];
                vertexList.Add(new Vertex(vertex.X, vertex.Y));
            }

            contour = new Contour(vertexList);
        }
    }
}
