using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Threading;
using System.Threading;

namespace MemoryMatchGame
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            PlayArea arena = new PlayArea();
            arena.StartArea(playArea,textBlock, startButton);//playarea to grid z xamla
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class PlayArea
    {
        private Element[,] elem;
        private int ile;
        public void StartArea(Grid area,TextBlock textBlock, Button startButton)
        {
            double width = area.ActualWidth / 100, height = area.ActualHeight / 100; //ile jpg zmiesci sie w pion/poziom
            area.MinWidth = area.ActualWidth; area.MinHeight = area.MinHeight;
            area.MaxWidth = area.ActualWidth; area.MaxHeight = area.MaxHeight;  area.ShowGridLines = true;
            clearArea(area);

            ColumnDefinition[] columns = new ColumnDefinition[(int)width];
            RowDefinition[] rows = new RowDefinition[(int)height];
            createColumnsRows(columns, rows, area);

            List<ImageBrush> imgs = downloadJpgList();//pobieranie jpgow z folderu
            int[] howMany = new int[imgs.Count];
            elem = new Element[columns.Length, rows.Length];
            Array.Clear(howMany, 0, howMany.Length);
            Array.Clear(elem, 0, elem.Length);
            
            setBoxElements(area,howMany, imgs, rows.Length, columns.Length);
            setNullElements(columns.Length, rows.Length, area);
            
            startButton.IsEnabled = false;
            setTimer(textBlock, startButton);
        }

        private void createColumnsRows(ColumnDefinition[] columns, RowDefinition[] rows, Grid area)
        {
            Array.Clear(rows, 0, rows.Length);
            Array.Clear(columns, 0, columns.Length);
            for (int i = 0; i < rows.Length; i++)
            {
                rows[i] = new RowDefinition();
                area.RowDefinitions.Add(rows[i]);
            }   
            for (int i = 0; i < columns.Length; i++)
            {
                columns[i] = new ColumnDefinition();
                area.ColumnDefinitions.Add(columns[i]);
            }
        }

        private void clearArea(Grid area)
        {
            area.RowDefinitions.Clear();
            area.ColumnDefinitions.Clear();
            area.Children.Clear();
        }

        private void setTimer(TextBlock textBlock, Button startButton)
        {
            DispatcherTimer timer = new DispatcherTimer();
            DispatcherTimer clock = new DispatcherTimer();
            DateTime timerStart;
            clock.Interval = new TimeSpan(100);
            timerStart = DateTime.Now;
            clock.Start();
            clock.Tick += (sender, EventArgs) =>
            {
                var curr = DateTime.Now - timerStart;
                textBlock.Text = (curr.Seconds).ToString();
                if(curr.Seconds>=10)
                {
                    clock.Stop();
                    textBlock.Text = "Start!";
                    startButton.IsEnabled = true;
                    ElClickUstaw(elem);

                    textBlockChange(textBlock, startButton);
                }
            };
            timer.Interval = new TimeSpan(0, 0, 10);
            timer.Tick += (sender, EventArgs) =>
            {
                showMsgHideAll(elem);
                timer.Stop();
            };
            timer.Start();
        }

        private async void textBlockChange(TextBlock textBlock,Button startButton)
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(500);
            DateTime start = DateTime.Now;
            timer.Tick += (sender, e) => Timer_Tick(sender, e, textBlock, start, startButton);
            timer.Start();
            await Task.Delay(3000);
            textBlock.Text = "";
        }

        private void Timer_Tick(object sender, EventArgs e, TextBlock textBlock, DateTime start, Button startButton)
        {
            var current = DateTime.Now - start;
            textBlock.Text = current.TotalSeconds.ToString();
            if(areAllDiscovered() || startButton.IsPressed)
                (sender as DispatcherTimer).Stop();
        }

        private void setBoxElements(Grid area,int[] howMany, List<ImageBrush> imgs, int rows, int columns)
        {
            Random rand = new Random();
            int k = 0, a = 0, b = 0;
            while (k < imgs.Count)
            {
                do
                {
                    a = rand.Next(0, columns); b = rand.Next(0, rows);
                } while (howMany[k] >= 2);
                if (howMany[k] < 2)
                {
                    if (elem[a, b] != null) continue;
                    elem[a, b] = new Element(imgs.ElementAt(k));
                    Grid.SetColumn(elem[a, b], a);
                    Grid.SetRow(elem[a, b], b);
                    area.Children.Add(elem[a, b]);
                    howMany[k]++;
                }
                if (howMany[k] > 1) k++;
            }
        }

        private void setNullElements(int a, int b, Grid area)
        {
            for (int i = 0; i < a; i++)
                for (int j = 0; j < b; j++)
                    if (elem[i, j] == null)
                    {
                        elem[i, j] = new Element("null", brush());
                        Grid.SetColumn(elem[i, j], i);
                        Grid.SetRow(elem[i, j], j);
                        area.Children.Add(elem[i, j]);
                    }
        }

        private void Element_Click1(object sender, RoutedEventArgs e)
        {
            Element element = sender as Element;
            element.last = true;
            element.Background = element.id;
            checkElements(element);
        }

        private async void checkElements(Element element)
        {
            foreach(Element cell in elem)
            {
                if(!Equals(cell, element) && cell.last &&element.last &&cell.odsloniete==false &&element.odsloniete==false && cell.name==element.name)
                {
                    cell.odsloniete = true;
                    element.odsloniete = true;
                    cell.last = false;
                    element.last = false;
                    cell.IsEnabled = false;
                    element.IsEnabled = false;
                }
            }
            ile = 0;
            foreach (Element cell in elem)
                if (cell.last) 
                    ile++;
            if (ile >= 2)
            {
                ElClickUsun(elem);
                await Task.Delay(3000);
                showMsgHideAll(elem);
                ElClickUstaw(elem);
            }
            if (areAllDiscovered())
                MessageBox.Show("All are discovered!!!");
        }

        private bool areAllDiscovered()
        {
            foreach(Element element in elem)
            {
                if (element.name == "null") continue;
                if (element.odsloniete == false)
                    return false;
            }
            return true;
        }

        private void ElClickUstaw(Element[,] elements)
        {
            foreach (Element element in elements)
                element.Click += Element_Click1;
        }
        private void ElClickUsun(Element[,] elements)
        {
            foreach (Element element in elements)
                element.Click -= Element_Click1;
        }

        public void showMsgHideAll(Element[,] elem)
        {
            foreach(Element cell in elem)
            {
                if (cell==null) continue;
                cell.last = false;
                cell.Background = brush();
            }
        }
        private LinearGradientBrush brush()
        {
            LinearGradientBrush myBrush = new LinearGradientBrush();
            myBrush.GradientStops.Add(new GradientStop(Colors.Yellow, 0.0));
            myBrush.GradientStops.Add(new GradientStop(Colors.Orange, 0.5));
            myBrush.GradientStops.Add(new GradientStop(Colors.MistyRose, 1.0));
            return myBrush;
        }
        private List<ImageBrush> downloadJpgList()
        {
            string source = Directory.GetCurrentDirectory();
            List<string> names = new List<string>();
            List<ImageBrush> list = new List<ImageBrush>();
            try
            {
                var jpgFiles = Directory.EnumerateFiles(source, "Images\\*.jpg");
                foreach (string currentFile in jpgFiles)
                {
                    string fileName = currentFile.Substring(source.Length + 1);
                    if (!names.Contains(fileName))
                        names.Add(fileName);
                    ImageBrush img = new ImageBrush();
                    img.ImageSource = new BitmapImage(new Uri(fileName, UriKind.Relative));
                    list.Add(img);
                }
            }
            catch(Exception e)
            {
                MessageBox.Show("Can not load images. Closing app");
                Environment.Exit(0);
            }
            return list;
        }      
    }

    public class Element:Button
    {
        private string _name;
        private bool _odsloniete;
        private bool _last;
        private ImageBrush _id;
        public bool odsloniete { get{ return _odsloniete;} set{ _odsloniete = value; }}
        public bool last { get { return _last; } set { _last = value; } }
        public string name { get { return _name; } }
        public ImageBrush id { get { return _id; } }
        public Element(ImageBrush img)
        {
            _name = nameValueSet(img.ImageSource.ToString());
            _id = img;
            _odsloniete = false;
            _last = false;
            Width = 70;
            Height = 70;
            Background = img;
        }
        public Element(string name, LinearGradientBrush brush)
        {
            _name = name;
            _id = null;
            _odsloniete = false;
            _last = false;
            Width = 70;
            Height = 70;
            Background = brush;
        }
        private string nameValueSet(string s)
        {
            string name= "";
            if(s.Contains('\\'))
            {
                int i = s.LastIndexOf('\\');
                name = s.Substring(i + 1);
                return name;
            }
            else return s;
        }


    }
}
