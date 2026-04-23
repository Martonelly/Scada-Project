using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read coil functions/requests.
    /// </summary>
    public class ReadCoilsFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadCoilsFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
		public ReadCoilsFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc/>
        public override byte[] PackRequest()
        {
            byte[] ret_val = new byte[12];

            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.NetworkToHostOrder((short)CommandParameters.TransactionId)), 0, ret_val, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.NetworkToHostOrder((short)CommandParameters.ProtocolId)), 0, ret_val, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.NetworkToHostOrder((short)CommandParameters.Length)), 0, ret_val, 4, 2);

            ret_val[6] = CommandParameters.UnitId;
            ret_val[7] = CommandParameters.FunctionCode;

            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.NetworkToHostOrder((short)((ModbusReadCommandParameters)CommandParameters).StartAddress)), 0, ret_val, 8, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.NetworkToHostOrder((short)((ModbusReadCommandParameters)CommandParameters).Quantity)), 0, ret_val, 10, 2);

            return ret_val;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            Dictionary<Tuple<PointType, ushort>, ushort> r = new Dictionary<Tuple<PointType, ushort>, ushort>();
            //Check for exception
            if (response[7] != 0x80 + CommandParameters.FunctionCode)
            {
                //Starting address
                //var address = BitConverter.ToInt16(response, 8);
                var address = ((ModbusReadCommandParameters)CommandParameters).StartAddress;
                var count = 0;
                //Go for the whole byteCount
                for (int i = 0; i < response[8]; i++) 
                {
                    //Need to get the current byte
                    byte temp = response[9 + i];
                    //Each byte wiht 8 bits
                    for (int j = 0; j < 8; j++) 
                    {
                        //Value needs to be one or zero
                        ushort value = (ushort)(temp & 0x00000001);
                        r.Add(new Tuple<PointType, ushort>(PointType.DIGITAL_OUTPUT, (ushort)address), value);
                        //Reset for the next bit
                        temp >>= 1;
                        address++;
                        count++;
                        ushort quantity = ((ModbusReadCommandParameters)CommandParameters).Quantity;
                        //If you reach the end of the last coil
                        if (quantity >= count) {
                            break;
                        }
                    }
                }
            }
            else {
                HandeException(response[8]);
            }

            return r;
        }
    }
}