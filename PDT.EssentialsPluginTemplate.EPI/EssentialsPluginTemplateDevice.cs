// For Basic SIMPL# Classes
// For Basic SIMPL#Pro classes

using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;

namespace EssentialsPluginTemplate
{
	/// <summary>
	/// Plugin device template for third party devices that use IBasicCommunication
	/// </summary>
	/// <remarks>
	/// Rename the class to match the device plugin being developed.
	/// </remarks>
	/// <example>
	/// "EssentialsPluginDeviceTemplate" renamed to "SamsungMdcDevice"
	/// </example>
	public class EssentialsPluginTemplateDevice : EssentialsBridgeableDevice
    {
        /// <summary>
        /// It is often desirable to store the config
        /// </summary>
        private EssentialsPluginConfigObjectTemplate _config;

        #region IBasicCommunication Properties and Constructor.  Remove if not needed.

        // TODO [ ] Add, modify, remove properties and fields as needed for the plugin being developed
		private readonly IBasicCommunication _comms;
		private readonly GenericCommunicationMonitor _commsMonitor;

		// _comms gather for ASCII based API's
		// TODO [ ] If not using an ASCII based API, delete the properties below
		private readonly CommunicationGather _commsGather;

        /// <summary>
        /// Set this value to that of the delimiter used by the API (if applicable)
        /// </summary>
		private const string CommsDelimiter = "\r";

		// _comms byte buffer for HEX/byte based API's
		// TODO [ ] If not using an HEX/byte based API, delete the properties below
		private byte[] _commsByteBuffer = { };



		/// <summary>
		/// Connects/disconnects the comms of the plugin device
		/// </summary>
		/// <remarks>
		/// triggers the _comms.Connect/Disconnect as well as thee comms monitor start/stop
		/// </remarks>
		public bool Connect
		{
			get { return _comms.IsConnected; }
			set
			{
				if (value)
				{
					_comms.Connect();
					_commsMonitor.Start();
				}
				else
				{
					_comms.Disconnect();
					_commsMonitor.Stop();
				}
			}
		}

		/// <summary>
		/// Reports connect feedback through the bridge
		/// </summary>
		public BoolFeedback ConnectFeedback { get; private set; }

		/// <summary>
		/// Reports online feedback through the bridge
		/// </summary>
		public BoolFeedback OnlineFeedback { get; private set; }

		/// <summary>
		/// Reports socket status feedback through the bridge
		/// </summary>
		public IntFeedback StatusFeedback { get; private set; }

		/// <summary>
		/// Plugin device constructor for devices that need IBasicCommunication
		/// </summary>
		/// <param name="key"></param>
		/// <param name="name"></param>
		/// <param name="config"></param>
		/// <param name="comms"></param>
		public EssentialsPluginTemplateDevice(string key, string name, EssentialsPluginConfigObjectTemplate config, IBasicCommunication comms)
			: base(key, name)
		{
			Debug.Console(0, this, "Constructing new {0} instance", name);

			// TODO [ ] Update the constructor as needed for the plugin device being developed

			_config = config;

			ConnectFeedback = new BoolFeedback(() => Connect);
			OnlineFeedback = new BoolFeedback(() => _commsMonitor.IsOnline);
			StatusFeedback = new IntFeedback(() => (int)_commsMonitor.Status);

			_comms = comms;
			_commsMonitor = new GenericCommunicationMonitor(this, _comms, _config.PollTimeMs, _config.WarningTimeoutMs, _config.ErrorTimeoutMs, Poll);

			var socket = _comms as ISocketStatus;
			if (socket != null)
			{
				// device comms is IP **ELSE** device comms is RS232
				socket.ConnectionChange += socket_ConnectionChange;
				Connect = true;
            }

            #region Communication data event handlers.  Comment out any that don't apply to the API type

            // Only one of the below handlers should be necessary.  

            // _comms gather for any API that has a defined delimiter
			// TODO [ ] If not using an ASCII based API, remove the line below
			_commsGather = new CommunicationGather(_comms, CommsDelimiter);
			_commsGather.LineReceived += Handle_LineRecieved;

			// _comms byte buffer for HEX/byte based API's with no delimiter
            // TODO [ ] If not using an HEX/byte based API, remove the line below
			_comms.BytesReceived += Handle_BytesReceived;

            // _comms byte buffer for HEX/byte based API's with no delimiter
            // TODO [ ] If not using an HEX/byte based API, remove the line below
            _comms.TextReceived += Handle_TextReceived;

            #endregion
        }


		private void socket_ConnectionChange(object sender, GenericSocketStatusChageEventArgs args)
		{
			if (ConnectFeedback != null)
				ConnectFeedback.FireUpdate();

			if (StatusFeedback != null)
				StatusFeedback.FireUpdate();
		}

		// TODO [ ] If not using an API with a delimeter, delete the method below
		private void Handle_LineRecieved(object sender, GenericCommMethodReceiveTextArgs args)
		{
			// TODO [ ] Implement method 
			throw new System.NotImplementedException();
		}

        // TODO [ ] If not using an HEX/byte based API with no delimeter,  delete the method below
		private void Handle_BytesReceived(object sender, GenericCommMethodReceiveBytesArgs args)
		{
			// TODO [ ] Implement method 
			throw new System.NotImplementedException();
		}

        // TODO [ ] If not using an ASCII based API with no delimeter, delete the method below
        void Handle_TextReceived(object sender, GenericCommMethodReceiveTextArgs e)
        {
            // TODO [ ] Implement method 
            throw new System.NotImplementedException();
        }


		// TODO [ ] If not using an ACII based API, delete the properties below
		/// <summary>
		/// Sends text to the device plugin comms
		/// </summary>
		/// <remarks>
		/// Can be used to test commands with the device plugin using the DEVPROPS and DEVJSON console commands
		/// </remarks>
		/// <param name="text">Command to be sent</param>		
		public void SendText(string text)
		{
			if (string.IsNullOrEmpty(text)) return;

			_comms.SendText(string.Format("{0}{1}", text, CommsDelimiter));
		}

		// TODO [ ] If not using an HEX/byte based API, delete the properties below
		/// <summary>
		/// Sends bytes to the device plugin comms
		/// </summary>
		/// <remarks>
		/// Can be used to test commands with the device plugin using the DEVPROPS and DEVJSON console commands
		/// </remarks>
		/// <param name="bytes">Bytes to be sent</param>		
		public void SendBytes(byte[] bytes)
		{
			if (bytes == null) return;

			_comms.SendBytes(bytes);
		}

		/// <summary>
		/// Polls the device
		/// </summary>
		/// <remarks>
		/// Poll method is used by the communication monitor.  Update the poll method as needed for the plugin being developed
		/// </remarks>
		public void Poll()
		{
			// TODO [ ] Update Poll method as needed for the plugin being developed
            // Example: SendText("getstatus");
			throw new System.NotImplementedException();
        }

        #endregion


        #region Overrides of EssentialsBridgeableDevice

        /// <summary>
        /// Links the plugin device to the EISC bridge
        /// </summary>
        /// <param name="trilist"></param>
        /// <param name="joinStart"></param>
        /// <param name="joinMapKey"></param>
        /// <param name="bridge"></param>
        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new EssentialsPluginBridgeJoinMapTemplate(joinStart);

            // This adds the join map to the collection on the bridge
            if (bridge != null)
            {
                bridge.AddJoinMap(Key, joinMap);
            }

            var customJoins = JoinMapHelper.TryGetJoinMapAdvancedForDevice(joinMapKey);

            if (customJoins != null)
            {
                joinMap.SetCustomJoinData(customJoins);
            }

            Debug.Console(1, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
            Debug.Console(0, "Linking to Bridge Type {0}", GetType().Name);

            // TODO [ ] Implement bridge links as needed

            // links to bridge
            trilist.SetString(joinMap.DeviceName.JoinNumber, Name);

            trilist.SetBoolSigAction(joinMap.Connect.JoinNumber, sig => Connect = sig);
            ConnectFeedback.LinkInputSig(trilist.BooleanInput[joinMap.Connect.JoinNumber]);

            StatusFeedback.LinkInputSig(trilist.UShortInput[joinMap.Status.JoinNumber]);
            OnlineFeedback.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline.JoinNumber]);

            UpdateFeedbacks();

            trilist.OnlineStatusChange += (o, a) =>
            {
                if (!a.DeviceOnLine) return;

                trilist.SetString(joinMap.DeviceName.JoinNumber, Name);
                UpdateFeedbacks();
            };
        }

        private void UpdateFeedbacks()
        {
            // TODO [ ] Update as needed for the plugin being developed
            ConnectFeedback.FireUpdate();
            OnlineFeedback.FireUpdate();
            StatusFeedback.FireUpdate();
        }

        #endregion

    }
}

