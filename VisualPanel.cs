using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ThreeNeighbors.Annotations;

namespace ThreeNeighbors
{
    public class VisualPanel: FrameworkElement,INotifyPropertyChanged
    {
        private Random _r = new Random();

        private VisualCollection _children;

        private List<MyPoint> _points = new List<MyPoint>();
        
        private List<MyLine> _lines = new List<MyLine>();
        
        private DrawingVisual _pointsDrawingVisual = new DrawingVisual();

        private DrawingVisual _linesDrawingVisual = new DrawingVisual();

        private int _pointsCount = 20;
        public int PointsCount
        {
            get => _pointsCount;
            set
            {
                if (_pointsCount == value)
                    return;
                _pointsCount = value;
                GeneratePoints(_pointsCount);
                DrawPoints();
                GenerateLines();
                DrawLines();
            }
        }

        public VisualPanel()
        {
            
            _children = new VisualCollection(this);
            

            this.Loaded += (o, e) =>
            {
                _children.Clear();

                _children.Add(_linesDrawingVisual);
                _children.Add(_pointsDrawingVisual);
                GeneratePoints(20);
                DrawPoints();
                GenerateLines();
                DrawLines();
            };
        }

        private void GeneratePoints(int pointsCount)
        {
            _points.Clear();
            

            for (int i = 0; i < pointsCount; i++)
            {
   
                _points.Add(new MyPoint(new Vector(_r.NextDouble()* ActualWidth, _r.NextDouble()*ActualHeight)));
            }

        }

        private void DrawPoints()
        {
            using var context = _pointsDrawingVisual.RenderOpen();
            foreach (var point in _points)
            {
                point.Draw(context);
            }
        }

        

        private void GenerateLines()
        {
            _lines.Clear();

            for (int i = 0; i < _points.Count; i++)
            {
                if(_points[i].Quantity == 3)
                    continue;
                
                for (int k = 0; k < 3; k++)
                {

                
                    int minIndex = -1;
                    double minValue = double.MaxValue;
                    for (int j = 0; j < _points.Count; j++)
                    {
                        if(i == j) continue;
                        if (_points[i].Lines[0]?.B == _points[j] ||
                            _points[i].Lines[1]?.B == _points[j] ||
                            _points[i].Lines[2]?.B == _points[j])
                            continue;
                        
                        double length = (_points[i].Coord - _points[j].Coord).Length;
                        if (minValue > length)
                        {
                            minIndex = j;
                            minValue = length;
                        }
                    }

                    if (minIndex >= 0)
                    {
                        var line = _points[i].CreateLine(_points[minIndex]);
                        _lines.Add(line);
                        if (_points[i].Quantity == 3)
                            break;
                    }
                }
            }
        }


        private void DrawLines()
        {

            using var context = _linesDrawingVisual.RenderOpen();
            foreach (var line in _lines)
            {
                line.Draw(context);
            }
        }



        private struct IndexedLine
        {
            public int A,B;
            
            public IndexedLine(int a, int b)
            {
                A = a;
                B = b;
            }

            public override string ToString()
            {
                return $"{{{A}, {B}}}";
            }
        }

        private class MyLine
        {
            public MyPoint A, B;
            private Pen _linePen = new Pen(Brushes.WhiteSmoke, 1);

            public MyLine(MyPoint a, MyPoint b)
            {
                A = a;
                B = b;
            }

            public void Draw(DrawingContext context)
            {
                context.DrawLine(_linePen, A.Coord.ToPoint(), ((A.Coord+B.Coord)/2).ToPoint());
               
            }
        }

        private class MyPoint
        {
            public Vector Coord;

            public MyLine[] Lines = new MyLine[3];
            public int Quantity { get; private set; } = 0;

            public MyPoint(Vector coord)
            {
                Coord = coord;
            }

            public MyLine CreateLine(MyPoint otherPoint)
            {
                if (otherPoint == null)
                    throw new ArgumentNullException(nameof(otherPoint));
                if(Quantity == 3)
                    throw new Exception("Point is full!");
               

                MyLine line = new MyLine(this, otherPoint);
                Lines[Quantity] = line;
                Quantity++;

                return line;
            }



            public void Draw(DrawingContext context)
            {
                context.DrawEllipse(Brushes.CornflowerBlue, null, Coord.ToPoint(), 2, 2);
            }
            
        }




        protected override int VisualChildrenCount => _children.Count;

        // Provide a required override for the GetVisualChild method.
        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _children.Count)
                throw new ArgumentOutOfRangeException();

            return _children[index];
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
