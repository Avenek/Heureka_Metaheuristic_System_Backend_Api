namespace Heureka_Metaheuristic_System_Backend_Api.Helpers
{
    public class NewTestRunner : SingleSessionTestRunner
    {
        public override async Task PrepareInitializeParameters()
        {
            InitialParameters = AlgorithmParametersConfig.Select(p => p.MinValue).ToArray();
            InitialDimension = MinimalDimension;
        }
    }
}
