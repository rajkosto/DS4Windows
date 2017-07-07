using System;

namespace DS4Windows
{
    public class DS4Cal
    {
        public static readonly int ACC_RESOLUTION_PER_G = 8192;
        public static readonly int GYRO_RESOLUTION_IN_DEG_SEC = 16;

        public struct CalValue
        {
            public short bias;
            public double mult;

            public short apply(short inValue)
            {
                int biased = (int)inValue - (int)bias;
                double scaled = (double)biased * mult;
                int rounded = (int)Math.Round(scaled);
                if (rounded > short.MaxValue)
                    return short.MaxValue;
                if (rounded < short.MinValue)
                    return short.MinValue;

                return (short)rounded;
            }
        }

        private CalValue _calPitch, _calYaw, _calRoll, _calX, _calY, _calZ;
        public CalValue Pitch { get { return _calPitch; } }
        public CalValue Yaw { get { return _calYaw; } }
        public CalValue Roll { get { return _calRoll; } }
        public CalValue X { get { return _calX; } }
        public CalValue Y { get { return _calY; } }
        public CalValue Z { get { return _calZ; } }

        public DS4Cal(byte[] calibReport)
        {
            bool fromBluetooth = (calibReport[0] == 5); //we always request feature report 5 from BT because it has CRC, the layout depends on USB or BT, not 2 or 5

            int idx = 0x1; //starts here, has 17 little endian shorts
            _calPitch.bias = (short)(((ushort)calibReport[idx + 0] << 0) + ((ushort)calibReport[idx + 1] << 8)); idx += 2;
            _calYaw.bias = (short)(((ushort)calibReport[idx + 0] << 0) + ((ushort)calibReport[idx + 1] << 8)); idx += 2;
            _calRoll.bias = (short)(((ushort)calibReport[idx + 0] << 0) + ((ushort)calibReport[idx + 1] << 8)); idx += 2;

            short pitch_pos, pitch_neg, yaw_pos, yaw_neg, roll_pos, roll_neg;
            if (fromBluetooth) //controller reports different layout of these depending on if its running the USB or BT stack
            {
                pitch_pos = (short)(((ushort)calibReport[idx + 0] << 0) + ((ushort)calibReport[idx + 1] << 8)); idx += 2;
                yaw_pos = (short)(((ushort)calibReport[idx + 0] << 0) + ((ushort)calibReport[idx + 1] << 8)); idx += 2;
                roll_pos = (short)(((ushort)calibReport[idx + 0] << 0) + ((ushort)calibReport[idx + 1] << 8)); idx += 2;

                pitch_neg = (short)(((ushort)calibReport[idx + 0] << 0) + ((ushort)calibReport[idx + 1] << 8)); idx += 2;
                yaw_neg = (short)(((ushort)calibReport[idx + 0] << 0) + ((ushort)calibReport[idx + 1] << 8)); idx += 2;
                roll_neg = (short)(((ushort)calibReport[idx + 0] << 0) + ((ushort)calibReport[idx + 1] << 8)); idx += 2;
            }
            else
            {
                pitch_pos = (short)(((ushort)calibReport[idx + 0] << 0) + ((ushort)calibReport[idx + 1] << 8)); idx += 2;
                pitch_neg = (short)(((ushort)calibReport[idx + 0] << 0) + ((ushort)calibReport[idx + 1] << 8)); idx += 2;

                yaw_pos = (short)(((ushort)calibReport[idx + 0] << 0) + ((ushort)calibReport[idx + 1] << 8)); idx += 2;
                yaw_neg = (short)(((ushort)calibReport[idx + 0] << 0) + ((ushort)calibReport[idx + 1] << 8)); idx += 2;

                roll_pos = (short)(((ushort)calibReport[idx + 0] << 0) + ((ushort)calibReport[idx + 1] << 8)); idx += 2;
                roll_neg = (short)(((ushort)calibReport[idx + 0] << 0) + ((ushort)calibReport[idx + 1] << 8)); idx += 2;
            }

            short gyro_pos_scale = (short)(((ushort)calibReport[idx + 0] << 0) + ((ushort)calibReport[idx + 1] << 8)); idx += 2;
            short gyro_neg_scale = (short)(((ushort)calibReport[idx + 0] << 0) + ((ushort)calibReport[idx + 1] << 8)); idx += 2;

            int gyro_numer = ((int)gyro_pos_scale + (int)gyro_neg_scale) * GYRO_RESOLUTION_IN_DEG_SEC;

            _calPitch.mult = (double)gyro_numer / (double)((int)pitch_pos - (int)pitch_neg);
            _calYaw.mult = (double)gyro_numer / (double)((int)yaw_pos - (int)yaw_neg);
            _calRoll.mult = (double)gyro_numer / (double)((int)roll_pos - (int)roll_neg);

            int acc_dbl_range = 2 * ACC_RESOLUTION_PER_G; //to

            short x_pos = (short)(((ushort)calibReport[idx + 0] << 0) + ((ushort)calibReport[idx + 1] << 8)); idx += 2;
            short x_neg = (short)(((ushort)calibReport[idx + 0] << 0) + ((ushort)calibReport[idx + 1] << 8)); idx += 2;

            short y_pos = (short)(((ushort)calibReport[idx + 0] << 0) + ((ushort)calibReport[idx + 1] << 8)); idx += 2;
            short y_neg = (short)(((ushort)calibReport[idx + 0] << 0) + ((ushort)calibReport[idx + 1] << 8)); idx += 2;

            short z_pos = (short)(((ushort)calibReport[idx + 0] << 0) + ((ushort)calibReport[idx + 1] << 8)); idx += 2;
            short z_neg = (short)(((ushort)calibReport[idx + 0] << 0) + ((ushort)calibReport[idx + 1] << 8)); idx += 2;

            //from
            int x_dbl_range = (int)x_pos - (int)x_neg;
            int y_dbl_range = (int)y_pos - (int)y_neg;
            int z_dbl_range = (int)z_pos - (int)z_neg;

            _calX.bias = (short)((int)x_pos - ((int)x_dbl_range / 2));
            _calX.mult = (double)acc_dbl_range / (double)x_dbl_range;

            _calY.bias = (short)((int)y_pos - ((int)y_dbl_range / 2));
            _calY.mult = (double)acc_dbl_range / (double)y_dbl_range;

            _calY.bias = (short)((int)z_pos - ((int)z_dbl_range / 2));
            _calZ.mult = (double)acc_dbl_range / (double)z_dbl_range;
        }

        public Tuple<short[], short[]> ApplyCalToInReport(byte[] inputReport, int startOffs = 0)
        {
            int idx = startOffs + 13; //first byte of gyro is here

            var prevVals = new short[6];
            var currVals = new short[6];
            for (int i = 0; i < 6; i++)
            {
                short axisVal = (short)(((ushort)inputReport[idx + 1] << 8) | (ushort)inputReport[idx + 0]);
                prevVals[i] = axisVal;

                if (i == 0)
                    axisVal = Pitch.apply(axisVal);
                else if (i == 1)
                    axisVal = Yaw.apply(axisVal);
                else if (i == 2)
                    axisVal = Roll.apply(axisVal);
                else if (i == 3)
                    axisVal = X.apply(axisVal);
                else if (i == 4)
                    axisVal = Y.apply(axisVal);
                else if (i == 5)
                    axisVal = Z.apply(axisVal);

                currVals[i] = axisVal;
                //put it back into the input report (little endian short)
                inputReport[idx++] = (byte)(((ushort)axisVal >> 0) & 0xFF);
                inputReport[idx++] = (byte)(((ushort)axisVal >> 8) & 0xFF);
            }

            return new Tuple<short[], short[]>(prevVals, currVals);
        }
    }
}