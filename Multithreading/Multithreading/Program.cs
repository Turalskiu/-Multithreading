namespace Multithreading
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (var pool = new ThreadPool())
            {
                pool.Queue(() => Console.WriteLine("Hello thread"));
            }


            Console.ReadLine();
        }
    }
}
