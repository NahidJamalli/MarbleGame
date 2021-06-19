using NUnit.Framework;

namespace MarbleGame.UnitTests
{
    public class FindSolutionTests
    {
        [Test]
        public void Default_Test_Two_Cases()
        {
            byte index = 0;
            string[] testCasesTrueResults = { "1_5_ENSWN", "2_impossible" };
            var response = MarbleGame.FindSolution(new System.IO.StreamReader("..\\..\\..\\..\\MarbleGame.Core\\Assets\\test_1.txt"), new System.IO.StreamWriter("..\\..\\..\\..\\MarbleGame.Core\\Assets\\output_1.txt"));

            foreach (var rp in response)
            {
                Assert.AreEqual(testCasesTrueResults[index++], rp);
            }
        }
    }
}
