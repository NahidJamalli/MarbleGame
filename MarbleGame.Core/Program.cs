namespace MarbleGame
{
    class Program
    {
        static void Main()
        {
            var response = MarbleGame.FindSolution(new System.IO.StreamReader("..\\..\\..\\..\\MarbleGame.Core\\Assets\\test_1.txt"));

            foreach (var item in response)
                System.Console.WriteLine(item);
        }
    }
}
