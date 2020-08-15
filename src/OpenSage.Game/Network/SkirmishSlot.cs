﻿using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using LiteNetLib.Utils;
using SixLabors.ImageSharp.Processing;

namespace OpenSage.Network
{
    public enum SkirmishSlotState : byte
    {
        Closed,
        Open,
        Human,
        EasyArmy,
        MediumArmy,
        HardArmy,
    }

    [Serializable]
    public class SkirmishSlot
    {
        public SkirmishSlot()
        {
        }

        public SkirmishSlot(int index)
        {
            Index = index;
        }

        public int Index { get; set; }
        public SkirmishSlotState State { get; set; } = SkirmishSlotState.Open;
        public string PlayerName { get; set; }
        public byte ColorIndex { get; set; }
        public byte FactionIndex { get; set; }
        public byte Team { get; set; }
        public bool Ready { get; set; }

        [NonSerialized]
        public IPEndPoint EndPoint;

        /// <summary>
        /// We need this during development to be able to run two games on the same machine.
        /// </summary>
        public int ProcessId { get; set; }

        private byte[] _cleanState = new byte[1];
        public bool IsDirty {
            get
            {
                return !_cleanState.Equals(_bytes);
            }
            internal set {
                if (!value)
                {
                    _cleanState = _bytes;
                }
            }
        }

        private byte[] _bytes
        {
            get
            {
                BinaryFormatter bf = new BinaryFormatter();
                using (var ms = new MemoryStream())
                {
                    bf.Serialize(ms, this);
                    return ms.ToArray();
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            //Maybe filter 
            IsDirty = true;
        }


        public static SkirmishSlot Deserialize(NetDataReader reader)
        {
            var slot = new SkirmishSlot()
            {
                Index = reader.GetInt(),
                State = (SkirmishSlotState) reader.GetByte(),
                PlayerName = reader.GetString(),
                ColorIndex = reader.GetByte(),
                FactionIndex = reader.GetByte(),
                Team = reader.GetByte(),
                Ready = reader.GetBool()
            };

            if (slot.State == SkirmishSlotState.Human)
            {
                slot.EndPoint = reader.GetNetEndPoint();
                slot.ProcessId = reader.GetInt();
            }

            return slot;
        }

        public static void Serialize(NetDataWriter writer, SkirmishSlot slot)
        {
            writer.Put(slot.Index);
            writer.Put((byte) slot.State);
            writer.Put(slot.PlayerName);
            writer.Put(slot.ColorIndex);
            writer.Put(slot.FactionIndex);
            writer.Put(slot.Team);
            writer.Put(slot.Ready);
            if (slot.State == SkirmishSlotState.Human)
            {
                writer.Put(slot.EndPoint);
                writer.Put(slot.ProcessId);
            }
        }
    }
}
