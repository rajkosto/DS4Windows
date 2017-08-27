using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows
{
    public class SixAxisEventArgs : EventArgs
    {
        public readonly SixAxis sixAxis;
        public readonly System.DateTime timeStamp;
        public SixAxisEventArgs(System.DateTime utcTimestamp, SixAxis sa)
        {
            sixAxis = sa;
            this.timeStamp = utcTimestamp;
        }
    }

    public class SixAxis
    {
        public readonly SixAxis previous;

        public readonly float gyroPitch, gyroYaw, gyroRoll; //in deg/s
        public readonly float accelX, accelY, accelZ;       //in Gs/s^2
        public readonly ulong timestampUs;                  //from controller, in microseconds

        public readonly int fakeGyroPitch, fakeGyroYaw, fakeGyroRoll;
        public readonly int fakeAccelX, fakeAccelY, fakeAccelZ;
        public int outputAccelX, outputAccelY, outputAccelZ;

        public SixAxis(ulong microseconds, float gX, float gY, float gZ, float aX, float aY, float aZ, SixAxis prevAxis = null)
        {
            timestampUs = microseconds;
            gyroPitch = gX;
            gyroYaw = gY;
            gyroRoll = gZ;
            accelX = aX;
            accelY = aY;
            accelZ = aZ;
            previous = prevAxis;

            // Put accel ranges between 0 - 128 abs
            fakeAccelX = (int)Math.Round(aX * (DS4Cal.ACC_RESOLUTION_PER_G / 64));
            fakeAccelY = (int)Math.Round(aY * (DS4Cal.ACC_RESOLUTION_PER_G / 64));
            fakeAccelZ = (int)Math.Round(-aZ * (DS4Cal.ACC_RESOLUTION_PER_G / 64));
			
            outputAccelX = fakeAccelX;
            outputAccelY = fakeAccelY;
            outputAccelZ = fakeAccelZ;

            // Legacy values
            fakeGyroPitch = (int)Math.Round(gX / (256 / DS4Cal.GYRO_RESOLUTION_IN_DEG_SEC));
            fakeGyroYaw =   (int)Math.Round(gY / (256 / DS4Cal.GYRO_RESOLUTION_IN_DEG_SEC));
            fakeGyroRoll =  (int)Math.Round(gZ / (256 / DS4Cal.GYRO_RESOLUTION_IN_DEG_SEC));
        }
    }

    public class DS4SixAxis
    {
        public event EventHandler<SixAxisEventArgs> SixAccelMoved = null;

        private DS4Cal _cal = null;

        public void setCalibrationData(byte[] calibData)
        {
            if (calibData == null)
                _cal = null;
            else
                _cal = new DS4Cal(calibData);
        }

        private SixAxis _values = new SixAxis(0, 0, 0, 0, 0, 0, 0, null);
        public SixAxis Values { get { return _values; } }

        internal long _prevReportTimestamp = -1;
        public void handleSixaxis(byte[] inputReport, DateTime receivedTimestamp, string MacAddress)
        {
            uint timestamp = (((uint)inputReport[11]) << 8) | ((uint)inputReport[10]);
            ulong fullTimestamp = 0;

            // convert wrapped time to absolute time
            if (_prevReportTimestamp < 0) //first one, start from zero
                fullTimestamp = ((uint)timestamp * 16) / 3;
            else
            {
                ushort delta;
                if (_prevReportTimestamp > timestamp) //wrapped around
                    delta = (ushort)(ushort.MaxValue - _prevReportTimestamp + timestamp + 1);
                else
                    delta = (ushort)(timestamp - _prevReportTimestamp);

                fullTimestamp = Values.timestampUs + (((uint)delta * 16) / 3);
            }
            _prevReportTimestamp = timestamp;

            if (_cal != null)
            {
                var calValues = _cal.ApplyCalToInReport(inputReport);

#if DUMP_DS4_CALIBRATION
                short[] preCal = calValues.Item1;
                short[] posCal = calValues.Item2;
                short[] delta = new short[preCal.Length];

                for (int i=1; i<posCal.Length; i++)
                {
                    preCal[i] = (short)-preCal[i];
                    posCal[i] = (short)-posCal[i];
                    delta[i] = (short)(posCal[i] - preCal[i]);
                }

                var fmt = "+00000;-00000";
                Console.WriteLine(MacAddress.ToString() + "> " +
                    String.Format("Cal applied (ts: {0}) pre: ({1} {2} {3}) ({4} {5} {6}) post: ({7} {8} {9}) ({10} {11} {12}) delta: ({13} {14} {15}) ({16} {17} {18})",
                        ((double)(fullTimestamp) / 1000).ToString("0.000"),
                        preCal[0].ToString(fmt), preCal[1].ToString(fmt), preCal[2].ToString(fmt), preCal[3].ToString(fmt), preCal[4].ToString(fmt), preCal[5].ToString(fmt),
                        posCal[0].ToString(fmt), posCal[1].ToString(fmt), posCal[2].ToString(fmt), posCal[3].ToString(fmt), posCal[4].ToString(fmt), posCal[5].ToString(fmt),
                        delta[0].ToString(fmt), delta[1].ToString(fmt), delta[2].ToString(fmt), delta[3].ToString(fmt), delta[4].ToString(fmt), delta[5].ToString(fmt))
                );
#endif
            }

            int intPitch = (short)(((ushort)inputReport[14] << 8) | (ushort)inputReport[13]);
            int intYaw  = (short)-(((ushort)inputReport[16] << 8) | (ushort)inputReport[15]);
            int intRoll = (short)-(((ushort)inputReport[18] << 8) | (ushort)inputReport[17]);
            int intAccX = (short)-(((ushort)inputReport[20] << 8) | (ushort)inputReport[19]);
            int intAccY = (short)-(((ushort)inputReport[22] << 8) | (ushort)inputReport[21]);
            int intAccZ = (short)-(((ushort)inputReport[24] << 8) | (ushort)inputReport[23]);

            float angVelPitch   = (float)(intPitch) / DS4Cal.GYRO_RESOLUTION_IN_DEG_SEC;  //in deg/s
            float angVelYaw     = (float)(intYaw)   / DS4Cal.GYRO_RESOLUTION_IN_DEG_SEC;  //in deg/s
            float angVelRoll    = (float)(intRoll)  / DS4Cal.GYRO_RESOLUTION_IN_DEG_SEC;  //in deg/s

            float accelX = (float)(intAccX) / DS4Cal.ACC_RESOLUTION_PER_G; //in Gs/s^2
            float accelY = (float)(intAccY) / DS4Cal.ACC_RESOLUTION_PER_G; //in Gs/s^2
            float accelZ = (float)(intAccZ) / DS4Cal.ACC_RESOLUTION_PER_G; //in Gs/s^2

            SixAxis sPrev;
            sPrev = new SixAxis(Values.timestampUs, Values.gyroPitch, Values.gyroYaw, Values.gyroRoll, Values.accelX, Values.accelY, Values.accelZ);
            _values = new SixAxis(fullTimestamp, angVelPitch, angVelYaw, angVelRoll, accelX, accelY, accelZ, sPrev);

            SixAxisEventArgs args;
            if (SixAccelMoved != null)
            {
                args = new SixAxisEventArgs(receivedTimestamp, Values);
                SixAccelMoved(this, args);
            }
        }
    }
}