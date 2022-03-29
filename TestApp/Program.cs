using Chapter2;
using System.Threading.Tasks;

namespace TestApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //var chapter2_1 = new Chapter2_1();
            //await chapter2_1.Run();

            //var chapter2_2 = new Chapter2_2();
            //await chapter2_2.Run();

            //var chapter2_3 = new Chapter2_3();
            //await chapter2_3.Run();

            //var chapter2_4 = new Chapter2_4();
            //await chapter2_4.Run();

            //Chapter2_5 chapter2_5 = new();
            //await chapter2_5.Run();

            //Chapter2_6 chapter2_6 = new();
            //await chapter2_6.Run();

            //Chapter2_7 chapter2_7 = new();
            //await chapter2_7.Run();

            Chapter2_8 chapter2_8 = new();
            await chapter2_8.Run();
        }
    }
}
