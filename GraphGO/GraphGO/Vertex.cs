
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows;
namespace GraphDemo
{
    public class Vertex
    {
        public int Id { get; set; }
        public Point Position { get; set; }
        public Ellipse Ellipse { get; set; }
        public TextBlock Label { get; set; }
    }
}
