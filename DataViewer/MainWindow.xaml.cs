using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using AdventureLandLibrary.Global;
using AdventureLandLibrary.GameObjects;

namespace DataViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Dictionary<string, Map> mapCache = new Dictionary<string, Map>();
        Map currentMap;
        List<string> maps;
        bool isMouseDown;
        double xOffset;
        double yOffset;
        ImageBrush background;

        List<Shape> currentPath = new List<Shape>();
        List<Line> currentPathSmoothed = new List<Line>();
        List<Line> currentPathDetailed = new List<Line>();

        AdventureLandLibrary.Geometry.Point PathEndPoint;

        Point initial;
        public MainWindow()
        {
            InitializeComponent();

            maps = ((JObject)Loader.data.geometry).Properties().Select(p => p.Name).ToList();

            MapSelect.ItemsSource = maps;

            MapSelect.SelectedIndex = 11;//19;
        }

        private void MapSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PathEndPoint = null;

            foreach(var line in currentPath)
            {
                Canvas.Children.Remove(line);
            }

            var mapname = (string)MapSelect.SelectedValue;
            
            if(mapCache.ContainsKey(mapname))
            {
                currentMap = mapCache[mapname];
            }
            else
            {
                currentMap = new Map(mapname);
                mapCache.Add(mapname, currentMap);
            }

            Canvas.Width = this.ActualWidth;
            Canvas.Height = this.ActualHeight;

            var bitmap = currentMap.GetBitmap();

            var bitmapImage = Convert(bitmap);
            var imagebrush = new ImageBrush();
            imagebrush.ImageSource = bitmapImage;
            imagebrush.Stretch = Stretch.None;
            imagebrush.AlignmentX = AlignmentX.Left;
            imagebrush.AlignmentY = AlignmentY.Top;
            imagebrush.ViewboxUnits = BrushMappingMode.Absolute;
            imagebrush.Viewbox = new Rect(new Point(xOffset, yOffset), new Point(xOffset + this.ActualWidth, yOffset + this.ActualHeight));

            background = imagebrush;

            Canvas.Background = imagebrush;

            //image.Source = bitmapImage;

            //Canvas.RenderTransform = new TranslateTransform(xOffset, yOffset);
        }

        public void Clip(System.Windows.Controls.Image image, Rect visibleRect)
        {
            image.RenderTransform = new TranslateTransform(-visibleRect.X, -visibleRect.Y);
            image.Clip = new RectangleGeometry
            {
                Rect = new Rect(
                    0,
                    0,
                    visibleRect.X + visibleRect.Width,
                    visibleRect.Y + visibleRect.Height)
            };
        }

        public void Clip(System.Windows.Controls.Canvas image, Rect visibleRect)
        {
            image.RenderTransform = new TranslateTransform(-visibleRect.X, -visibleRect.Y);
            image.Clip = new RectangleGeometry
            {
                Rect = new Rect(
                    0,
                    0,
                    visibleRect.X + visibleRect.Width,
                    visibleRect.Y + visibleRect.Height)
            };
        }

        /// <summary>
        /// Takes a bitmap and converts it to an image that can be handled by WPF ImageBrush
        /// </summary>
        /// <param name="src">A bitmap image</param>
        /// <returns>The image as a BitmapImage for WPF</returns>
        public BitmapImage Convert(System.Drawing.Bitmap src)
            {
                MemoryStream ms = new MemoryStream();
                ((System.Drawing.Bitmap)src).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                ms.Seek(0, SeekOrigin.Begin);
                image.StreamSource = ms;
                image.EndInit();
                return image;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = true;

            if (initial == null)
            {
                initial = Mouse.GetPosition(this);
            }
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = false;
        }

        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            var position = Mouse.GetPosition(this);
            var mouseDown = System.Windows.Input.Mouse.LeftButton == MouseButtonState.Pressed;
            if (mouseDown)
            {
                var xChange = (e.GetPosition(this).X - initial.X);
                var yChange = (e.GetPosition(this).Y - initial.Y);
                xOffset = xOffset - (xChange);
                yOffset = yOffset - (yChange);

                if (xOffset < 0)
                {
                    xOffset = 0;
                }

                if (yOffset < 0)
                {
                    yOffset = 0;
                }

                initial = position;

                //var maxClipX = xOffset + this.ActualWidth - image.Margin.Left - image.Margin.Right;
                //var maxClipY = yOffset + this.ActualHeight - image.Margin.Top - image.Margin.Bottom;

                //if(maxClipX >= currentMap.Width + 17)// - this.ActualWidth - image.Margin.Left - image.Margin.Right)
                //{
                //    xOffset = currentMap.Width - this.ActualWidth + image.Margin.Left + image.Margin.Right + 17;
                //    maxClipX = xOffset + this.ActualWidth - image.Margin.Left - image.Margin.Right + 17;
                //}

                //if(maxClipY >= currentMap.Height + 77)// - this.ActualHeight - image.Margin.Top - image.Margin.Bottom)
                //{
                //    yOffset = currentMap.Height - this.ActualHeight + image.Margin.Top + image.Margin.Bottom + 77;
                //    maxClipY = yOffset + this.ActualHeight - image.Margin.Top - image.Margin.Bottom + 77;
                //}

                //if (yOffset > viewer.ScrollableHeight)
                //{
                //    yOffset = viewer.ScrollableHeight;
                //}

                //if (xOffset > viewer.ScrollableWidth)
                //{
                //    xOffset = viewer.ScrollableWidth;
                //}


                //viewer.ScrollToHorizontalOffset(xOffset);
                //viewer.ScrollToVerticalOffset(yOffset);
                //initial = e.GetPosition(this);
                //Clip(image, new Rect(new Point(xOffset, yOffset), new Point(maxClipX, maxClipY)));
                //foreach(var line in currentPath)
                //{
                //    line.X1 += xChange;
                //    line.X2 += xChange;
                //    line.Y1 += yChange;
                //    line.Y2 += yChange;
                //}
                background.Viewbox = new Rect(new Point(xOffset, yOffset), new Point(xOffset + this.ActualWidth, yOffset + this.ActualHeight));
            }
            else
            {
                initial = position;
            }

            if (PathEndPoint != null)
            {
                foreach (var line in currentPath)
                {
                    Canvas.Children.Remove(line);
                }

                foreach (var line in currentPathSmoothed)
                {
                    Canvas.Children.Remove(line);
                }

                foreach (var line in currentPathDetailed)
                {
                    Canvas.Children.Remove(line);
                }

                currentPath = new List<Shape>();

                var pathStartPoint = GetMouseMapPoint();

                //var path = currentMap.FindPath(pathStartPoint, PathEndPoint);
                var nodes = currentMap.FindPathDebug(pathStartPoint, PathEndPoint);

                //var start = new System.Windows.Shapes.Path();

                //start.StrokeThickness = 2;
                //start.Stroke = Brushes.Green;

                //var startEll = new EllipseGeometry();

                //startEll.Center = new Point(nodes[0].center.X - xOffset, nodes[0].center.Y - yOffset);
                //startEll.RadiusX = 5;
                //startEll.RadiusY = 5;

                //start.Data = startEll;

                //currentPath.Add(start);
                //Canvas.Children.Add(start);

                //var end = new System.Windows.Shapes.Path();

                //end.StrokeThickness = 2;
                //end.Stroke = Brushes.Purple;

                //var endEll = new EllipseGeometry();

                //endEll.Center = new Point(nodes.Last().center.X - xOffset, nodes.Last().center.Y - yOffset);
                //endEll.RadiusX = 5;
                //endEll.RadiusY = 5;

                //end.Data = endEll;

                //currentPath.Add(end);
                //Canvas.Children.Add(end);

                List<AdventureLandLibrary.Geometry.LineD> portals = new List<AdventureLandLibrary.Geometry.LineD>();

                //for (var i = 0; i < nodes.Length - 1; i++)
                //{
                //    portals.Add(nodes[i].GetPortal(nodes[i + 1]));
                //}
                //if (nodes.Length > 2)
                //{
                //    var ending = nodes[nodes.Length - 1].GetNonPortal(nodes[nodes.Length - 2]);

                //    portals.AddRange(ending);
                //}

                //for (var i = 0; i < portals.Count; i++)
                //{
                //    var portal = portals[i];

                //    var p1Link = portals.Where(p => (p.P1.X == portal.P1.X && p.P1.Y == portal.P1.Y) || (p.P2.X == portal.P1.X && p.P2.Y == portal.P1.Y)).ToList();
                //    var p2Link = portals.Where(p => (p.P1.X == portal.P2.X && p.P1.Y == portal.P2.Y) || (p.P2.X == portal.P2.X && p.P2.Y == portal.P2.Y)).ToList();

                //    if (true)
                //    {
                //        var line = new Line();
                //        line.X1 = portal.P1.X - xOffset;
                //        line.X2 = portal.P2.X - xOffset;
                //        line.Y1 = portal.P1.Y - yOffset;
                //        line.Y2 = portal.P2.Y - yOffset;


                //        line.StrokeThickness = 2;
                //        line.Stroke = Brushes.Green;

                //        line.MouseRightButtonDown += Window_MouseRightButtonDown;

                //        currentPath.Add(line);
                //        Canvas.Children.Add(line);

                //        var right = new System.Windows.Shapes.Path();

                //        right.StrokeThickness = 2;
                //        right.Stroke = Brushes.Purple;

                //        var rightEll = new EllipseGeometry();

                //        rightEll.Center = new Point(portal.P1.X - xOffset, portal.P1.Y - yOffset);
                //        rightEll.RadiusX = 5;
                //        rightEll.RadiusY = 5;

                //        right.Data = rightEll;

                //        currentPath.Add(right);
                //        Canvas.Children.Add(right);

                //        var left = new System.Windows.Shapes.Path();

                //        left.StrokeThickness = 2;
                //        left.Stroke = Brushes.Cyan;

                //        var leftEll = new EllipseGeometry();

                //        leftEll.Center = new Point(portal.P2.X - xOffset, portal.P2.Y - yOffset);
                //        leftEll.RadiusX = 5;
                //        leftEll.RadiusY = 5;

                //        left.Data = leftEll;

                //        currentPath.Add(left);
                //        Canvas.Children.Add(left);

                //    }
                //}
                timer.Reset();
                timer.Start();
                var funneled = FunnelSmooth(nodes.ToList(), pathStartPoint, PathEndPoint);
                var smoothed = currentMap.SmoothPath(funneled.ToArray());
                timer.Stop();
                PathTime.Content = timer.ElapsedMilliseconds;

                for (var i = 0; i < smoothed.Length - 1; i++)
                {
                    var p1 = smoothed[i];
                    var p2 = smoothed[i + 1];
                    var line2 = new Line();
                    line2.X1 = p1.X - xOffset + currentMap.OffsetX;
                    line2.X2 = p2.X - xOffset + currentMap.OffsetX;
                    line2.Y1 = p1.Y - yOffset + currentMap.OffsetY;
                    line2.Y2 = p2.Y - yOffset + currentMap.OffsetY;


                    line2.StrokeThickness = 1;
                    line2.Stroke = Brushes.Blue;

                    line2.MouseRightButtonDown += Window_MouseRightButtonDown;

                    currentPath.Add(line2);
                    Canvas.Children.Add(line2);
                }



                //for (var i = 0; i < detailedPath.Length - 1; i++)
                //{
                //    var p1 = detailedPath[i];
                //    var p2 = detailedPath[i + 1];

                //    var line = new Line();
                //    line.X1 = p1.X + currentMap.OffsetX - xOffset;
                //    line.X2 = p2.X + currentMap.OffsetX - xOffset;
                //    line.Y1 = p1.Y + currentMap.OffsetY - yOffset;
                //    line.Y2 = p2.Y + currentMap.OffsetY - yOffset;

                //    line.StrokeThickness = 2;
                //    line.Stroke = Brushes.Green;

                //    line.MouseRightButtonDown += Window_MouseRightButtonDown;

                //    currentPathDetailed.Add(line);
                //    Canvas.Children.Add(line);
                //}

                //for (var i = 0; i < smoothedPath.Length - 1; i++)
                //{
                //    var p1 = smoothedPath[i];
                //    var p2 = smoothedPath[i + 1];

                //    var line = new Line();
                //    line.X1 = p1.X + currentMap.OffsetX - xOffset;
                //    line.X2 = p2.X + currentMap.OffsetX - xOffset;
                //    line.Y1 = p1.Y + currentMap.OffsetY - yOffset;
                //    line.Y2 = p2.Y + currentMap.OffsetY - yOffset;

                //    line.StrokeThickness = 2;
                //    line.Stroke = Brushes.Yellow;

                //    line.MouseRightButtonDown += Window_MouseRightButtonDown;

                //    currentPathSmoothed.Add(line);
                //    Canvas.Children.Add(line);
                //}

                //for (var i = 0; i < path.Length - 1; i++)
                //{
                //    var p1 = path[i];
                //    var p2 = path[i + 1];

                //    var line = new Line();
                //    line.X1 = p1.X + currentMap.OffsetX - xOffset;
                //    line.X2 = p2.X + currentMap.OffsetX - xOffset;
                //    line.Y1 = p1.Y + currentMap.OffsetY - yOffset;
                //    line.Y2 = p2.Y + currentMap.OffsetY - yOffset;

                //    line.StrokeThickness = 2;
                //    line.Stroke = Brushes.Blue;

                //    line.MouseRightButtonDown += Window_MouseRightButtonDown;

                //    currentPath.Add(line);
                //    Canvas.Children.Add(line);
                //}

            }
        }

        public void DrawLine(AdventureLandLibrary.Geometry.LineD line, Brush color, int thickness = 1)
        {
            timer.Stop();
            var p1 = line.P1;
            var p2 = line.P2;
            var line2 = new Line();
            line2.X1 = p1.X - xOffset;
            line2.X2 = p2.X - xOffset;
            line2.Y1 = p1.Y - yOffset;
            line2.Y2 = p2.Y - yOffset;

            line2.StrokeThickness = thickness;
            line2.Stroke = color;

            line2.MouseRightButtonDown += Window_MouseRightButtonDown;

            //currentPath.Add(line2);
            //Canvas.Children.Add(line2);
            timer.Start();
        }

        public List<AdventureLandLibrary.Geometry.Point> FunnelSmooth(List<AdventureLandLibrary.Geometry.GraphNode> path, AdventureLandLibrary.Geometry.Point startPoint, AdventureLandLibrary.Geometry.Point endPoint)
        {
            if (path.Count > 2)
            {
                var offsetEndpoint = new AdventureLandLibrary.Geometry.Point(endPoint.X + currentMap.OffsetX, endPoint.Y + currentMap.OffsetY);

                List<AdventureLandLibrary.Geometry.Point> newPath = new List<AdventureLandLibrary.Geometry.Point>();

                List<AdventureLandLibrary.Geometry.LineD> portals = new List<AdventureLandLibrary.Geometry.LineD>();

                for (var i = 0; i < path.Count - 1; i++)
                {
                    portals.Add(path[i].GetPortal(path[i + 1]));
                }

                var ending = path[path.Count - 1].GetNonPortal(path[path.Count - 2], offsetEndpoint);

                portals.AddRange(ending);

                var portalsToDelete = new List<AdventureLandLibrary.Geometry.LineD>();

                for (var i = 0; i < portals.Count; i++)
                {
                    var portal = portals[i];

                    var p1Link = portals.Where(p => (p.P1.X == portal.P1.X && p.P1.Y == portal.P1.Y) || (p.P2.X == portal.P1.X && p.P2.Y == portal.P1.Y)).ToList();
                    var p2Link = portals.Where(p => (p.P1.X == portal.P2.X && p.P1.Y == portal.P2.Y) || (p.P2.X == portal.P2.X && p.P2.Y == portal.P2.Y)).ToList();

                    if ((p1Link.Count == 1 || p2Link.Count == 1) && (i != 0 && i != portals.Count - 1))
                    {
                        //portalsToDelete.Add(portal);
                    }
                }

                foreach (var portal in portalsToDelete)
                {
                    portals.Remove(portal);
                }

                AdventureLandLibrary.Geometry.PointD currentNode = new AdventureLandLibrary.Geometry.PointD(startPoint.X + currentMap.OffsetX, startPoint.Y + currentMap.OffsetY);

                newPath.Add(new AdventureLandLibrary.Geometry.Point((int)currentNode.X - currentMap.OffsetX, (int)currentNode.Y - currentMap.OffsetY));

                int funnelLeftIndex = 1;
                int funnelRightIndex = 1;

                int curFunnelLeftIndex = 1;
                int curFunnelRightIndex = 1;

                AdventureLandLibrary.Geometry.LineD funnelLeft = new AdventureLandLibrary.Geometry.LineD(currentNode, portals[0].P2);
                AdventureLandLibrary.Geometry.LineD funnelRight = new AdventureLandLibrary.Geometry.LineD(currentNode, portals[0].P1);

                DrawLine(funnelLeft, Brushes.Cyan, 2);
                DrawLine(funnelRight, Brushes.Purple, 2);

                int leftSide = 0;
                int rightSide = 1;

                AdventureLandLibrary.Geometry.PointD lastPoint = new AdventureLandLibrary.Geometry.PointD(offsetEndpoint.X, offsetEndpoint.Y);

                int count = 0;
                while (funnelLeftIndex < portals.Count && funnelRightIndex < portals.Count && !(funnelLeft.P2.X == lastPoint.X && funnelLeft.P2.Y == lastPoint.Y) && !(funnelRight.P2.X == lastPoint.X && funnelRight.P2.Y == lastPoint.Y))
                {

                    var prevLeftPoint = portals[funnelLeftIndex].P2;

                    if (leftSide == 0 && funnelLeftIndex > 0)
                    {
                        prevLeftPoint = portals[funnelLeftIndex - 1].P2;
                    }

                    var prevRightPoint = portals[funnelRightIndex].P1;

                    if (rightSide == 0 && funnelRightIndex > 0)
                    {
                        prevRightPoint = portals[funnelRightIndex - 1].P1;
                    }

                    var newFunnelLeftIndex = funnelLeftIndex + leftSide;
                    var newFunnelRightIndex = funnelRightIndex + rightSide;

                    if (newFunnelLeftIndex > portals.Count - 1)
                    {
                        newFunnelLeftIndex = portals.Count - 1;
                    }

                    if (newFunnelRightIndex > portals.Count - 1)
                    {
                        newFunnelRightIndex = portals.Count - 1;
                    }

                    curFunnelLeftIndex = curFunnelLeftIndex + leftSide;
                    curFunnelRightIndex = curFunnelRightIndex + rightSide;

                    if (curFunnelLeftIndex > portals.Count - 1)
                    {
                        curFunnelLeftIndex = portals.Count - 1;
                    }

                    if (curFunnelRightIndex > portals.Count - 1)
                    {
                        curFunnelRightIndex = portals.Count - 1;
                    }

                    var actualLeftPoint = portals[newFunnelLeftIndex].P2;
                    var actualRightPoint = portals[newFunnelRightIndex].P1;

                    var leftPoint = portals[curFunnelLeftIndex].P2;

                    var rightPoint = portals[curFunnelRightIndex].P1;

                    var insideFunnel = false;

                    var dirLeftLeft = funnelLeft.Direction(leftPoint);
                    var dirRightRight = funnelRight.Direction(rightPoint);
                    var leftIsRightOfLeft = funnelLeft.Direction(leftPoint) <= 0;
                    var rightIsLeftOfRight = funnelRight.Direction(rightPoint) >= 0;

                    var dirLeftRight = funnelRight.Direction(leftPoint);
                    var dirRightLeft = funnelLeft.Direction(rightPoint);
                    var leftIsLeftOfRight = funnelRight.Direction(leftPoint) >= 0;
                    var rightIsRightOfLeft = funnelLeft.Direction(rightPoint) <= 0;

                    if (leftIsRightOfLeft && rightIsLeftOfRight && leftIsLeftOfRight && rightIsRightOfLeft)
                    {
                        insideFunnel = true;
                    }

                    if (!insideFunnel)
                    {
                        if (!leftIsLeftOfRight)
                        {
                            currentNode = new AdventureLandLibrary.Geometry.PointD(portals[newFunnelRightIndex].P1.X, portals[newFunnelRightIndex].P1.Y);

                            newPath.Add(new AdventureLandLibrary.Geometry.Point((int)currentNode.X - currentMap.OffsetX, (int)currentNode.Y - currentMap.OffsetY));
                            //funnelLeftIndex = newFunnelLeftIndex;
                            //funnelRightIndex = newFunnelLeftIndex;
                            var minIndex = Math.Min(newFunnelLeftIndex, newFunnelRightIndex);

                            var newFRight = NextRight(minIndex, portals);
                            var newFLeft = NextLeft(minIndex, portals);

                            var maxPortal = Math.Max(newFRight, newFLeft);

                            funnelLeftIndex = maxPortal;
                            funnelRightIndex = maxPortal;
                            funnelLeft = new AdventureLandLibrary.Geometry.LineD(currentNode, portals[maxPortal].P2);
                            funnelRight = new AdventureLandLibrary.Geometry.LineD(currentNode, portals[maxPortal].P1);
                            curFunnelLeftIndex = maxPortal;
                            curFunnelRightIndex = maxPortal;
                            DrawLine(funnelLeft, Brushes.Cyan, 3);
                            DrawLine(funnelRight, Brushes.Purple, 3);
                        }
                        else if (!rightIsRightOfLeft)
                        {
                            currentNode = new AdventureLandLibrary.Geometry.PointD(portals[newFunnelLeftIndex].P2.X, portals[newFunnelLeftIndex].P2.Y);

                            newPath.Add(new AdventureLandLibrary.Geometry.Point((int)currentNode.X - currentMap.OffsetX, (int)currentNode.Y - currentMap.OffsetY));

                            //funnelLeftIndex = newFunnelRightIndex;
                            //funnelRightIndex = newFunnelRightIndex;
                            //curFunnelLeftIndex = funnelLeftIndex;
                            //curFunnelRightIndex = funnelRightIndex;

                            var minIndex = Math.Min(newFunnelLeftIndex, newFunnelRightIndex);

                            var newFRight = NextRight(minIndex, portals);
                            var newFLeft = NextLeft(minIndex, portals);

                            var maxPortal = Math.Max(newFRight, newFLeft);

                            funnelLeftIndex = maxPortal;
                            funnelRightIndex = maxPortal;
                            funnelLeft = new AdventureLandLibrary.Geometry.LineD(currentNode, portals[maxPortal].P2);
                            funnelRight = new AdventureLandLibrary.Geometry.LineD(currentNode, portals[maxPortal].P1);
                            curFunnelLeftIndex = maxPortal;
                            curFunnelRightIndex = maxPortal;
                            DrawLine(funnelLeft, Brushes.Cyan, 3);
                            DrawLine(funnelRight, Brushes.Purple, 3);
                        }
                        else if (rightIsLeftOfRight)
                        {
                            funnelRight = new AdventureLandLibrary.Geometry.LineD(currentNode, rightPoint);
                            //funnelRightIndex = newFunnelRightIndex;
                            //curFunnelRightIndex = newFunnelRightIndex;
                            //DrawLine(funnelLeft, Brushes.Cyan, 3);
                            //DrawLine(funnelRight, Brushes.Purple, 3);
                        }
                        else if (leftIsRightOfLeft)
                        {
                            funnelLeft = new AdventureLandLibrary.Geometry.LineD(currentNode, leftPoint);
                            //funnelLeftIndex = newFunnelLeftIndex;
                            //curFunnelLeftIndex = newFunnelRightIndex;
                            DrawLine(funnelLeft, Brushes.Cyan, 3);
                            DrawLine(funnelRight, Brushes.Purple, 3);
                        }
                        //else if (!rightIsLeftOfRight)
                        //{
                        //    currentNode = new AdventureLandLibrary.Geometry.PointD(prevRightPoint.X, prevRightPoint.Y);

                        //    newPath.Add(new AdventureLandLibrary.Geometry.Point((int)currentNode.X - currentMap.OffsetX, (int)currentNode.Y - currentMap.OffsetY));
                        //    funnelLeft = new AdventureLandLibrary.Geometry.LineD(currentNode, leftPoint);
                        //    funnelRight = new AdventureLandLibrary.Geometry.LineD(currentNode, rightPoint);
                        //    DrawLine(funnelLeft, Brushes.Cyan, 2);
                        //    DrawLine(funnelRight, Brushes.Purple, 2);
                        //}
                        //else if (!leftIsRightOfLeft)
                        //{
                        //    currentNode = new AdventureLandLibrary.Geometry.PointD(prevLeftPoint.X, prevLeftPoint.Y);

                        //    newPath.Add(new AdventureLandLibrary.Geometry.Point((int)currentNode.X - currentMap.OffsetX, (int)currentNode.Y - currentMap.OffsetY));
                        //    funnelLeft = new AdventureLandLibrary.Geometry.LineD(currentNode, leftPoint);
                        //    funnelRight = new AdventureLandLibrary.Geometry.LineD(currentNode, rightPoint);
                        //    DrawLine(funnelLeft, Brushes.Cyan, 2);
                        //    DrawLine(funnelRight, Brushes.Purple, 2);
                        //}
                    }
                    else
                    {
                        var newfunnelLeft = new AdventureLandLibrary.Geometry.LineD(currentNode, leftPoint);
                        var newfunnelRight = new AdventureLandLibrary.Geometry.LineD(currentNode, rightPoint);

                        funnelLeftIndex = newFunnelLeftIndex;
                        funnelRightIndex = newFunnelRightIndex;
                        funnelLeft = newfunnelLeft;
                        funnelRight = newfunnelRight;
                        curFunnelLeftIndex = funnelLeftIndex;
                        curFunnelRightIndex = funnelRightIndex;

                        //DrawLine(funnelLeft, Brushes.Cyan);
                        //DrawLine(funnelRight, Brushes.Purple);
                    }
                    leftSide++;
                    rightSide++;

                    if(leftSide > 1)
                    {
                        leftSide = 0;
                    }

                    if(rightSide > 1)
                    {
                        rightSide = 0;
                    }
                    count++;
                }

                newPath.Add(new AdventureLandLibrary.Geometry.Point(endPoint.X, endPoint.Y));
                return newPath;
            }
            else
            {
                List<AdventureLandLibrary.Geometry.Point> newPath = new List<AdventureLandLibrary.Geometry.Point>();

                newPath.Add(startPoint);
                newPath.Add(endPoint);

                return newPath;
            }
        }

        public int NextLeft(int leftIndex, List<AdventureLandLibrary.Geometry.LineD> portals)
        {
            var curPoint = portals[leftIndex].P2;

            if (leftIndex < portals.Count - 1)
            {
                int nextIndex = leftIndex + 1;
                AdventureLandLibrary.Geometry.PointD nextPoint = portals[nextIndex].P2;

                while (curPoint.X == nextPoint.X && curPoint.Y == nextPoint.Y && nextIndex < portals.Count - 1)
                {
                    nextIndex++;
                    nextPoint = portals[nextIndex].P2;
                }

                return nextIndex;
            }
            else
            {
                return leftIndex;
            }
        }

        public int NextRight(int rightIndex, List<AdventureLandLibrary.Geometry.LineD> portals)
        {
            var curPoint = portals[rightIndex].P1;

            if (rightIndex < portals.Count - 1)
            {
                int nextIndex = rightIndex + 1;
                AdventureLandLibrary.Geometry.PointD nextPoint = portals[nextIndex].P1;

                while (curPoint.X == nextPoint.X && curPoint.Y == nextPoint.Y && nextIndex < portals.Count - 1)
                {
                    nextIndex++;
                    nextPoint = portals[nextIndex].P2;
                }

                return nextIndex;
            }
            else
            {
                return rightIndex;
            }
        }

        public AdventureLandLibrary.Geometry.Point GetMouseMapPoint()
        {
            var mousePos = Mouse.GetPosition(Canvas);

            return new AdventureLandLibrary.Geometry.Point((int)(mousePos.X - currentMap.OffsetX + xOffset), (int) (mousePos.Y - currentMap.OffsetY + yOffset));
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Canvas.Width = this.ActualWidth;
            Canvas.Height = this.ActualHeight;
        }

        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var endPoint = GetMouseMapPoint();
            PathEndPoint = endPoint;
        }
    }
}
