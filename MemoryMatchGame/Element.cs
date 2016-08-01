using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;


namespace MemoryMatchGame
{
    public class Element : Button
    {
        private string _name;
        private bool _odsloniete;
        private bool _last;
        private ImageBrush _id;
        public bool odsloniete { get { return _odsloniete; } set { _odsloniete = value; } }
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
            string name = "";
            if (s.Contains('\\'))
            {
                int i = s.LastIndexOf('\\');
                name = s.Substring(i + 1);
                return name;
            }
            else return s;
        }
    }
}
