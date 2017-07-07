using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DS4Windows
{
    public class DS4StateExposed
    {
        private DS4State _state;

        public DS4StateExposed()
        {
            _state = new DS4State();
        }
        public DS4StateExposed(DS4State state)
        {
            _state = state;
        }

        public bool Square { get { return _state.Square; } }
        public bool Triangle { get { return _state.Triangle; } }
        public bool Circle { get { return _state.Circle; } }
        public bool Cross { get { return _state.Cross; } }
        public bool DpadUp { get { return _state.DpadUp; } }
        public bool DpadDown { get { return _state.DpadDown; } }
        public bool DpadLeft { get { return _state.DpadLeft; } }
        public bool DpadRight { get { return _state.DpadRight; } }
        public bool L1 { get { return _state.L1; } }
        public bool L3 { get { return _state.L3; } }
        public bool R1 { get { return _state.R1; } }
        public bool R3 { get { return _state.R3; } }
        public bool Share { get { return _state.Share; } }
        public bool Options { get { return _state.Options; } }
        public bool PS { get { return _state.PS; } }
        public bool Touch1 { get { return _state.Touch1; } }
        public bool Touch2 { get { return _state.Touch2; } }
        public bool TouchButton { get { return _state.TouchButton; } }
        public byte LX { get { return _state.LX; } }
        public byte RX { get { return _state.RX; } }
        public byte LY { get { return _state.LY; } }
        public byte RY { get { return _state.RY; } }
        public byte L2 { get { return _state.L2; } }
        public byte R2 { get { return _state.R2; } }
        public int Battery { get { return _state.Battery; } }
        public SixAxis Motion { get { return _state.Motion; } }
        public int AccelX { get { return (Motion == null) ? 0 : (int)(Motion.accelX * 8192); } }
        public int AccelY { get { return (Motion == null) ? 0 : (int)(Motion.accelY * 8192); } }
        public int AccelZ { get { return (Motion == null) ? 0 : (int)(Motion.accelZ * 8192); } }

        public int GyroX { get { return (Motion == null) ? 0 : (int)(Motion.gyroPitch * 16); } }
        public int GyroY { get { return (Motion == null) ? 0 : (int)(Motion.gyroYaw * 16); } }
        public int GyroZ { get { return (Motion == null) ? 0 : (int)(Motion.gyroRoll * 16); } }
    }
}
