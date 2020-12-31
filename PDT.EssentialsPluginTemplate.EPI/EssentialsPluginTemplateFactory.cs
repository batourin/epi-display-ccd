using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using Crestron.SimplSharpPro.UI;

namespace EssentialsPluginTemplate
{
	/// <summary>
	/// Plugin device factory for devices that use IBasicCommunication
	/// </summary>
	/// <remarks>
	/// Rename the class to match the device plugin being developed
	/// </remarks>
	/// <example>
	/// "EssentialsPluginFactoryTemplate" renamed to "MyDeviceFactory"
	/// </example>
    public class EssentialsPluginFactoryTemplate : EssentialsPluginDeviceFactory<EssentialsPluginTemplateDevice>
    {
		/// <summary>
		/// Plugin device factory constructor
		/// </summary>
		/// <remarks>
		/// Update the MinimumEssentialsFrameworkVersion & TypeNames as needed when creating a plugin
		/// </remarks>
		/// <example>
 		/// Set the minimum Essentials Framework Version
		/// <code>
		/// MinimumEssentialsFrameworkVersion = "1.6.4;
        /// </code>
		/// In the constructor we initialize the list with the typenames that will build an instance of this device
        /// <code>
		/// TypeNames = new List<string>() { "SamsungMdc", "SamsungMdcDisplay" };
        /// </code>
		/// </example>
        public EssentialsPluginFactoryTemplate()
        {
            // Set the minimum Essentials Framework Version
			// TODO [ ] Update the Essentials minimum framework version which this plugin has been tested against
			MinimumEssentialsFrameworkVersion = "1.6.4";

            // In the constructor we initialize the list with the typenames that will build an instance of this device
			// TODO [ ] Update the TypeNames for the plugin being developed
            TypeNames = new List<string>() { "examplePluginDevice" };
        }
        
		/// <summary>
		/// Builds and returns an instance of EssentialsPluginDeviceTemplate
		/// </summary>
		/// <param name="dc">device configuration</param>
		/// <returns>plugin device or null</returns>
		/// <remarks>		
		/// The example provided below takes the device key, name, properties config and the comms device created.
		/// Modify the EssetnialsPlugingDeviceTemplate constructor as needed to meet the requirements of the plugin device.
		/// </remarks>
		/// <seealso cref="PepperDash.Core.eControlMethod"/>
        public override EssentialsDevice BuildDevice(PepperDash.Essentials.Core.Config.DeviceConfig dc)
        {
            Debug.Console(1, "[{0}] Factory Attempting to create new device from type: {1}", dc.Key, dc.Type);

            // get the plugin device properties configuration object & check for null 
            var propertiesConfig = dc.Properties.ToObject<EssentialsPluginConfigObjectTemplate>();
            if (propertiesConfig == null)
            {
                Debug.Console(0, "[{0}] Factory: failed to read properties config for {1}", dc.Key, dc.Name);
                return null;
            }

            // attempt build the plugin device comms device & check for null
            // TODO { ] As of PepperDash Core 1.0.41, HTTP and HTTPS are not valid eControlMethods and will throw an exception.
            var comms = CommFactory.CreateCommForDevice(dc);
            if (comms == null)
            {
                Debug.Console(1, "[{0}] Factory Notice: No control object present for device {1}", dc.Key, dc.Name);
                return null;
            }
            else
            {
                return new EssentialsPluginTemplateDevice(dc.Key, dc.Name, propertiesConfig, comms);
            }

        }

    }

    /// <summary>
    /// Plugin device factory for logic devices that don't communicate
    /// </summary>
    /// <remarks>
    /// Rename the class to match the device plugin being developed
    /// </remarks>
    /// <example>
    /// "EssentialsPluginFactoryTemplate" renamed to "MyLogicDeviceFactory"
    /// </example>
    public class EssentialsPluginFactoryLogicDeviceTemplate : EssentialsPluginDeviceFactory<EssentialsPluginTemplateLogicDevice>
    {
		/// <summary>
		/// Plugin device factory constructor
		/// </summary>
		/// <remarks>
		/// Update the MinimumEssentialsFrameworkVersion & TypeNames as needed when creating a plugin
		/// </remarks>
		/// <example>
 		/// Set the minimum Essentials Framework Version
		/// <code>
		/// MinimumEssentialsFrameworkVersion = "1.6.4;
        /// </code>
		/// In the constructor we initialize the list with the typenames that will build an instance of this device
        /// <code>
		/// TypeNames = new List<string>() { "SamsungMdc", "SamsungMdcDisplay" };
        /// </code>
		/// </example>
        public EssentialsPluginFactoryLogicDeviceTemplate()
        {
            // Set the minimum Essentials Framework Version
			// TODO [ ] Update the Essentials minimum framework version which this plugin has been tested against
			MinimumEssentialsFrameworkVersion = "1.6.4";

            // In the constructor we initialize the list with the typenames that will build an instance of this device
			// TODO [ ] Update the TypeNames for the plugin being developed
            TypeNames = new List<string>() { "examplePluginLogicDevice" };
        }
        
		/// <summary>
        /// Builds and returns an instance of EssentialsPluginTemplateLogicDevice
		/// </summary>
		/// <param name="dc">device configuration</param>
		/// <returns>plugin device or null</returns>
		/// <remarks>		
		/// The example provided below takes the device key, name, properties config and the comms device created.
		/// Modify the EssetnialsPlugingDeviceTemplate constructor as needed to meet the requirements of the plugin device.
		/// </remarks>
		/// <seealso cref="PepperDash.Core.eControlMethod"/>
        public override EssentialsDevice BuildDevice(PepperDash.Essentials.Core.Config.DeviceConfig dc)
        {

            Debug.Console(1, "[{0}] Factory Attempting to create new device from type: {1}", dc.Key, dc.Type);

            // get the plugin device properties configuration object & check for null 
            var propertiesConfig = dc.Properties.ToObject<EssentialsPluginConfigObjectTemplate>();
            if (propertiesConfig == null)
            {
                Debug.Console(0, "[{0}] Factory: failed to read properties config for {1}", dc.Key, dc.Name);
                return null;
            }

            var controlConfig = CommFactory.GetControlPropertiesConfig(dc);

            if (controlConfig == null)
            {
                return new EssentialsPluginTemplateLogicDevice(dc.Key, dc.Name, propertiesConfig);
            }
            else
            {
                Debug.Console(0, "[{0}] Factory: Unable to get control properties from device config for {1}", dc.Key, dc.Name);
                return null;
            }
        }
    }

    /// <summary>
    /// Plugin device factory for Crestron wrapper devices
    /// </summary>
    /// <remarks>
    /// Rename the class to match the device plugin being developed
    /// </remarks>
    /// <example>
    /// "EssentialsPluginFactoryTemplate" renamed to "MyCrestronDeviceFactory"
    /// </example>
    public class EssentialsPluginFactoryCrestronDeviceTemplate : EssentialsPluginDeviceFactory<EssentialsPluginTemplateCrestronDevice>
    {
        /// <summary>
        /// Plugin device factory constructor
        /// </summary>
        /// <remarks>
        /// Update the MinimumEssentialsFrameworkVersion & TypeNames as needed when creating a plugin
        /// </remarks>
        /// <example>
        /// Set the minimum Essentials Framework Version
        /// <code>
        /// MinimumEssentialsFrameworkVersion = "1.6.4;
        /// </code>
        /// In the constructor we initialize the list with the typenames that will build an instance of this device
        /// <code>
        /// TypeNames = new List<string>() { "SamsungMdc", "SamsungMdcDisplay" };
        /// </code>
        /// </example>
        public EssentialsPluginFactoryCrestronDeviceTemplate()
        {
            // Set the minimum Essentials Framework Version
            // TODO [ ] Update the Essentials minimum framework version which this plugin has been tested against
            MinimumEssentialsFrameworkVersion = "1.6.4";

            // In the constructor we initialize the list with the typenames that will build an instance of this device
            // TODO [ ] Update the TypeNames for the plugin being developed
            TypeNames = new List<string>() { "examplePluginCrestronDevice" };
        }

        /// <summary>
        /// Builds and returns an instance of EssentialsPluginTemplateCrestronDevice
        /// </summary>
        /// <param name="dc">device configuration</param>
        /// <returns>plugin device or null</returns>
        /// <remarks>		
        /// The example provided below takes the device key, name, properties config and the comms device created.
        /// Modify the EssetnialsPlugingDeviceTemplate constructor as needed to meet the requirements of the plugin device.
        /// </remarks>
        /// <seealso cref="PepperDash.Core.eControlMethod"/>
        public override EssentialsDevice BuildDevice(PepperDash.Essentials.Core.Config.DeviceConfig dc)
        {

            Debug.Console(1, "[{0}] Factory Attempting to create new device from type: {1}", dc.Key, dc.Type);

            // get the plugin device properties configuration object & check for null 
            var propertiesConfig = dc.Properties.ToObject<EssentialsPluginConfigObjectTemplate>();
            if (propertiesConfig == null)
            {
                Debug.Console(0, "[{0}] Factory: failed to read properties config for {1}", dc.Key, dc.Name);
                return null;
            }

            var controlConfig = CommFactory.GetControlPropertiesConfig(dc);

            if (controlConfig == null)
            {
                var myTouchpanel = new Tsw760(controlConfig.IpIdInt, Global.ControlSystem);

                return new EssentialsPluginTemplateCrestronDevice(dc.Key, dc.Name, propertiesConfig, myTouchpanel);
            }
            else
            {
                Debug.Console(0, "[{0}] Factory: Unable to get control properties from device config for {1}", dc.Key, dc.Name);
                return null;
            }
        }
    }

}

          