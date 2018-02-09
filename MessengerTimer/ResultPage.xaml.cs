using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TimeData;
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
    public sealed partial class ResultPage : Page {
        public ObservableCollection<Result> Results;
        public ResultPage() {
            this.InitializeComponent();
            Results = new ObservableCollection<Result>();
            Results.Add(new Result() { Id = 0, result = new TimeSpan(123145124), ao5Value = 12.345, ao12Value = 67.890 });
            Results.Add(new Result() { Id = 1, result = new TimeSpan(12314123124), ao5Value = 12.5, ao12Value = 67.0 });
            Results.Add(new Result() { Id = 2, result = new TimeSpan(121235124), ao5Value = 1, ao12Value = 6 });

        }
    }
}
