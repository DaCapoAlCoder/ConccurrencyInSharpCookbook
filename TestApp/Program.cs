using Chapter2;
using System.Threading.Tasks;

namespace TestApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var chapter2_1 = new Chapter2_1();
            await chapter2_1.Run();

            var chapter2_2 = new Chapter2_2();
            await chapter2_2.Run();

            var chapter2_3 = new Chapter2_3();
            await chapter2_3.Run();
        }
    }

}
