using System.Collections.Generic;
using Newtonsoft.Json;
using PepperDash.Essentials.Core;

namespace CCDDisplay
{
	/// <summary>
	/// Plugin device configuration object
	/// </summary>
	[ConfigSnippet("\"properties\":{\"control\":{}")]
	public class CCDDisplayConfig
	{
		/// <summary>
		/// JSON control object
		/// </summary>
		/// <remarks>
		/// Required for CCD Transports: ISerialComport, ICecDevice, IIr.
		/// </remarks>
		/// <example>
		/// <code>
		/// "control": {
        ///		"method": "com",
		///		"controlPortDevKey": "processor",
		///		"controlPortNumber": 1,
		///		"comParams": {
		///			"baudRate": 9600,
		///			"dataBits": 8,
		///			"stopBits": 1,
		///			"parity": "None",
		///			"protocol": "RS232",
		///			"hardwareHandshake": "None",
		///			"softwareHandshake": "None"
		///		}
		///	}
		/// </code>
		/// </example>
		[JsonProperty("control", Required=Required.Default)]
		public EssentialsControlPropertiesConfig Control { get; set; }

		/// <summary>
		/// Crestron Certified Drivers Device Id
		/// </summary>
		/// <remarks>
		/// Some drivers require to set device id matching id set on the actual Display device .
		/// </remarks>
		/// <value>
		/// Id property gets/sets the value as a byte
		/// </value>
		/// <example>
		/// <code>
		/// "properties": {
		///		"id": 0
		/// }
		/// </code>
		/// </example>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
		public byte Id { get; set; }

        /// <summary>
        /// Path to the DLL containing the driver
        /// </summary>
        /// <remarks>
        /// Full path to the DLL.
        /// </remarks>
        /// <value>
        /// Driver property gets/sets the value as a string
        /// </value>
        /// <example>
        /// <code>
        /// "properties": {
        ///		"driver": "\user\Driver.dll"
        /// }
        /// </code>
        /// </example>
        [JsonProperty("driver", Required=Required.Always)]
        public string Driver { get; set; }

        /// <summary>
        /// Transport used by CCD Driver
        /// </summary>
        /// <remarks>
        /// Remarks...
        /// </remarks>
        /// <value>
        /// Transport can be only one of the following values: ITcp, ISerialComport, ICecDevice, IIr, ICrestronConnected
        /// </value>
        /// <example>
        /// <code>
        /// "properties": {
        ///		"transport": "ISerialComport"
        /// }
        /// </code>
        /// </example>
        [JsonProperty("transport", Required=Required.Always)]
        public string Transport { get; set; }


		/// <summary>
		/// Constuctor
		/// </summary>
		/// <remarks>
		/// If using a collection you must instantiate the collection in the constructor
		/// to avoid exceptions when reading the configuration file 
		/// </remarks>
		public CCDDisplayConfig()
		{
		}
	}
}