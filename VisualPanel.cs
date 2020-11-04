using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
                
        private DrawingVisual _pointsDrawingVisual = new DrawingVisual();

        private DrawingVisual _linesDrawingVisual = new DrawingVisual();

        private int _pointsCount = 120;
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
                CompositionTarget.Rendering += OnRendering;
            };
            this.Unloaded += (o, e) =>
            {
                CompositionTarget.Rendering -= OnRendering;
            };
        }

        private Vector[] _newPoints;
        private void OnRendering(object sender, EventArgs e)
        {
            if (_newPoints == null || _newPoints.Length != _pointsCount) 
                _newPoints = new Vector[_pointsCount];
            for (var i = 0; i < _points.Count; i++)
            {
               var myPoint = _points[i];
                /* var a = myPoint.Lines[0].B.Coord;
                var b = myPoint.Lines[1].B.Coord;
                var c = myPoint.Lines[2].B.Coord;
                var d = 2 * (a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y));

                var al = a.X * a.X + a.Y * a.Y;
                var bl = b.X * b.X + b.Y * b.Y;
                var cl = c.X * c.X + c.Y * c.Y;

                var ux = (al * (b.Y - c.Y) + bl * (c.Y - a.Y) + cl * (a.Y - b.Y)) / d;
                var uy = (al * (c.X - b.X) + bl * (a.X - c.X) + cl * (b.X - a.X)) / d;

                var u = new Vector(ux, uy);*/

                _newPoints[i] = myPoint.Coord;
                for (int j = 0; j < 3; j++)
                {  
                    var du = (myPoint.Lines[j].B.Coord - myPoint.Coord);
                    var dl = du.Length;
                    _newPoints[i] += du * (dl - 10) / 1000 / (j + 1);
                }
                
                myPoint.Clear();
            }

            for (var i = 0; i < _points.Count; i++)
            {
                _points[i].Coord = _newPoints[i];
            }
            
            DrawPoints();
            GenerateLines();
            DrawLines();
            
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
                        _points[i].CreateLine(_points[minIndex]);
                        
                        if (_points[i].Quantity == 3)
                            break;
                    }
                }
            }
        }


        private void DrawLines()
        {

            using var context = _linesDrawingVisual.RenderOpen();
            foreach (var point in _points)
            {
                point.Lines[0]?.Draw(context);
                point.Lines[1]?.Draw(context);
                point.Lines[2]?.Draw(context);
            }
        }
        
        private class MyLine
        {
            public MyPoint A, B;
            private static Pen _linePen = new Pen(Brushes.WhiteSmoke, 1);

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
                for (int i = 0; i < 3; i++)
                {
                    Lines[i] = new MyLine(this, null);
                }
            }

            public void CreateLine(MyPoint otherPoint)
            {
                if (otherPoint == null)
                    throw new ArgumentNullException(nameof(otherPoint));
                if(Quantity == 3)
                    throw new Exception("Point is full!");
               
                Lines[Quantity].B = otherPoint;
                Quantity++;
            }

            public void Clear()
            {
                Quantity = 0;
                Lines[0].B = Lines[1].B = Lines[2].B = null;
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
