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
            CC_Info info = new CC_Info();
            info.SampleRate = 8000;
            info.BufferSize = info.SampleRate / 20;

            IS_MidiIn inputSource = new IS_MidiIn(info,
                (CC_Channel chan) =>
                {
                    List<IAudioChainMember> is_chain = new List<IAudioChainMember>();
                    is_chain.Add(new CM_Oscillator(chan, (float freq, int step, int sr) =>
                    {
                        double oi = 2 * Math.PI * freq * step / (double)sr;
                        return (float)(Math.Cos(oi) + 0.375 * Math.Cos(3 * oi) +
                            0.581 * Math.Cos(5 * oi) + 0.382 * Math.Cos(7 * oi) +
                            0.141 * Math.Cos(9 * oi) + 0.028 * Math.Cos(11 * oi) + 0.009 * Math.Cos(13 * oi));
                    }));
                    //is_chain.Add(new CM_ChannelTerminator(chan));
                    is_chain.Add(new CM_ADSR_Envelope(chan, new float[] { 0.3f, 0, 0.3f }, 1, CM_ADSR_Envelope.ADSRLinearFunction));
                    chan.SetChain(is_chain);
                });

            CC_Channel outChannel = new CC_Channel(info);
            List<IAudioChainMember> chain = new List<IAudioChainMember>();

            chain.Add(new CM_Normalizator(outChannel));
            chain.Add(new CM_BufferVisualizer(outChannel, 800));

            outChannel.SetChain(chain);
            outChannel.Activate();

            aio = new AudioInOut(inputSource, outChannel, info);
            aio.StartLoop();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            aio.Dispose();
            aio = null;
        }
    }
}
