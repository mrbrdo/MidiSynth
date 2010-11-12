using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MidiSynth.ChainCommon;

namespace MidiSynth.ChainMembers
{
    class CM_KarplusStrong : AudioChainMember
    {
        float K;
        float[] delay = null;

        public CM_KarplusStrong(CC_Channel c, float K = 1) : base(c)
        {
            this.K = K;
        }

        public override float Process(float input)
        {
            // assume input = frequency
            int dLength = (int)(c.Info.SampleRate / input - 0.5);
            if (delay == null || delay.Length != dLength)
            {
                delay = new float[dLength];
                // white noise input
                Random r = new Random();
                float sum = 0;
                for (int i = 0; i < delay.Length; i++)
                {
                    delay[i] = (float)r.NextDouble() * 2;
                    sum += delay[i];
                }
                sum /= delay.Length;
                for (int i = 0; i < delay.Length; i++)
                    delay[i] -= sum; // subtract mean value
            }
            float result = K * (float)(0.5 * delay[delay.Length - 1] + 0.5 * delay[delay.Length - 2]);
            float[] delay2 = new float[delay.Length];
            delay2[0] = result;
            Array.Copy(delay, 0, delay2, 1, delay.Length - 1);
            delay = delay2;

            return result;
        }
    }
}
