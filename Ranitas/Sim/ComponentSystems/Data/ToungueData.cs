namespace Ranitas.Sim
{
    public class ToungueData
    {
        public ToungueData(Data.FrogData data)
        {
            mStateTimes = new float[(int)ToungueStages.Extending + 1];
            mStateTimes[(int)ToungueStages.Extending] = data.ToungueExtendTime;
            mStateTimes[(int)ToungueStages.Extended] = data.ToungueFullyExtendedTime;
            mStateTimes[(int)ToungueStages.Retracting] = data.ToungueRetractTime;
            mStateTimes[(int)ToungueStages.Refreshing] = data.ToungueRefreshTime;
        }

        public float GetStateTime(ToungueStages state)
        {
            return mStateTimes[(int)state];
        }

        private float[] mStateTimes;
    }
}
