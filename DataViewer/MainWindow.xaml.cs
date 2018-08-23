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

        List<Line> currentPath = new List<Line>();

        AdventureLandLibrary.Geometry.Point PathEndPoint;

        Point initial;
        public MainWindow()
        {
            InitializeComponent();

            maps = ((JObject)Loader.data.geometry).Properties().Select(p => p.Name).ToList();

            MapSelect.ItemsSource = maps;
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

                currentPath = new List<Line>();

                var pathStartPoint = GetMouseMapPoint();
                System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
                timer.Start();
                var path = currentMap.FindPath(pathStartPoint, PathEndPoint);

                if (chkSmooth.IsChecked.HasValue && chkSmooth.IsChecked.Value)
                {
                    var smoothedpath = currentMap.SmoothPath(path);
                    path = smoothedpath;
                }
                timer.Stop();
                PathTime.Content = timer.ElapsedMilliseconds;

                for (var i = 0; i < path.Length - 1; i++)
                {
                    var p1 = path[i];
                    var p2 = path[i + 1];

                    var line = new Line();
                    line.X1 = p1.X + currentMap.OffsetX - xOffset;
                    line.X2 = p2.X + currentMap.OffsetX - xOffset;
                    line.Y1 = p1.Y + currentMap.OffsetY - yOffset;
                    line.Y2 = p2.Y + currentMap.OffsetY - yOffset;

                    line.StrokeThickness = 2;
                    line.Stroke = Brushes.Blue;

                    line.MouseRightButtonDown += Window_MouseRightButtonDown;

                    currentPath.Add(line);
                    Canvas.Children.Add(line);
                }

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
