using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MidiSynth.ChainCommon
{
    class CC_Channel
    {
        public List<IAudioChainMember> chain;
        public Object tag = null;
        public CC_Info Info;
        public bool Active = false;
        public float defaultInput = 0;
        public bool InputEnded = false;

        public CC_Channel(CC_Info info)
        {
            Info = info;
        }

        public void Activate(bool doActivate = true)
        {
            Active = doActivate;
            InputEnded = false;
        }

        public void SetChain(List<IAudioChainMember> chain)
        {
            this.chain = chain;
        }

        public float Process()
        {
            return Process(defaultInput);
        }

        public float Process(float input)
        {
            foreach (IAudioChainMember m in chain)
                input = m.Process(input);
            return input;
        }

        public void Free()
        {
            Activate(false);
            tag = null;
        }
    }
}
