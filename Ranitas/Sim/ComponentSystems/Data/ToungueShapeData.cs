namespace Ranitas.Sim
{
    public struct ToungueShapeData
    {
        public ToungueShapeData(Data.FrogData frogData)
        {
            Length = frogData.ToungueLength;
            Thickness = frogData.ToungueThickness;
            RelativeVerticalOffset = frogData.ToungueRelativeVerticalOffset;
        }

        public float Length;
        public float Thickness;
        public float RelativeVerticalOffset;
    }
}
