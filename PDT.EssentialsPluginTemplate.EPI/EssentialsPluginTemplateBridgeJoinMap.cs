using PepperDash.Essentials.Core;

namespace EssentialsPluginTemplate
{
	/// <summary>
	/// Plugin device Bridge Join Map
	/// </summary>
	/// <remarks>
	/// Rename the class to match the device plugin being developed.  Reference Essentials JoinMaps, if one exists for the device plugin being developed
	/// </remarks>
	/// <see cref="PepperDash.Essentials.Core.Bridges"/>
	/// <example>
	/// "EssentialsPluginBridgeJoinMapTemplate" renamed to "SamsungMdcBridgeJoinMap"
	/// </example>
	public class EssentialsPluginBridgeJoinMapTemplate : JoinMapBaseAdvanced
	{
		#region Digital

		// TODO [ ] Add digital joins below plugin being developed

		[JoinName("IsOnline")]
		public JoinDataComplete IsOnline = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 1,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Is Online",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Digital
			});

		[JoinName("Connect")]
		public JoinDataComplete Connect = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 2,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Connect (Held)/Disconnect (Release) & corresponding feedback",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Digital
			});		

		#endregion


		#region Analog

		// TODO [ ] Add analog joins below plugin being developed

		[JoinName("Status")]
		public JoinDataComplete Status = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 1,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Socket Status",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Analog
			});

		#endregion


		#region Serial

		// TODO [ ] Add serial joins below plugin being developed

		public JoinDataComplete DeviceName = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 1,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Device Name",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Serial
			});

		#endregion

		/// <summary>
		/// Plugin device BridgeJoinMap constructor
		/// </summary>
		/// <param name="joinStart">This will be the join it starts on the EISC bridge</param>
		public EssentialsPluginBridgeJoinMapTemplate(uint joinStart)
			: base(joinStart, typeof(EssentialsPluginBridgeJoinMapTemplate))
		{
		}
	}
}