// For Basic SIMPL# Classes
// For Basic SIMPL#Pro classes

using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;

namespace EssentialsPluginTemplate
{
	/// <summary>
	/// Plugin device
	/// </summary>
	/// <remarks>
	/// Rename the class to match the device plugin being developed.
	/// </remarks>
	/// <example>
	/// "EssentialsPluginDeviceTemplate" renamed to "SamsungMdcDevice"
	/// </example>
	public class EssentialsPluginTemplateCrestronDevice : CrestronGenericBridgeableBaseDevice
    {
        /// <summary>
        /// It is often desirable to store the config
        /// </summary>
        private EssentialsPluginConfigObjectTemplate _config;


        #region Constructor for Devices without IBasicCommunication.  Remove if not needed
        /// <summary>
        /// Plugin device constructor for Crestron devices
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="config"></param>
        /// <param name="hardware"></param>
        public EssentialsPluginTemplateCrestronDevice(string key, string name, EssentialsPluginConfigObjectTemplate config, GenericBase hardware)
            : base(key, name, hardware)
        {
            Debug.Console(0, this, "Constructing new {0} instance", name);

            // The base class takes care of registering the hardware device for you

            // TODO [ ] Update the constructor as needed for the plugin device being developed

            _config = config;
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

            trilist.OnlineStatusChange += (o, a) =>
            {
                if (!a.DeviceOnLine) return;

                trilist.SetString(joinMap.DeviceName.JoinNumber, Name);
            };
        }


        #endregion

    }
}

