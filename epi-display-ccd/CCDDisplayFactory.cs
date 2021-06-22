using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.UI;

// RAD
using System;
using System.Linq;
using Crestron.SimplSharp.Reflection;
using Crestron.SimplSharpPro.DM;
using Crestron.RAD.Common.BasicDriver;
using Crestron.RAD.Common.Enums;
using Crestron.RAD.Common.Interfaces;
using Crestron.RAD.Common.Transports;
using Crestron.RAD.ProTransports;

namespace CCDDisplay
{
	/// <summary>
	/// Plugin device factory for CCD/RAD IBasicVideoDisplay devices
	/// </summary>
    public class CCDDisplayFactory : EssentialsPluginDeviceFactory<CCDDisplayDevice>
    {
		/// <summary>
		/// CCDDisplay EPI Plugin device factory constructor
		/// </summary>
        public CCDDisplayFactory()
        {
            // Set the minimum Essentials Framework Version
			MinimumEssentialsFrameworkVersion = "1.6.9";

            // In the constructor we initialize the list with the typenames that will build an instance of this device
            TypeNames = new List<string>() { "ccddisplay" };
        }
        
		/// <summary>
		/// Builds and returns an instance of CCDDisplayDevice
		/// </summary>
		/// <param name="dc">device configuration</param>
		/// <returns>plugin device or null</returns>
		/// <remarks>		
		/// The example provided below takes the device key, name, properties config and the comms device created.
		/// Modify the EssetnialsPlugingDeviceTemplate constructor as needed to meet the requirements of the plugin device.
		/// </remarks>
		/// <seealso cref="PepperDash.Core.eControlMethod"/>
        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "[{0}] Factory Attempting to create new device from type: {1}", dc.Key, dc.Type);

            // get the plugin device properties configuration object & check for null 
            var propertiesConfig = dc.Properties.ToObject<CCDDisplayConfig>();
            if (propertiesConfig == null)
            {
                Debug.Console(0, "[{0}] Factory: failed to read properties config for {1}", dc.Key, dc.Name);
                return null;
            }

            CType transportType = null;

            switch (propertiesConfig.Transport)
            {
                case "ITcp":
                    transportType = typeof(ITcp);
                    break;
                case "ISerialComport":
                    transportType = typeof(ISerialComport);
                    break;
                case "ICecDevice":
                    transportType = typeof(ICecDevice);
                    break;
                //case "ICrestronConnected":
                //    transportType = typeof(ICrestronConnected);
                //    break;
                //case "IIr":
                //    transportType = typeof(IIr);
                //    break;
                default:
                    Debug.Console(0, "[{0}] Factory: transport `{3}` is not supported by current implementation of plugin {1}", dc.Key, dc.Name, propertiesConfig.Transport);
                    return null;
            }

            // Load and instantiate driver from DLL assembly
            IBasicVideoDisplay _radDevice = null;

            if (File.Exists(propertiesConfig.Driver))
            {
                try
                {
                    var type = Assembly.LoadFrom(propertiesConfig.Driver).GetTypes()
                                .FirstOrDefault(t => typeof(IBasicVideoDisplay).IsAssignableFrom(t) && transportType.IsAssignableFrom(t));
                    _radDevice = (IBasicVideoDisplay)Crestron.SimplSharp.Reflection.Activator.CreateInstance(type);
                }
                catch (Exception)
                {
                    Debug.Console(0, "[{0}] Factory: loading driver from `{3}` failed for device {1}", dc.Key, dc.Name, propertiesConfig.Driver);
                    return null;
                }
            }

            // Initialize transport
            IBasicCommunication comm = null;
            switch (propertiesConfig.Transport)
            {
                case "ITcp":
                    // If Port supplied in Control parameters, use it, otherwise use default Driver Port
                    int port = (propertiesConfig.Control.TcpSshProperties.Port != 0)? propertiesConfig.Control.TcpSshProperties.Port: ((ITcp)_radDevice).Port;
                    ((ITcp)_radDevice).Initialize(IPAddress.Parse(propertiesConfig.Control.TcpSshProperties.Address), port);
                    break;
                case "ISerialComport":
                    //comm = CommFactory.CreateCommForDevice(dc);
                    ComPort comPort = CommFactory.GetComPort(CommFactory.GetControlPropertiesConfig(dc));
		    
		    if (comPort.Parent is CrestronControlSystem)
            	    {
                        var result = Port.Register();
                        if (result != eDeviceRegistrationUnRegistrationResponse.Success)
                        {
                            Debug.Console(0, this, "ERROR: Cannot register Com port: {0}", result);
                            return; // false
                        }
                    }
		    
                    var serialTransport = new SerialTransport(comPort);
                    var serialDriver = _radDevice as ISerialComport;
                    if (serialDriver != null)
                    {
                        if (dc.Properties.SelectToken("control.comParams", false) != null)
                        {
                            /// control.comParams object supplied in configuration, using defined there ComParams
                            Debug.Console(0, "[{0}] Factory: loading ComParams from config for device {1}", dc.Key, dc.Name);
                            ComPort.ComPortSpec configComSpec = CommFactory.GetControlPropertiesConfig(dc).ComParams;

                            /// TODO: find better way - Crestron stupidity - CCD/RAD ComPortSpec is not the same as Crestron.SimpleSharpPro.ComPort.ComPortSpec
                            serialTransport.SetComPortSpec(TranslateComPortSpec(configComSpec));

                        }
                        else
                        {
                            /// Driver's default ComSpecs
                            Debug.Console(0, "[{0}] Factory: loading default ComParams from driver for device {1}", dc.Key, dc.Name);
                            serialTransport.SetComPortSpec(serialDriver.ComSpec);
                        }

                        // Initialize the transport
                        serialDriver.Initialize(serialTransport);
                    }
                    break;
                case "ICecDevice":
                    Cec cec = CommFactory.GetCecPort(CommFactory.GetControlPropertiesConfig(dc)) as Cec;
                    if (cec == null)
                    {
                        Debug.Console(0, "[{0}] Factory: Cec transport can't be constructed from `{3}` failed for device {1}", dc.Key, dc.Name, CommFactory.GetControlPropertiesConfig(dc).ToString());
                        return null;
                    }
                    var cecTransport = new CecTransport();
                    cecTransport.Initialize(cec);
                    cecTransport.Start();
                    var cecDriver = _radDevice as ICecDevice;
                    if (cecDriver != null)
                    {
                        // Initialize the transport
                        cecDriver.Initialize(cecTransport);
                    }
                    break;
            }

            return new CCDDisplayDevice(dc.Key, dc.Name, propertiesConfig, _radDevice, comm);
        }



        private static Crestron.RAD.Common.Transports.ComPortSpec TranslateComPortSpec(Crestron.SimplSharpPro.ComPort.ComPortSpec comSpec)
        {
            Crestron.RAD.Common.Transports.ComPortSpec radComSpec = new Crestron.RAD.Common.Transports.ComPortSpec()
            {
                BaudRate = eComBaudRates.NotSpecified,
                DataBits = eComDataBits.NotSpecified,
                HardwareHandShake = eComHardwareHandshakeType.NotSpecified,
                Parity = eComParityType.NotSpecified,
                Protocol = eComProtocolType.NotSpecified,
                SoftwareHandshake = eComSoftwareHandshakeType.NotSpecified,
                StopBits = eComStopBits.NotSpecified,
                ReportCTSChanges = comSpec.ReportCTSChanges
            };

            switch (comSpec.BaudRate)
            {
                case ComPort.eComBaudRates.ComspecBaudRate300:
                    radComSpec.BaudRate = eComBaudRates.ComspecBaudRate300;
                    break;
                case ComPort.eComBaudRates.ComspecBaudRate600:
                    radComSpec.BaudRate = eComBaudRates.ComspecBaudRate600;
                    break;
                case ComPort.eComBaudRates.ComspecBaudRate1200:
                    radComSpec.BaudRate = eComBaudRates.ComspecBaudRate1200;
                    break;
                case ComPort.eComBaudRates.ComspecBaudRate1800:
                    radComSpec.BaudRate = eComBaudRates.ComspecBaudRate1800;
                    break;
                case ComPort.eComBaudRates.ComspecBaudRate2400:
                    radComSpec.BaudRate = eComBaudRates.ComspecBaudRate2400;
                    break;
                case ComPort.eComBaudRates.ComspecBaudRate3600:
                    radComSpec.BaudRate = eComBaudRates.ComspecBaudRate3600;
                    break;
                case ComPort.eComBaudRates.ComspecBaudRate7200:
                    radComSpec.BaudRate = eComBaudRates.ComspecBaudRate7200;
                    break;
                case ComPort.eComBaudRates.ComspecBaudRate9600:
                    radComSpec.BaudRate = eComBaudRates.ComspecBaudRate9600;
                    break;
                case ComPort.eComBaudRates.ComspecBaudRate14400:
                    radComSpec.BaudRate = eComBaudRates.ComspecBaudRate14400;
                    break;
                case ComPort.eComBaudRates.ComspecBaudRate19200:
                    radComSpec.BaudRate = eComBaudRates.ComspecBaudRate19200;
                    break;
                case ComPort.eComBaudRates.ComspecBaudRate28800:
                    radComSpec.BaudRate = eComBaudRates.ComspecBaudRate28800;
                    break;
                case ComPort.eComBaudRates.ComspecBaudRate38400:
                    radComSpec.BaudRate = eComBaudRates.ComspecBaudRate38400;
                    break;
                case ComPort.eComBaudRates.ComspecBaudRate57600:
                    radComSpec.BaudRate = eComBaudRates.ComspecBaudRate57600;
                    break;
                case ComPort.eComBaudRates.ComspecBaudRate115200:
                    radComSpec.BaudRate = eComBaudRates.ComspecBaudRate115200;
                    break;
            }

            switch (comSpec.DataBits)
            {
                case ComPort.eComDataBits.ComspecDataBits7:
                    radComSpec.DataBits = eComDataBits.ComspecDataBits7;
                    break;
                case ComPort.eComDataBits.ComspecDataBits8:
                    radComSpec.DataBits = eComDataBits.ComspecDataBits8;
                    break;
            }

            switch (comSpec.HardwareHandShake)
            {
                case ComPort.eComHardwareHandshakeType.ComspecHardwareHandshakeCTS:
                    radComSpec.HardwareHandShake = eComHardwareHandshakeType.ComspecHardwareHandshakeCTS;
                    break;
                case ComPort.eComHardwareHandshakeType.ComspecHardwareHandshakeNone:
                    radComSpec.HardwareHandShake = eComHardwareHandshakeType.ComspecHardwareHandshakeNone;
                    break;
                case ComPort.eComHardwareHandshakeType.ComspecHardwareHandshakeRTS:
                    radComSpec.HardwareHandShake = eComHardwareHandshakeType.ComspecHardwareHandshakeRTS;
                    break;
                case ComPort.eComHardwareHandshakeType.ComspecHardwareHandshakeRTSCTS:
                    radComSpec.HardwareHandShake = eComHardwareHandshakeType.ComspecHardwareHandshakeRTSCTS;
                    break;
            }

            switch (comSpec.Parity)
            {
                case ComPort.eComParityType.ComspecParityEven:
                    radComSpec.Parity = eComParityType.ComspecParityEven;
                    break;
                case ComPort.eComParityType.ComspecParityNone:
                    radComSpec.Parity = eComParityType.ComspecParityNone;
                    break;
                case ComPort.eComParityType.ComspecParityOdd:
                    radComSpec.Parity = eComParityType.ComspecParityOdd;
                    break;
                case ComPort.eComParityType.ComspecParityMark:
                    radComSpec.Parity = eComParityType.NotSpecified;
                    break;
            }

            switch (comSpec.Protocol)
            {
                case ComPort.eComProtocolType.ComspecProtocolRS232:
                    radComSpec.Protocol = eComProtocolType.ComspecProtocolRS232;
                    break;
                case ComPort.eComProtocolType.ComspecProtocolRS422:
                    radComSpec.Protocol = eComProtocolType.ComspecProtocolRS422;
                    break;
                case ComPort.eComProtocolType.ComspecProtocolRS485:
                    radComSpec.Protocol = eComProtocolType.ComspecProtocolRS485;
                    break;
            }

            switch (comSpec.SoftwareHandshake)
            {
                case ComPort.eComSoftwareHandshakeType.ComspecSoftwareHandshakeNone:
                    radComSpec.SoftwareHandshake = eComSoftwareHandshakeType.ComspecSoftwareHandshakeNone;
                    break;
                case ComPort.eComSoftwareHandshakeType.ComspecSoftwareHandshakeXON:
                    radComSpec.SoftwareHandshake = eComSoftwareHandshakeType.ComspecSoftwareHandshakeXON;
                    break;
                case ComPort.eComSoftwareHandshakeType.ComspecSoftwareHandshakeXONR:
                    radComSpec.SoftwareHandshake = eComSoftwareHandshakeType.ComspecSoftwareHandshakeXONR;
                    break;
                case ComPort.eComSoftwareHandshakeType.ComspecSoftwareHandshakeXONT:
                    radComSpec.SoftwareHandshake = eComSoftwareHandshakeType.ComspecSoftwareHandshakeXONT;
                    break;
            }

            switch (comSpec.StopBits)
            {
                case ComPort.eComStopBits.ComspecStopBits1:
                    radComSpec.StopBits = eComStopBits.ComspecStopBits1;
                    break;
                case ComPort.eComStopBits.ComspecStopBits2:
                    radComSpec.StopBits = eComStopBits.ComspecStopBits2;
                    break;
            }

            return radComSpec;
        }
    }
}

          
