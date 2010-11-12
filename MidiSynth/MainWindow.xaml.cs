using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MidiSynth.ChainCommon;
using MidiSynth.ChainMembers;
using MidiSynth.InputSources;

namespace MidiSynth
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AudioInOut aio = null;
        public static BufferPlot drawingSurface;
        public MainWindow()
        {
            InitializeComponent();
            drawingSurface = canvas1;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Window_Closed(object sender, EventArgs e)
        {
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IAudioInputSource inputSource;
            CC_Channel outChannel;

            ConfSetup.Setup(out inputSource, out outChannel);

            aio = new AudioInOut(inputSource, outChannel, ConfSetup.Info);
            aio.StartLoop();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            aio.Dispose();
            aio = null;
        }
    }
}
