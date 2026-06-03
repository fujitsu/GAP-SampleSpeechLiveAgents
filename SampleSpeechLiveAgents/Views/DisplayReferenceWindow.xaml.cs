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
using System.Windows.Shapes;

namespace SampleSpeechLiveAgents.Views
{
    public partial class DisplayReferenceWindow : Window
    {
        public ViewModels.MainViewModel ViewModel { get; } = App.MainVM;

        /// <summary>
        /// コンストラクター
        /// </summary>
        public DisplayReferenceWindow(MainWindow window)
        {
            this.Owner = window;
            InitializeComponent();
        }
    }
}
