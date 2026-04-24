using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read holding registers functions/requests.
    /// </summary>
    public class ReadHoldingRegistersFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadHoldingRegistersFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public ReadHoldingRegistersFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
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

            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.NetworkToHostOrder((short)((ModbusReadCommandParameters)CommandParameters).StartAddress)), 0, ret_val, 8, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.NetworkToHostOrder((short)((ModbusReadCommandParameters)CommandParameters).Quantity)), 0, ret_val, 10, 2);

            return ret_val;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            Dictionary<Tuple<PointType, ushort>, ushort> r = new Dictionary<Tuple<PointType, ushort>, ushort>();

            if (response[7] != CommandParameters.FunctionCode + 0x80)
            {
                var address = ((ModbusReadCommandParameters)CommandParameters).StartAddress;
                int count = 0;

                for (int i = 0; i < response[8]; i+=2)
                {
                    ushort value = (ushort)BitConverter.ToInt16(response, 9 + i);
                    value = (ushort)IPAddress.NetworkToHostOrder((short)value);

                    r.Add(new Tuple<PointType, ushort>(PointType.ANALOG_OUTPUT, (ushort)address), (ushort)value);

                    count++;
                    address++;
                    ushort quantity = ((ModbusReadCommandParameters)CommandParameters).Quantity;
                    if (quantity <= count)
                    {
                        break;
                    }
                    
                }
            }
            else
            {
                HandeException(response[8]);
            }

            return r;
        }
    }
}