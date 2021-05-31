namespace ProducerConsumer
{
    public class Result
    {
        public Result(int raw, double calculated)
        {
            RawValue = raw;
            CalculatedValue = calculated;
        }

        public int RawValue { get; }
        public double CalculatedValue { get; }

        public override string ToString()
        {
            return $"Raw={RawValue}, CalculatedValue={CalculatedValue}";
        }
    }
}
