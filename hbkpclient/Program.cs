using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hbkpclient
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length == 0)
            {
                Console.WriteLine("Por favor, informe os parâmetros necessários para execução do comando.");
                Environment.Exit(1);
            }

            for (int i=0; i< args.Length; i++)
            {
                switch (args[i])
                {
                    case "-f"
                }
            }


            Console.WriteLine("Cliente não implementado.");
        }
    }
}
