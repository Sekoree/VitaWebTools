using BasicDataTypes;

// A Sfo Parser Written by SilicaAndPina
// Because all the others are overly-complicated for no reason!
// MIT Licensed.

namespace VitaWebTools.SfoNET
{
    internal class Sfo
    {
        private const int PSF_TYPE_BIN = 0;
        private const int PSF_TYPE_STR = 2;
        private const int PSF_TYPE_VAL = 4;
        public static Dictionary<string, object> ReadSfo(Stream sfo)
        {
            var sfoValues = new Dictionary<string, object>();

            // Read Sfo Header
            var magic = DataUtils.ReadUInt32(sfo);
            var version = DataUtils.ReadUInt32(sfo);
            var keyOffset = DataUtils.ReadUInt32(sfo);
            var valueOffset = DataUtils.ReadUInt32(sfo);
            var count = DataUtils.ReadUInt32(sfo);

            if (magic == 0x46535000) //\x00PSF
            {
                for(int i = 0; i < count; i++)
                {
                    var nameOffset = DataUtils.ReadUInt16(sfo);
                    var alignment = (byte)sfo.ReadByte();
                    var type = (byte)sfo.ReadByte();
                    var valueSize = DataUtils.ReadUInt32(sfo);
                    var totalSize = DataUtils.ReadUInt32(sfo);
                    var dataOffset = DataUtils.ReadUInt32(sfo);

                    var keyLocation = Convert.ToInt32(keyOffset + nameOffset);
                    var keyName = DataUtils.ReadStringAt(sfo, keyLocation);
                    var valueLocation = Convert.ToInt32(valueOffset + dataOffset);
                    object value = "Unknown Type";


                    switch (type)
                    {
                        case PSF_TYPE_STR:
                            value = DataUtils.ReadStringAt(sfo, valueLocation);
                            break; 

                        case PSF_TYPE_VAL:
                            value = DataUtils.ReadUint32At(sfo, valueLocation + i);
                            break;

                        case PSF_TYPE_BIN:
                            value = DataUtils.ReadBytesAt(sfo,valueLocation + i, Convert.ToInt32(valueSize));
                            break;
                    }

                    sfoValues[keyName] = value;
                }

                return sfoValues;
            }

            throw new InvalidDataException("Sfo Magic is Invalid.");
        }
        public static Dictionary<string, object> ReadSfo(byte[] Sfo)
        {
            using MemoryStream sfoStream = new MemoryStream(Sfo);
            return ReadSfo(sfoStream);
        }
    }
}
