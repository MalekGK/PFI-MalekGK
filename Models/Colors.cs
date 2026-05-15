using Microsoft.Ajax.Utilities;
using System.Collections.Generic;
using System.Linq;
// Auteur: Nicolas Chourot 2026
namespace SelectionDemo.Models
{
    
    public class Color
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Hex { get; set; }
        public string Caption => Hex + " " + Name;
    }

    public class Colors
    {
        public Colors()
        {
            Init();
        }
        private int index = 0;
        private void Init()
        {
            Add("aliceblue", "#f0f8ff");
            Add("antiquewhite", "#faebd7");
            Add("aqua", "#00ffff");
            Add("aquamarine", "#7fffd4");
            Add("azure", "#f0ffff");
            Add("beige", "#f5f5dc");
            Add("bisque", "#ffe4c4");
            Add("black", "#000000");
            Add("blanchedalmond", "#ffebcd");
            Add("blue", "#0000ff");
            Add("blueviolet", "#8a2be2");
            Add("brown", "#a52a2a");
            Add("burlywood", "#deb887");
            Add("cadetblue", "#5f9ea0");
            Add("chartreuse", "#7fff00");
            Add("chocolate", "#d2691e");
            Add("coral", "#ff7f50");
            Add("cornflowerblue", "#6495ed");
            Add("cornsilk", "#fff8dc");
            Add("crimson", "#dc143c");
            Add("cyan", "#00ffff");
            Add("darkblue", "#00008b");
            Add("darkcyan", "#008b8b");
            Add("darkgoldenrod", "#b8860b");
            Add("darkgray", "#a9a9a9");
            Add("darkgreen", "#006400");
            Add("darkkhaki", "#bdb76b");
            Add("darkmagenta", "#8b008b");
            Add("darkolivegreen", "#556b2f");
            Add("darkorange", "#ff8c00");
            Add("darkorchid", "#9932cc");
            Add("darkred", "#8b0000");
            Add("darksalmon", "#e9967a");
            Add("darkseagreen", "#8fbc8f");
            Add("darkslateblue", "#483d8b");
            Add("darkslategrey", "#2f4f4f");
            Add("darkturquoise", "#00ced1");
            Add("darkviolet", "#9400d3");
            Add("deeppink", "#ff1493");
            Add("deepskyblue", "#00bfff");
            Add("dimgray", "#696969");
            Add("dodgerblue", "#1e90ff");
            Add("firebrick", "#b22222");
            Add("floralwhite", "#fffaf0");
            Add("forestgreen", "#228b22");
            Add("fuchsia", "#ff00ff");
            Add("gainsboro", "#dcdcdc");
            Add("ghostwhite", "#f8f8ff");
            Add("goldenrod", "#daa520");
            Add("gold", "#ffd700");
            Add("green", "#008000");
            Add("greenyellow", "#adff2f");
            Add("grey", "#808080");
            Add("honeydew", "#f0fff0");
            Add("hotpink", "#ff69b4");
            Add("indianred", "#cd5c5c");
            Add("indigo", "#4b0082");
            Add("ivory", "#fffff0");
            Add("khaki", "#f0e68c");
            Add("lavenderblush", "#fff0f5");
            Add("lavender", "#e6e6fa");
            Add("lawngreen", "#7cfc00");
            Add("lemonchiffon", "#fffacd");
            Add("lightblue", "#add8e6");
            Add("lightcoral", "#f08080");
            Add("lightcyan", "#e0ffff");
            Add("lightgoldenrodyellow", "#fafad2");
            Add("lightgray", "#d3d3d3");
            Add("lightgreen", "#90ee90");
            Add("lightpink", "#ffb6c1");
            Add("lightsalmon", "#ffa07a");
            Add("lightseagreen", "#20b2aa");
            Add("lightskyblue", "#87cefa");
            Add("lightslategray", "#778899");
            Add("lightsteelblue", "#b0c4de");
            Add("lightyellow", "#ffffe0");
            Add("lime", "#00ff00");
            Add("limegreen", "#32cd32");
            Add("linen", "#faf0e6");
            Add("magenta", "#ff00ff");
            Add("maroon", "#800000");
            Add("mediumaquamarine", "#66cdaa");
            Add("mediumblue", "#0000cd");
            Add("mediumorchid", "#ba55d3");
            Add("mediumpurple", "#9370db");
            Add("mediumseagreen", "#3cb371");
            Add("mediumslateblue", "#7b68ee");
            Add("mediumspringgreen", "#00fa9a");
            Add("mediumturquoise", "#48d1cc");
            Add("mediumvioletred", "#c71585");
            Add("midnightblue", "#191970");
            Add("mintcream", "#f5fffa");
            Add("mistyrose", "#ffe4e1");
            Add("moccasin", "#ffe4b5");
            Add("navajowhite", "#ffdead");
            Add("navy", "#000080");
            Add("oldlace", "#fdf5e6");
            Add("olive", "#808000");
            Add("olivedrab", "#6b8e23");
            Add("orange", "#ffa500");
            Add("orangered", "#ff4500");
            Add("orchid", "#da70d6");
            Add("palegoldenrod", "#eee8aa");
            Add("palegreen", "#98fb98");
            Add("paleturquoise", "#afeeee");
            Add("palevioletred", "#db7093");
            Add("papayawhip", "#ffefd5");
            Add("peachpuff", "#ffdab9");
            Add("peru", "#cd853f");
            Add("pink", "#ffc0cb");
            Add("plum", "#dda0dd");
            Add("powderblue", "#b0e0e6");
            Add("purple", "#800080");
            Add("rebeccapurple", "#663399");
            Add("red", "#ff0000");
            Add("rosybrown", "#bc8f8f");
            Add("royalblue", "#4169e1");
            Add("saddlebrown", "#8b4513");
            Add("salmon", "#fa8072");
            Add("sandybrown", "#f4a460");
            Add("seagreen", "#2e8b57");
            Add("seashell", "#fff5ee");
            Add("sienna", "#a0522d");
            Add("silver", "#c0c0c0");
            Add("skyblue", "#87ceeb");
            Add("slateblue", "#6a5acd");
            Add("slategray", "#708090");
            Add("snow", "#fffafa");
            Add("springgreen", "#00ff7f");
            Add("steelblue", "#4682b4");
            Add("tan", "#d2b48c");
            Add("teal", "#008080");
            Add("thistle", "#d8bfd8");
            Add("tomato", "#ff6347");
            Add("turquoise", "#40e0d0");
            Add("violet", "#ee82ee");
            Add("wheat", "#f5deb3");
            Add("white", "#ffffff");
            Add("whitesmoke", "#f5f5f5");
            Add("yellow", "#ffff00");
            Add("yellowgreen", "#9acd32");
        }
        private readonly List<Color> Items = new List<Color>();
        private void Add(string name, string hex)
        {
            index++;
            Items.Add(new Color { Id = index, Name = name, Hex = hex.ToUpper() });
        }

        public List<Color> ToList() => Items.ToList();
        public Color Get(int id) => Items[id-1];
    }

    public class ColorChoice
    {
        public int ColorId { get; set; }
        public Color Color => DB.Colors.Get(ColorId);
    }

    public class ColorChoices
    {
        private readonly List<ColorChoice> _colors = new List<ColorChoice>();
        public List<Color> ToList()
        {
            List<Color> colors = new List<Color>();
            foreach (var item in _colors)
            {
                colors.Add(item.Color);
            }
            return colors;
        }
        public void Update(List<int> ColorChoicesId)
        {
            _colors.Clear();
            if (ColorChoicesId != null)
                foreach (int colorId in ColorChoicesId)
                    _colors.Add(new ColorChoice { ColorId = colorId });
        }
    }

    public sealed class DB
    {
        #region singleton setup
        private static readonly DB instance = new DB();
        public static DB Instance { get { return instance; } }
        #endregion

        static public ColorChoices ColorChoices = new ColorChoices();
        static public Colors Colors = new Colors();
    }

}