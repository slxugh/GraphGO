using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphDemo
{
    public class VertexData
    {
        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class EdgeData
    {
        public int From { get; set; }
        public int To { get; set; }
        public int Weight { get; set; }
    }

    public class GraphData
    {
        public List<VertexData> Vertices { get; set; } = new List<VertexData>();
        public List<EdgeData> Edges { get; set; } = new List<EdgeData>();
    }
}
