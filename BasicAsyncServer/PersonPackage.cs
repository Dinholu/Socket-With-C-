using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace BasicAsyncServer
{
    class PersonPackage
    {
        public bool IsMale { get; set; }
        public ushort Age { get; set; }
        public string Name { get; set; }

        public string Message { get; set; }
        public string ImageBase64 { get; set; }
        public Int32 ImageSize { get; set; }



        public PersonPackage(bool male, ushort age, string name, string message, string imagebase64 )
        {
            IsMale = male;
            Age = age;
            Name = name;
            Message = message;
            ImageSize = imagebase64.Length;
            ImageBase64 = imagebase64;
        }

        public PersonPackage(byte[] data)
        {
            IsMale = BitConverter.ToBoolean(data, 0);
            Age = BitConverter.ToUInt16(data, 1);


            int nameLength = BitConverter.ToInt32(data, 3);
            Name = Encoding.Unicode.GetString(data, 7, nameLength);

            int messageLength = BitConverter.ToInt32(data, 7 + nameLength);
            Message = Encoding.Unicode.GetString(data, 11 + nameLength, messageLength);

            ImageSize = BitConverter.ToInt32(data, 11 + nameLength + messageLength);
            
            // récupérer imagebase64
            ImageBase64 = Encoding.Unicode.GetString(data, 15 + nameLength + messageLength, data.Length - (15 + nameLength + messageLength));



        }

        /// <summary>
        ///  Serializes this package to a byte array.
        /// </summary>
        /// <remarks>
        /// Use the <see cref="Buffer"/> or <see cref="Array"/> class for better performance.
        /// </remarks>
        public byte[] ToByteArray()
        {
            List<byte> byteList = new List<byte>();
            byteList.AddRange(BitConverter.GetBytes(IsMale));
            byteList.AddRange(BitConverter.GetBytes(Age));
            byteList.AddRange(BitConverter.GetBytes(Encoding.Unicode.GetByteCount(Name))); // Obtenir la longueur en octets de Name
            byteList.AddRange(Encoding.Unicode.GetBytes(Name));
            byteList.AddRange(BitConverter.GetBytes(Encoding.Unicode.GetByteCount(Message))); // Obtenir la longueur en octets de Message
            byteList.AddRange(Encoding.Unicode.GetBytes(Message));
            byteList.AddRange(BitConverter.GetBytes(ImageSize));
            byteList.AddRange(Encoding.Unicode.GetBytes(ImageBase64));
            return byteList.ToArray();
        }
    }
}
