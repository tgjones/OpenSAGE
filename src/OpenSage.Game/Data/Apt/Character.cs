﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSage.Data.Apt.Characters;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Apt
{
    public enum CharacterType : uint
    {
        SHAPE = 1,
        TEXT = 2,
        FONT = 3,
        BUTTON = 4,
        SPRITE = 5,
        SOUND = 6,
        IMAGE = 7,
        MORPH = 8,
        MOVIE = 9,
        STATICTEXT = 10,
        NONE = 11,
        VIDEO = 12
    };

    //base class for all characters used in apt
    public class Character
    {
        private const uint SIGNATURE = 0x09876543;

        static public Character Create(BinaryReader br)
        {
            Character ch = new Character();

            var type = br.ReadUInt32AsEnum<CharacterType>();
            var sig = br.ReadUInt32();

            if (sig != SIGNATURE)
                throw new InvalidDataException();

            switch(type)
            {
                case CharacterType.MOVIE:
                    ch = new Movie(br);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return ch;
        }
    }
}
