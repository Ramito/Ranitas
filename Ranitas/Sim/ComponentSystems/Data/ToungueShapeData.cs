namespace Ranitas.Sim
{
    public struct ToungueShapeData
    {
        public ToungueShapeData(Data.FrogData frogData)
        {
            Length = frogData.ToungueLength;
            Thickness = frogData.ToungueThickness;
            RelativeVerticalOffset = 0.25f;  //TODO: Data drive!
        }

        public float Length;
        public float Thickness;
        public float RelativeVerticalOffset;
    }
}
