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
using Windows.UI.Xaml.Shapes;

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
        private static Style RectangleStyle = new Style();

        public static Dictionary<char, Brush> FaceToBrush = new Dictionary<char, Brush> {
            { 'U', WhiteBrush},{ 'D', YellowBrush},{ 'F', GreenBrush},{ 'B', BlueBrush},{ 'R', RedBrush},{ 'L', OrangeBrush}
        };

        private int Order { get; set; }

        private void GenerateFaceGrid(ref Grid grid) {
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();
            GridLength gridLength = new GridLength(1d, GridUnitType.Star);
            for (int i = 0; i < Order; i++) {
                grid.RowDefinitions.Add(new RowDefinition { Height = gridLength });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength });
            }

            grid.Children.Clear();
            for (int i = 0; i < Order; i++) {
                for (int j = 0; j < Order; j++) {
                    Rectangle rectangle = new Rectangle { Style = RectangleStyle };
                    rectangle.SetValue(Grid.RowProperty, i);
                    rectangle.SetValue(Grid.ColumnProperty, j);
                    grid.Children.Add(rectangle);
                }
            }

        }

        private void SetFaceColor(ref Grid grid, string face) {
            int i = 0;
            foreach (Rectangle rec in grid.Children) {
                rec.Fill = FaceToBrush[face[i++]];
            }
        }

        public void RefreshScramble(string cube, int order) {
            if (order != Order) {
                Order = order;

                GenerateFaceGrid(ref UFaceGrid);
                GenerateFaceGrid(ref RFaceGrid);
                GenerateFaceGrid(ref FFaceGrid);
                GenerateFaceGrid(ref DFaceGrid);
                GenerateFaceGrid(ref LFaceGrid);
                GenerateFaceGrid(ref BFaceGrid);
            }
            int start = 0;
            int nos = Order * Order;
            SetFaceColor(ref UFaceGrid, cube.Substring(start, nos));
            SetFaceColor(ref RFaceGrid, cube.Substring(start += nos, nos));
            SetFaceColor(ref FFaceGrid, cube.Substring(start += nos, nos));
            SetFaceColor(ref DFaceGrid, cube.Substring(start += nos, nos));
            SetFaceColor(ref LFaceGrid, cube.Substring(start += nos, nos));
            SetFaceColor(ref BFaceGrid, cube.Substring(start += nos, nos));
        }

        public ScramblePage() {
            this.InitializeComponent();

            if (RectangleStyle.Setters.Count == 0) {
                RectangleStyle.TargetType = typeof(Rectangle);
                RectangleStyle.Setters.Add(new Setter(Shape.StrokeProperty, Windows.UI.Colors.Gray));
                RectangleStyle.Setters.Add(new Setter(Shape.StrokeThicknessProperty, 0.5d));
            }
        }
    }
}
