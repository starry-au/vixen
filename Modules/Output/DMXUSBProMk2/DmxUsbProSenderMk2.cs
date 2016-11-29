namespace VixenModules.Output.DmxUsbProMk2
{
	using System;
	using System.IO.Ports;
	using Vixen.Commands;

	internal class DmxUsbProSenderMk2 : IDisposable
	{
		private static NLog.Logger Logging = NLog.LogManager.GetCurrentClassLogger(); 
		
		private readonly Message _dmxPacketMessage;     // DMX Port 1
		private readonly byte[] _statePacket;

        private readonly Message _dmxPacketMessage2;    // DMX Port 2
        private readonly byte[] _statePacket2;

        private SerialPort _serialPort;

		public DmxUsbProSenderMk2(SerialPort serialPort)
		{
			this._serialPort = serialPort;
			this._statePacket = new byte[513];
			this._dmxPacketMessage = new Message(MessageType.OutputOnlySendDMXPacketRequest)
			                         	{
			                         		Data = this._statePacket
			                         	};
            this._statePacket2 = new byte[513];
            this._dmxPacketMessage2 = new Message(MessageType.OutputOnlySendDMXPacketRequestPort2)
            {
                Data = this._statePacket2
            };
        }

		~DmxUsbProSenderMk2()
		{
			this.Dispose();
		}

        public void SendDmxPacket(ICommand[] outputStates)
        {
            if (outputStates == null || this._statePacket == null || _serialPort == null) {
                return;
            }

            var channelValues = new byte[outputStates.Length];
            for (int index = 0; index < outputStates.Length; index++) {
                _8BitCommand command = outputStates[index] as _8BitCommand;
                if (command == null) {
                    // State reset
                    channelValues[index] = 0;
                    continue;
                }

                channelValues[index] = command.CommandValue;
            }
            if (!this._serialPort.IsOpen) {
				//this._serialPort.Open();
				return;
			}

			this._statePacket[0] = 0; // Start code
			Array.Copy(channelValues, 0, this._statePacket, 1, Math.Min(512, channelValues.Length));
			byte[] packet = this._dmxPacketMessage.Packet;
			if (packet != null)
            {
				this._serialPort.Write(packet, 0, packet.Length);
                //Logging.Info("port1");
            }
            if (outputStates.Length > 512)
            {

                this._statePacket2[0] = 0; // Start code
                Array.Copy(channelValues, 512, this._statePacket2, 1, Math.Min(512, (channelValues.Length - 512)));
                byte[] packet2 = this._dmxPacketMessage2.Packet;
                if (packet2 != null)
                {
              //      Logging.Info("port2");
                    this._serialPort.Write(packet2, 0, packet2.Length);
                }
            }
        }

		public void Start()
		{
			try {
				if (_serialPort != null && !this._serialPort.IsOpen) {
					this._serialPort.Open();
                }
			}
			catch (Exception ex) {
				Logging.Error(string.Format("Serial Port Open failed"), ex);
			}
            try
            {
                EnableAPIMk2();
            }
            catch (Exception ex)
            {
                Logging.Error(string.Format("DMX PRO Mk2 Port 2 Init failed"), ex);
            }
        }
        private void EnableAPIMk2()
        {
            /******************** PRO MK2 LABELS: ASSIGN AS PER YOUR API (request the pdf if you don't have one) *********************/
            // THE API Key is LSB First: so if it says 11223344 .. define it as ... 44,33,22,11
            // 0004a5ca
            byte[] Mk2Packet = { 0xca, 0xa5, 0x04, 0x00 };
            Message Mk2PacketMessage = new Message(MessageType.SetAPIKey)
            {
                Data = Mk2Packet
            };
            byte[] PortPacket = { 1, 1 };
            Message PortPacketMessage = new Message(MessageType.SetPortAssignment)
            {
                Data = PortPacket
            };

            byte[] packet1 = Mk2PacketMessage.Packet;
            if (packet1 != null)
            {
                this._serialPort.Write(packet1, 0, packet1.Length);
            }
            Logging.Info("PRO Mk2 ... Activated ...");


            byte[] packet2 = PortPacketMessage.Packet;
            if (packet2 != null)
            {
                this._serialPort.Write(packet2, 0, packet2.Length);
            }
            Logging.Info("PRO Mk2 ... Ready for DMX on both ports ... ");
        }

		public void Stop()
		{
			if (_serialPort != null && this._serialPort.IsOpen) {
				this._serialPort.Close();
			}
		}

		public void Dispose()
		{
			if (this._serialPort != null && this._serialPort.IsOpen) {
				this._serialPort.Close();
			}
			if (this._serialPort != null) {
				this._serialPort.Dispose();
				this._serialPort = null;
			}

			GC.SuppressFinalize(this);
		}
	}
}