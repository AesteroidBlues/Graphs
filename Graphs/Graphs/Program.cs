using System;

namespace Graphs
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (SearchAlgorithms game = new SearchAlgorithms())
            {
                game.Run();
            }
        }
    }
#endif
}

