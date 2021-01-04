using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;

namespace CCDDisplay
{
	/// <summary>
	/// Plugin device Bridge Join Map
	/// </summary>
	/// <remarks>
	/// Rename the class to match the device plugin being developed.  Reference Essentials JoinMaps, if one exists for the device plugin being developed
	/// </remarks>
	/// <see cref="PepperDash.Essentials.Core.Bridges"/>
    public class CCDDisplayBridgeJoinMap : DisplayControllerJoinMap
	{
		#region Digital

		// TODO: IsOnline signal is already defined in DisplayControllerJoinMap
        /*
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
        */
		[JoinName("Connect")]
		public JoinDataComplete Connect = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 52,
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
				JoinNumber = 52,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Communication Status",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Analog
			});

		#endregion


		#region Serial

		// TODO [ ] Add serial joins below plugin being developed

        [JoinName("DeviceName")]
		public JoinDataComplete DeviceName = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 52,
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
		public CCDDisplayBridgeJoinMap(uint joinStart)
			: base(joinStart, typeof(CCDDisplayBridgeJoinMap))
		{
		}
	}
}