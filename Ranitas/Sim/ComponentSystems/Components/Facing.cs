namespace Ranitas.Sim
{
    public struct Facing
    {
        public Facing(int facing)
        {
            CurrentFacing = facing;
        }

        public int CurrentFacing;   //Positive right, negative left
    }
}
