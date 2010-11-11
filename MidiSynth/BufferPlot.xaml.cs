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
using System.Windows.Threading;
using System.Threading;

namespace MidiSynth
{
    /// <summary>
    /// Interaction logic for BufferPlot.xaml
    /// </summary>
    public partial class BufferPlot : Canvas
    {
        private float[] _buffer = null;
        private Object _bufferLock = new Object();

        public BufferPlot()
        {
            InitializeComponent();
        }

        private delegate void InvalidateMeDelegate();

        public void InvalidateMe()
        {
            if (this.Dispatcher.CheckAccess())
            {
                this.InvalidateVisual();
            }
            else
            {
                // Async call (BeginInvoke instead of Invoke) fixes problem if this is being called when Form is closing...
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new InvalidateMeDelegate(InvalidateMe));
            }
        }

        public void UpdateBuffer(float[] buffer)
        {
            lock (_bufferLock)
            {
                _buffer = new float[buffer.Length];
                Array.Copy(buffer, _buffer, buffer.Length);
            }
            this.InvalidateMe();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            lock (_bufferLock)
            {
                if (_buffer != null)
                {
                    int size = _buffer.Length;
                    int xstep = (int)Math.Floor(size / this.ActualWidth);
                    if (xstep == 0) xstep = 1;

                    Pen pen = new Pen(new SolidColorBrush(Colors.GreenYellow), 2);

                    Point prevPoint = new Point(0, 0);
                    int x = 0;
                    for (int i = 0; i < size; i += xstep)
                    {
                        Point p = new Point(x++, (int)((_buffer[i] + 1) / 2.0 * this.ActualHeight));
                        if (i != 0)
                            drawingContext.DrawLine(pen, prevPoint, p);
                        prevPoint = p;
                    }
                }
            }
        }

    }
}
