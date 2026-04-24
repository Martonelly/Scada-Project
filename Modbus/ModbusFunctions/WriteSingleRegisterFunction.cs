using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus write single register functions/requests.
    /// </summary>
    public class WriteSingleRegisterFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WriteSingleRegisterFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public WriteSingleRegisterFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusWriteCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            byte[] ret_val = new byte[12];

            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.NetworkToHostOrder((short)CommandParameters.TransactionId)), 0, ret_val, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.NetworkToHostOrder((short)CommandParameters.ProtocolId)), 0, ret_val, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.NetworkToHostOrder((short)CommandParameters.Length)), 0, ret_val, 4, 2);

            ret_val[6] = CommandParameters.UnitId;
            ret_val[7] = CommandParameters.FunctionCode;

            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.NetworkToHostOrder((short)((ModbusWriteCommandParameters)CommandParameters).OutputAddress)), 0, ret_val, 8, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.NetworkToHostOrder((short)((ModbusWriteCommandParameters)CommandParameters).Value)), 0, ret_val, 10, 2);
            return ret_val;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            Dictionary<Tuple<PointType, ushort>, ushort> r = new Dictionary<Tuple<PointType, ushort>, ushort>();
            //Check the id 0x86 error indicator
            if (response[7] != CommandParameters.FunctionCode + 0x80)
            {
                var address = BitConverter.ToUInt16(response, 8);
                var value = BitConverter.ToUInt16(response, 10);

                address = (ushort)IPAddress.NetworkToHostOrder((short)address);
                value = (ushort)IPAddress.NetworkToHostOrder((short)value);

                r.Add(new Tuple<PointType, ushort>(PointType.ANALOG_OUTPUT, address), value);
            }
            else
            {
                HandeException(response[8]);
            }
            return r;
        }
    }
}