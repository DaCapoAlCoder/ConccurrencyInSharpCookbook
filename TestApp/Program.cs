using Chapter2;
using Chapter3;
using System.Threading.Tasks;

namespace TestApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            #region Chapter2
            //Chapter2_01 chapter2_1 = new();
            //await chapter2_1.Run();

            //Chapter2_02 chapter2_2 = new();
            //await chapter2_2.Run();

            //Chapter2_03 chapter2_3 = new();
            //await chapter2_3.Run();

            //Chapter2_04 chapter2_4 = new();
            //await chapter2_4.Run();

            //Chapter2_05 chapter2_5 = new();
            //await chapter2_5.Run();

            //Chapter2_06 chapter2_6 = new();
            //await chapter2_6.Run();

            //Chapter2_07 chapter2_7 = new();
            //await chapter2_7.Run();

            //Chapter2_08 chapter2_8 = new();
            //await chapter2_8.Run();

            //Chapter2_09 chapter2_9 = new();
            //await chapter2_9.Run();

            //Chapter2_10 chapter2_10 = new();
            //await chapter2_10.Run();

            //Chapter2_11 chapter2_11 = new();
            //await chapter2_11.Run();
            #endregion

            #region Chapter3
            Chapter3_01 chapter3 = new();
            await chapter3.Run();
            #endregion
        }
    }
}
