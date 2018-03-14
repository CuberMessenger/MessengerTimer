using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace MessengerTimer {
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>

    public sealed partial class ScramblePage : Page {
        private static Brush WhiteBrush = new SolidColorBrush(Windows.UI.Colors.White);
        private static Brush YellowBrush = new SolidColorBrush(Windows.UI.Colors.Yellow);
        private static Brush BlueBrush = new SolidColorBrush(Windows.UI.Colors.Blue);
        private static Brush GreenBrush = new SolidColorBrush(Windows.UI.Colors.Green);
        private static Brush RedBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private static Brush OrangeBrush = new SolidColorBrush(Windows.UI.Colors.Orange);

        public static Dictionary<char, Brush> FaceToBrush = new Dictionary<char, Brush> {
            { 'U', WhiteBrush},{ 'D', YellowBrush},{ 'F', GreenBrush},{ 'B', BlueBrush},{ 'R', RedBrush},{ 'L', OrangeBrush}
        };

        public List<Brush> brushes = new List<Brush>();

        public void RefreshScramble(string cube) {
            for (int i = 0; i < cube.Length; i++)
                brushes[i] = FaceToBrush[cube[i]];
            Bindings.Update();
        }

        public ScramblePage() {
            this.InitializeComponent();

            for (int i = 0; i < 54; i++)
                brushes.Add(null);
        }
    }
}
