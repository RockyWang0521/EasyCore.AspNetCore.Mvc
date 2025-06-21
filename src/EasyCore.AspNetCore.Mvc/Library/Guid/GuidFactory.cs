using System.Security.Cryptography;

namespace EasyCore.AspNetCore.Mvc.AppService
{
    internal class GuidFactory : IGuidFactory
    {
        private static readonly RandomNumberGenerator RandomNumberGenerator = RandomNumberGenerator.Create();

        public Guid NewGuid => Create();

        public Guid Create()
        {
            return Create(SequentialGuidType.SequentialAsBinary);
        }

        public Guid Create(SequentialGuidType guidType)
        {
            var randomBytes = new byte[10];
            RandomNumberGenerator.GetBytes(randomBytes);

            long timestamp = DateTime.UtcNow.Ticks / 10000L;

            byte[] timestampBytes = BitConverter.GetBytes(timestamp);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(timestampBytes);
            }

            byte[] guidBytes = new byte[16];

            switch (guidType)
            {
                case SequentialGuidType.SequentialAsString:

                case SequentialGuidType.SequentialAsBinary:

                    Buffer.BlockCopy(timestampBytes, 2, guidBytes, 0, 6);

                    Buffer.BlockCopy(randomBytes, 0, guidBytes, 6, 10);

                    if (guidType == SequentialGuidType.SequentialAsString && BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(guidBytes, 0, 4);

                        Array.Reverse(guidBytes, 4, 2);
                    }

                    break;

                case SequentialGuidType.SequentialAtEnd:

                    Buffer.BlockCopy(randomBytes, 0, guidBytes, 0, 10);

                    Buffer.BlockCopy(timestampBytes, 2, guidBytes, 10, 6);
                    break;
            }

            return new Guid(guidBytes);
        }

        public enum SequentialGuidType
        {
            /// <summary>
            /// The GUID should be sequential when formatted using the <see cref="Guid.ToString()" /> method.
            /// Used by MySql and PostgreSql.
            /// </summary>
            SequentialAsString,

            /// <summary>
            /// The GUID should be sequential when formatted using the <see cref="Guid.ToByteArray" /> method.
            /// Used by Oracle.
            /// </summary>
            SequentialAsBinary,

            /// <summary>
            /// The sequential portion of the GUID should be located at the end of the Data4 block.
            /// Used by SqlServer.
            /// </summary>
            SequentialAtEnd
        }
    }
}
