using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MidiSynth.ChainCommon
{
    interface IAudioInputSource : IDisposable
    {
        float GetOutput();
    }
}
