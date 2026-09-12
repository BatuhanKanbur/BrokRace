namespace Game.Simulation.Structure
{
    public readonly struct RuleCheck
    {
        public string Name { get; }
        public bool Passed { get; }
        public string Detail { get; }

        public RuleCheck(string name, bool passed, string detail)
        {
            Name = name;
            Passed = passed;
            Detail = detail;
        }
    }
}
