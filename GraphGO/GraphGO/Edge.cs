using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Controls;
using System.Windows.Shapes;

namespace GraphDemo
{
    public class Edge
    {
        public Vertex From { get; set; }
        public Vertex To { get; set; }
        public int Weight { get; set; }
        public Line Line { get; set; }
        public TextBlock WeightLabel { get; set; }
    }
}
