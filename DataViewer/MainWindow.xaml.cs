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

            MapSelect.SelectedIndex = 11;
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
                System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
                
                //var path = currentMap.FindPath(pathStartPoint, PathEndPoint);
                var nodes = currentMap.FindPathDebug(pathStartPoint, PathEndPoint);
                timer.Start();
                var funneled = TunnelSmooth(nodes.ToList());
                timer.Stop();
                PathTime.Content = timer.ElapsedMilliseconds;

                for (var i = 0; i < funneled.Count - 1; i++)
                {
                    var p1 = funneled[i];
                    var p2 = funneled[i + 1];
                    var line2 = new Line();
                    line2.X1 = p1.X - xOffset + currentMap.OffsetX;
                    line2.X2 = p2.X - xOffset + currentMap.OffsetX;
                    line2.Y1 = p1.Y - yOffset + currentMap.OffsetY;
                    line2.Y2 = p2.Y - yOffset + currentMap.OffsetY;


                    line2.StrokeThickness = 2;
                    line2.Stroke = Brushes.Blue;

                    line2.MouseRightButtonDown += Window_MouseRightButtonDown;

                    currentPath.Add(line2);
                    Canvas.Children.Add(line2);
                }

                for (var i = 0; i < nodes.Length - 2; i++)
                {
                    var portal = nodes[i].GetPortal(nodes[i + 1]);

                    if (portal != null)
                    {
                        var line = new Line();
                        line.X1 = portal.P1.X - xOffset;
                        line.X2 = portal.P2.X - xOffset;
                        line.Y1 = portal.P1.Y - yOffset;
                        line.Y2 = portal.P2.Y - yOffset;
                    

                        line.StrokeThickness = 2;
                        line.Stroke = Brushes.Green;

                        line.MouseRightButtonDown += Window_MouseRightButtonDown;

                        currentPath.Add(line);
                        Canvas.Children.Add(line);

                        var right = new System.Windows.Shapes.Path();

                        right.StrokeThickness = 2;
                        right.Stroke = Brushes.Purple;

                        var rightEll = new EllipseGeometry();

                        rightEll.Center = new Point(portal.P1.X - xOffset, portal.P1.Y - yOffset);
                        rightEll.RadiusX = 5;
                        rightEll.RadiusY = 5;

                        right.Data = rightEll;

                        currentPath.Add(right);
                        Canvas.Children.Add(right);

                        var left = new System.Windows.Shapes.Path();

                        left.StrokeThickness = 2;
                        left.Stroke = Brushes.Cyan;

                        var leftEll = new EllipseGeometry();

                        leftEll.Center = new Point(portal.P2.X - xOffset, portal.P2.Y - yOffset);
                        leftEll.RadiusX = 5;
                        leftEll.RadiusY = 5;

                        left.Data = leftEll;

                        currentPath.Add(left);
                        Canvas.Children.Add(left);

                    }
                    else
                    {
                        var test = true;
                    }
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

        public List<AdventureLandLibrary.Geometry.Point> TunnelSmooth(List<AdventureLandLibrary.Geometry.GraphNode> path)
        {
            if (path.Count > 2)
            {
                List<AdventureLandLibrary.Geometry.Point> newPath = new List<AdventureLandLibrary.Geometry.Point>();

                List<AdventureLandLibrary.Geometry.LineD> portals = new List<AdventureLandLibrary.Geometry.LineD>();

                for (var i = 0; i < path.Count - 2; i++)
                {
                    portals.Add(path[i].GetPortal(path[i + 1]));
                }

                AdventureLandLibrary.Geometry.PointD currentNode = new AdventureLandLibrary.Geometry.PointD(path[0].center.X, path[0].center.Y);

                newPath.Add(new AdventureLandLibrary.Geometry.Point((int)currentNode.X - currentMap.OffsetX, (int)currentNode.Y - currentMap.OffsetY));

                int funnelLeftIndex = 1;
                int funnelRightIndex = 1;
                //int leftLastInside = 0;
                //int rightLastInside = 0;

                int currentPortal = 0;

                AdventureLandLibrary.Geometry.LineD funnelLeft = new AdventureLandLibrary.Geometry.LineD(currentNode, portals[0].P2);
                AdventureLandLibrary.Geometry.LineD funnelRight = new AdventureLandLibrary.Geometry.LineD(currentNode, portals[0].P1);

                int count = 0;
                while (funnelLeftIndex < portals.Count && funnelRightIndex < portals.Count && count < 100)
                {
                    var leftPortal = portals[funnelLeftIndex];
                    var rightPortal = portals[funnelRightIndex];

                    var insideFunnel = false;

                    //var dirLeftLeft = funnelLeft.Direction(leftPortal.P2);
                    //var dirRightRight = funnelRight.Direction(rightPortal.P1);
                    var leftIsRightOfLeft = funnelLeft.Direction(leftPortal.P2) <= 0;
                    var rightIsLeftOfRight = funnelRight.Direction(rightPortal.P1) >= 0;

                    //var dirLeftRight = funnelRight.Direction(leftPortal.P2);
                    //var dirRightLeft = funnelLeft.Direction(rightPortal.P1);
                    var leftIsLeftOfRight = funnelRight.Direction(leftPortal.P2) >= 0;
                    var rightIsRightOfLeft = funnelLeft.Direction(rightPortal.P1) <= 0;

                    if (leftIsRightOfLeft && rightIsLeftOfRight && leftIsLeftOfRight && rightIsRightOfLeft)
                    {
                        insideFunnel = true;
                    }

                    if (!insideFunnel)
                    {
                        if (leftIsLeftOfRight && leftIsRightOfLeft)
                        {
                            //leftLastInside = funnelLeftIndex;
                            funnelLeftIndex += 1;
                            funnelLeft = new AdventureLandLibrary.Geometry.LineD(currentNode, leftPortal.P2);

                        }
                        else if(leftIsLeftOfRight)
                        {
                            funnelLeftIndex += 1;
                            //funnelLeft = new AdventureLandLibrary.Geometry.LineD(currentNode, leftPortal.P2);
                        }

                        if (rightIsLeftOfRight && rightIsRightOfLeft)
                        {
                            //rightLastInside = funnelRightIndex;
                            funnelRightIndex += 1;
                            funnelRight = new AdventureLandLibrary.Geometry.LineD(currentNode, rightPortal.P1);
                        }
                        else if(rightIsRightOfLeft)
                        {
                            funnelRightIndex += 1;
                            //funnelRight = new AdventureLandLibrary.Geometry.LineD(currentNode, rightPortal.P1);
                        }

                        if(!rightIsRightOfLeft)
                        {
                            newPath.Add(new AdventureLandLibrary.Geometry.Point((int)funnelLeft.P2.X - currentMap.OffsetX, (int)funnelLeft.P2.Y - currentMap.OffsetY));
                            currentNode = funnelLeft.P2;

                            var minFunnelIndex = Math.Min(funnelLeftIndex, funnelRightIndex);

                            funnelLeft = new AdventureLandLibrary.Geometry.LineD(currentNode, portals[minFunnelIndex].P2);
                            funnelLeftIndex = minFunnelIndex + 1;
                            //leftLastInside = funnelLeftIndex;
                            funnelRight = new AdventureLandLibrary.Geometry.LineD(currentNode, portals[minFunnelIndex].P1);
                            funnelRightIndex = minFunnelIndex + 1;
                            //rightLastInside = funnelRightIndex;
                        }

                        if(!leftIsLeftOfRight)
                        {
                            newPath.Add(new AdventureLandLibrary.Geometry.Point((int)funnelRight.P2.X - currentMap.OffsetX, (int)funnelRight.P2.Y - currentMap.OffsetY));
                            currentNode = funnelRight.P2;

                            var minFunnelIndex = Math.Min(funnelLeftIndex, funnelRightIndex);

                            funnelLeft = new AdventureLandLibrary.Geometry.LineD(currentNode, portals[minFunnelIndex].P2);
                            funnelLeftIndex = minFunnelIndex + 1;
                            //leftLastInside = funnelLeftIndex;
                            funnelRight = new AdventureLandLibrary.Geometry.LineD(currentNode, portals[minFunnelIndex].P1);
                            funnelRightIndex = minFunnelIndex + 1;
                            //rightLastInside = funnelRightIndex;
                        }

                    }
                    else
                    {
                        //rightLastInside = funnelRightIndex;
                        //leftLastInside = funnelLeftIndex;
                        funnelLeftIndex += 1;
                        funnelRightIndex += 1;

                        funnelLeft = new AdventureLandLibrary.Geometry.LineD(currentNode, leftPortal.P2);

                        funnelRight = new AdventureLandLibrary.Geometry.LineD(currentNode, rightPortal.P1);
                    }
                    //count++;
                }

                newPath.Add(new AdventureLandLibrary.Geometry.Point(path.Last().center.X - currentMap.OffsetX, path.Last().center.Y - currentMap.OffsetY));
                return newPath;
            }
            else
            {
                List<AdventureLandLibrary.Geometry.Point> newPath = new List<AdventureLandLibrary.Geometry.Point>();

                foreach (var node in path)
                {
                    newPath.Add(new AdventureLandLibrary.Geometry.Point(node.center));
                }

                return newPath;
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
