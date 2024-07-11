using System;
using System.Collections.Generic;
using System.Text;
using DXSharp.Sound;
using System.IO;
using System.Runtime.InteropServices;

namespace Planes3D
{
    public sealed class SoundSource
    {

    }

    public static class SoundLoader
    {
        public struct WavHeader
        {
            public int chunkId;
            public int chunkSize;
            public int format;
            public int subchunkid;
            public int subchunksize;
            public short audioFormat;
            public short numChannels;
            public int sampleRate;
            public int byteRate;
            public short blockAlign;
            public short bitsPerSample;
            public int subchunk2Id;
            public int subchunk2Size;
        }

        public static WaveBuffer LoadFromStream(string debugName, Stream strm)
        {
            if(strm != null)
            {
                BinaryReader reader = new BinaryReader(strm);
                WavHeader hdr = new WavHeader()
                {
                    chunkId = reader.ReadInt32(),
                    chunkSize = reader.ReadInt32(),
                    format = reader.ReadInt32(),
                    subchunkid = reader.ReadInt32(),
                    subchunksize = reader.ReadInt32(),
                    audioFormat = reader.ReadInt16(),
                    numChannels = reader.ReadInt16(),
                    sampleRate = reader.ReadInt32(),
                    byteRate = reader.ReadInt32(),
                    blockAlign = reader.ReadInt16(),
                    bitsPerSample = reader.ReadInt16(),
                    subchunk2Id = reader.ReadInt32(),
                    subchunk2Size = reader.ReadInt32()
                };

                if (hdr.numChannels > 2 || hdr.bitsPerSample > 16)
                {
                    Log.WriteLine("Can't load wave file {0}: Unsupported format", debugName);

                    return null;
                }

                byte[] pcmData = new byte[strm.Length - strm.Position];
                reader.Read(pcmData, 0, pcmData.Length);

                WaveFormat fmt = new WaveFormat();
                fmt.BitsPerSample = (ushort)hdr.bitsPerSample;
                fmt.BlockAlign = (ushort)hdr.blockAlign;
                fmt.SamplesPerSec = (uint)hdr.sampleRate;
                fmt.FormatTag = (ushort)hdr.audioFormat;
                fmt.AvgBytesPerSec = (uint)(hdr.sampleRate * hdr.numChannels * 2);
                fmt.Channels = (ushort)hdr.numChannels;

                return new WaveBuffer(fmt, pcmData);
            }

            return null;
        }

        public static WaveBuffer LoadFromFile(string fileName)
        {
            if(File.Exists(fileName))
            {
                using (Stream strm = File.OpenRead(fileName))
                    return LoadFromStream(fileName, strm);
            }

            return null;
        }
    }

    public sealed class WaveBuffer
    {
        private SoundBuffer buffer;

        public WaveBuffer(WaveFormat fmt, byte[] pcmData)
        {
            BufferDescription desc = new BufferDescription();
            desc.BufferBytes = (uint)pcmData.Length;
            desc.Flags = BufferFlags.ControlDefault |BufferFlags.Software;
            desc.Format = fmt;

            buffer = Engine.Current.Sound.Context.CreateSoundBuffer(desc);
            IntPtr data = buffer.Lock();
            Marshal.Copy(pcmData, 0, data, pcmData.Length);
            buffer.Unlock();

            buffer.Play();
        }
    }

    public sealed class SoundDevice
    {
        public const int WaveFormatPCM = 1; // TODO: Move somewhere else

        public DirectSound Context;
        public float Volume;

        private SoundBuffer primaryBuffer;

        public SoundDevice()
        {
            Log.WriteLine("Initializing DirectSound");

            Context = new DirectSound();
            Context.Initialize();

            Log.WriteLine("Creating primary sound buffer...");
            BufferDescription desc = new BufferDescription();
            desc.Flags = BufferFlags.PrimaryBuffer | BufferFlags.Control3D;
            
            primaryBuffer = Context.CreateSoundBuffer(desc);
        }
    }
}
