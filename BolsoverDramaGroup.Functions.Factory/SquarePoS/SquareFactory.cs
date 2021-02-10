using Square;

namespace BolsoverDramaGroup.Functions.Factory.SquarePoS
{
    public abstract class SquareFactory
    {
        protected SquareClient Client { get; }
        public SquareFactory()
        {
            Client = new SquareClient.Builder()
                .Environment(Square.Environment.Production)
                .AccessToken(System.Environment.GetEnvironmentVariable("SquareTokenFromKeyVault", System.EnvironmentVariableTarget.Process))
                .Build();
        }
    }
}
