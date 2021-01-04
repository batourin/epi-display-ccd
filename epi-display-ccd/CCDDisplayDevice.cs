using System;
using System.Linq;
using System.Text;
// For Basic SIMPL# Classes
// For Basic SIMPL#Pro classes
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;

// RAD
using Crestron.RAD.Common;
using Crestron.RAD.Common.Enums;
using Crestron.RAD.Common.Interfaces;

namespace CCDDisplay
{
	/// <summary>
	/// Plugin device template for third party devices that use IBasicCommunication
	/// </summary>
    [Description("Crestron Certified Drivers Display")]
    public class CCDDisplayDevice : TwoWayDisplayBase, IHasPowerControlWithFeedback, IWarmingCooling, IBasicVolumeWithFeedback, IRoutingNumericWithFeedback, ICommunicationMonitor, IBridgeAdvanced
    {
        /// <summary>
        /// It is often desirable to store the config
        /// </summary>
        private CCDDisplayConfig _config;

        private IBasicVideoDisplay _display;

        private Action<DisplayStateObjects, IBasicVideoDisplay, byte> _displayStateChangeAction;

        /// <summary>
        /// CCDDisplay Plugin device constructor for ISerialComport transport
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="config"></param>
        /// <param name="display">Loaded and initialized instance of CCD Display driver instance</param>
        public CCDDisplayDevice(string key, string name, CCDDisplayConfig config, IBasicVideoDisplay display)
            : base(key, name)
        {
            Debug.Console(0, this, "Constructing new {0} instance", name);

            // TODO [ ] Update the constructor as needed for the plugin device being developed

            _config = config;
            _display = display;

            // Set Display Id
            _display.Id = _config.Id;

            // TODO: Handle Essentials debug logic
            object debug = Debug.GetDeviceDebugSettingsForKey(key);
            if (debug != null && false)
            {
                _display.EnableLogging = true;
                _display.EnableRxDebug = true;
                _display.EnableTxDebug = true;
                //_display.EnableRxOut = true;
            }

            ConnectFeedback = new BoolFeedback(() => Connect);
            OnlineFeedback = new BoolFeedback(() => _display.Connected);
            StatusFeedback = new IntFeedback(() => (int)CommunicationMonitor.Status);

            VolumeLevelFeedback = new IntFeedback(() => { return (int)_display.VolumePercent; });
            MuteFeedback = new BoolFeedback(() => _display.Muted);

            Feedbacks.Add(VolumeLevelFeedback);
            Feedbacks.Add(MuteFeedback);

            if (_display.SupportsSetInputSource)
            {
                foreach (InputDetail input in _display.GetUsableInputs())
                {
                    eRoutingPortConnectionType connectionType = eRoutingPortConnectionType.None;
                    eRoutingSignalType signalType = eRoutingSignalType.AudioVideo;

                    // TODO: review mapping between Essentials PortConnection and Signal types and CCD types 
                    switch (input.InputConnector)
                    {
                        case VideoConnectionTypes.Hdmi:
                            connectionType = eRoutingPortConnectionType.Hdmi;
                            signalType = eRoutingSignalType.AudioVideo;
                            break;

                        case VideoConnectionTypes.Dvi:
                            connectionType = eRoutingPortConnectionType.Dvi;
                            signalType = eRoutingSignalType.Video;
                            break;

                        case VideoConnectionTypes.HdBaseT:
                            connectionType = eRoutingPortConnectionType.DmCat;
                            signalType = eRoutingSignalType.AudioVideo;
                            break;

                        case VideoConnectionTypes.Antenna:
                            connectionType = eRoutingPortConnectionType.BackplaneOnly;
                            signalType = eRoutingSignalType.AudioVideo;
                            break;

                        case VideoConnectionTypes.Component:
                            connectionType = eRoutingPortConnectionType.Component;
                            signalType = eRoutingSignalType.AudioVideo;
                            break;

                        case VideoConnectionTypes.Composite:
                            connectionType = eRoutingPortConnectionType.Composite;
                            signalType = eRoutingSignalType.AudioVideo;
                            break;

                        case VideoConnectionTypes.DisplayPort:
                            connectionType = eRoutingPortConnectionType.DisplayPort;
                            signalType = eRoutingSignalType.AudioVideo;
                            break;

                        case VideoConnectionTypes.GenericAV:
                            connectionType = eRoutingPortConnectionType.BackplaneOnly;
                            signalType = eRoutingSignalType.AudioVideo;
                            break;

                        case VideoConnectionTypes.GenericVideo:
                            connectionType = eRoutingPortConnectionType.BackplaneOnly;
                            signalType = eRoutingSignalType.Video;
                            break;

                        case VideoConnectionTypes.Network:
                            connectionType = eRoutingPortConnectionType.Streaming;
                            signalType = eRoutingSignalType.AudioVideo;
                            break;

                        case VideoConnectionTypes.Other:
                            connectionType = eRoutingPortConnectionType.BackplaneOnly;
                            signalType = eRoutingSignalType.AudioVideo;
                            break;

                        case VideoConnectionTypes.SVideo:
                            connectionType = eRoutingPortConnectionType.BackplaneOnly;
                            signalType = eRoutingSignalType.AudioVideo;
                            break;

                        case VideoConnectionTypes.Universal:
                            connectionType = eRoutingPortConnectionType.BackplaneOnly;
                            signalType = eRoutingSignalType.AudioVideo;
                            break;

                        case VideoConnectionTypes.Unknown:
                            connectionType = eRoutingPortConnectionType.BackplaneOnly;
                            signalType = eRoutingSignalType.AudioVideo;
                            break;

                        case VideoConnectionTypes.Usb:
                            connectionType = eRoutingPortConnectionType.DmCat;
                            signalType = eRoutingSignalType.UsbOutput;
                            break;

                        case VideoConnectionTypes.Vga:
                            connectionType = eRoutingPortConnectionType.Vga;
                            signalType = eRoutingSignalType.Video;
                            break;
                    }
                    var inputFix = input;
                    InputPorts.Add(new RoutingInputPort(input.InputType.ToString(), signalType, connectionType, new Action(() => _display.SetInputSource(inputFix.InputType)), this)
                    {
                        FeedbackMatchObject = inputFix.InputType
                    });
                }
            }

            // TODO: Figure out what is Outport for display device, do we need it?
            OutputPorts.Add(new RoutingOutputPort("Screen", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.BackplaneOnly, null, this));

            _displayStateChangeAction = new Action<DisplayStateObjects, IBasicVideoDisplay, byte>(displayStateChangeEvent);

            CommunicationMonitor = new CCDCommunicationMonitor(this, _display, 12000, 30000);

            CrestronConsole.AddNewConsoleCommand((s) => 
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Driver Information:");
                    sb.AppendFormat("\tBase Model:       {0}\r\n", _display.BaseModel);
                    sb.AppendFormat("\tDescription:      {0}\r\n", _display.Description);
                    sb.AppendFormat("\tDriver Version:   {0}\r\n", _display.DriverVersion);
                    sb.AppendFormat("\tGuid:             {0}\r\n", _display.Guid);
                    sb.AppendFormat("\tManufacturer:     {0}\r\n", _display.Manufacturer);
                    sb.AppendFormat("\tSupported Models:\r\n");
                    foreach (string model in _display.SupportedModels)
                        sb.AppendFormat("\t\t{0}\r\n", model);
                    sb.AppendFormat("\tSupportedSeries:\r\n");
                    foreach (string series in _display.SupportedSeries)
                        sb.AppendFormat("\t\t{0}\r\n", series);
                    sb.AppendFormat("\tVersionDate:      {0}\r\n", _display.VersionDate);
                    sb.AppendFormat("\tId:               {0}\r\n", _display.Id);

                    CrestronConsole.ConsoleCommandResponse("{0}", sb.ToString());
                },
                Key + "INFO", "Print Driver Info", ConsoleAccessLevelEnum.AccessOperator);

            CrestronConsole.AddNewConsoleCommand((s) =>
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Inputs:");
                    foreach (var input in _display.GetUsableInputs())
                    {
                        sb.AppendFormat("\t{0}: {1} on {2}({3})\r\n", input.Description, input.InputConnector, input.InputType, (int)input.InputType);
                    }
                    CrestronConsole.ConsoleCommandResponse(sb.ToString());
                },
                Key + "INPUTS", "Display Driver Inputs", ConsoleAccessLevelEnum.AccessOperator);

            CrestronConsole.AddNewConsoleCommand((s) =>
                {
                    if (_display.EnableLogging)
                    {
                        _display.EnableLogging = false;
                        _display.EnableRxDebug = false;
                        _display.EnableTxDebug = false;
                    }
                    else
                    {
                        _display.EnableLogging = true;
                        _display.EnableRxDebug = true;
                        _display.EnableTxDebug = true;
                    }
                },
                Key + "DEBUG", "Display Driver Inputs", ConsoleAccessLevelEnum.AccessOperator);

        }

        private void displayStateChangeEvent(DisplayStateObjects state, IBasicVideoDisplay display, byte arg3)
        {
            switch (state)
            {
                case DisplayStateObjects.Connection:
                    // Read current state of the projector power and if differ from room state update room state accordingly without setters actions.
                    if (display.Connected)
                    {
                    }

                    foreach (var feeedback in Feedbacks)
                        feeedback.FireUpdate();

                    break;

                case DisplayStateObjects.Power:
                    PowerIsOnFeedback.FireUpdate();
                    break;

                case DisplayStateObjects.PoweredOn:
                    PowerIsOnFeedback.FireUpdate();
                    break;

                case DisplayStateObjects.PoweredOff:
                    PowerIsOnFeedback.FireUpdate();

                    break;
                case DisplayStateObjects.CoolingDown:
                case DisplayStateObjects.CooledDown:
                    IsCoolingDownFeedback.FireUpdate();
                    break;

                case DisplayStateObjects.WarmingUp:
                case DisplayStateObjects.WarmedUp:
                    IsWarmingUpFeedback.FireUpdate();
                    break;

                case DisplayStateObjects.Mute:
                    MuteFeedback.FireUpdate();
                    break;

                case DisplayStateObjects.Volume:
                    VolumeLevelFeedback.FireUpdate();
                    break;

                case DisplayStateObjects.Input:
                    var localInputPort = InputPorts.FirstOrDefault( p => (VideoConnections)p.FeedbackMatchObject == _display.InputSource.InputType);
                    CurrentInputFeedback.FireUpdate();
                    // TODO: First argument can be fake OutputPort - OutputPort.First() which is "Screen"
                    OnSwitchChange(new RoutingNumericEventArgs(null, localInputPort, eRoutingSignalType.AudioVideo));
                    break;

                case DisplayStateObjects.LampHours:
                    break;

                case DisplayStateObjects.Audio:
                    break;

            
            }
        }

        /// <summary>
        /// Connects/disconnects the underlying RAD device
        /// </summary>
        /// <remarks>
        /// triggers the _comms.Connect/Disconnect as well as thee comms monitor start/stop
        /// </remarks>
        public bool Connect
        {
            get { return _display.Connected; }
            set
            {
                if (value)
                {
                    _display.Connect();
                }
                else
                {
                    if (_display.SupportsDisconnect)
                        _display.Disconnect();
                }
            }
        }

        public override bool CustomActivate()
        {
            if (!base.CustomActivate())
                return false;

            _display.StateChangeEvent += _displayStateChangeAction;
            Connect = true;
            CommunicationMonitor.Start();

            return true;
        }

        public override bool Deactivate()
        {
            if (!base.Deactivate())
                return false;

            CommunicationMonitor.Stop();
            Connect = false;
            _display.StateChangeEvent -= _displayStateChangeAction;

            return true;
        }

        #region TwoWayDisplayBase abstract class implementation

        protected override Func<bool> PowerIsOnFeedbackFunc
        {
            get { return () => _display.PowerIsOn; }
        }

        protected override Func<string> CurrentInputFeedbackFunc
        {
            get { return () => _display.InputSource.InputType.ToString(); }
        }

        public override void ExecuteSwitch(object selector)
        {
            throw new System.NotImplementedException();
        }

        // DisplayBase
        protected override Func<bool> IsCoolingDownFeedbackFunc
        {
            get { return () => _display.CoolingDown; }
        }

        protected override Func<bool> IsWarmingUpFeedbackFunc
        {
            get { return () => _display.WarmingUp; }
        }

        public override void PowerOff()
        {
            _display.PowerOff();
        }

        public override void PowerOn()
        {
            _display.PowerOn();
        }

        public override void PowerToggle()
        {
            _display.PowerToggle();
        }

        #endregion

        #region IBasicVolumeWithFeedback Members

        /// <summary>
        /// Provides feedback of current mute state
        /// </summary>
        public BoolFeedback MuteFeedback { get; private set; }

        /// <summary>
        /// Unmutes the display
        /// </summary>
        public void MuteOff()
        {
            _display.MuteOff();
        }

        /// <summary>
        /// Mutes the display
        /// </summary>
        public void MuteOn()
        {
            _display.MuteOn();
        }

        /// <summary>
        /// Provides feedback of current volume level
        /// </summary>
        public IntFeedback VolumeLevelFeedback { get; private set; }

        /// <summary>
        /// Set current volume level
        /// </summary>
        public void SetVolume(ushort level)
        {
            _display.SetVolume(level);
        }

        #endregion

        #region IBasicVolumeControls Members

        public void MuteToggle()
        {
            if (_display.Muted)
                MuteOn();
            else
                MuteOff();
        }

        public void VolumeDown(bool pressRelease)
        {
            _display.VolumeDown(Crestron.RAD.Common.Enums.CommandAction.Release);
        }

        public void VolumeUp(bool pressRelease)
        {
            _display.VolumeUp(Crestron.RAD.Common.Enums.CommandAction.Release);
        }

        #endregion

        #region ICommunicationMonitor Members

        public StatusMonitorBase CommunicationMonitor { get; private set;}

        #endregion

        #region IBridgeAdvanced Members

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
        /// Links the plugin device to the EISC bridge
        /// </summary>
        /// <param name="trilist"></param>
        /// <param name="joinStart"></param>
        /// <param name="joinMapKey"></param>
        /// <param name="bridge"></param>
        public void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            LinkDisplayToApi(this, trilist, joinStart, joinMapKey, bridge);

            var joinMap = new CCDDisplayBridgeJoinMap(joinStart);

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


        #region IRoutingNumeric Members

        public void ExecuteNumericSwitch(ushort input, ushort output, eRoutingSignalType type)
        {

            if (!Enum.IsDefined(typeof(VideoConnections), input))
            {
                Debug.Console(2, this, "Invalid Video Source Index : {0}", input);
                return;
            }
            _display.SetInputSource((VideoConnections) input);
        }

        #endregion

        #region IRouting Members

        public void ExecuteSwitch(object inputSelector, object outputSelector, eRoutingSignalType signalType)
        {
            Debug.Console(2, this, "Input Selector = {0}", inputSelector.ToString());
            var handler = inputSelector as Action;
            if (handler != null)
                handler();
        }

        #endregion

        #region IRoutingOutputs Members

        public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }

        #endregion
    }
}

