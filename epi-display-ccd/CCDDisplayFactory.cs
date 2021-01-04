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
            switch (propertiesConfig.Transport)
            {
                case "ITcp":
                    // If Port supplied in Control parameters, use it, otherwise use default Driver Port
                    int port = (propertiesConfig.Control.TcpSshProperties.Port != 0)? propertiesConfig.Control.TcpSshProperties.Port: ((ITcp)_radDevice).Port;
                    ((ITcp)_radDevice).Initialize(IPAddress.Parse(propertiesConfig.Control.TcpSshProperties.Address), port);
                    break;
                case "ISerialComport":
                    ComPort comPort = CommFactory.GetComPort(CommFactory.GetControlPropertiesConfig(dc));
                    var serialTransport = new SerialTransport(comPort);
                    var serialDriver = _radDevice as ISerialComport;
                    if (serialDriver != null)
                    {
                        // Driver default ComSpecs
                        serialTransport.SetComPortSpec(serialDriver.ComSpec);

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

            return new CCDDisplayDevice(dc.Key, dc.Name, propertiesConfig, _radDevice);
        }

    }
}

          