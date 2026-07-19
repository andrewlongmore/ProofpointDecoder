namespace ProofpointDecoder
{
    internal sealed class Program
    {
        static void Main(string[] args)
        {
            foreach (string arg in args)
            {
                Console.WriteLine(ProofpointUtils.GetProofpointUrl(arg));
            }
        }
    }
}
