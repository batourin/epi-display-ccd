using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using Crestron.RAD.Common.Enums;
using Crestron.RAD.Common.Interfaces;

namespace CCDDisplay
{
    public class CCDCommunicationMonitor: StatusMonitorBase
    {
        private IBasicVideoDisplay _device;
        private Action<DisplayStateObjects, IBasicVideoDisplay, byte> _stateChangeAction;

        public CCDCommunicationMonitor(IKeyed parent, IBasicVideoDisplay device, long warningTime, long errorTime)
            : base(parent, warningTime, errorTime)
        {
            _device = device;
            _stateChangeAction = new Action<DisplayStateObjects, IBasicVideoDisplay, byte>(stateChangeEvent);
        }

        public override void Start()
        {
            _device.StateChangeEvent -= _stateChangeAction;
            _device.StateChangeEvent += _stateChangeAction;
            getStatus();
        }

        public override void Stop()
        {
            _device.StateChangeEvent -= stateChangeEvent;
        }

        private void stateChangeEvent(Crestron.RAD.Common.Enums.DisplayStateObjects state, IBasicVideoDisplay display, byte arg3)
        {
            switch (state)
            {
                case DisplayStateObjects.Connection:
                    getStatus();
                    break;
            }
        }

        private void getStatus()
        {
            if (_device.Connected)
            {
                Status = MonitorStatus.IsOk;
                StopErrorTimers();
            }
            else
                StartErrorTimers();
        }

    }
}