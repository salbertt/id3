using System;
using System.Collections.Generic;
using System.Data;
using System.IO;


namespace id3
{
    class Program
    {       
            private static void Main(string[] args)
            {
                Console.WindowWidth = Console.LargestWindowWidth - 10;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Bienvenido al arbol de decisiones ID3");
                Console.WriteLine("---------------------------------------");
                Console.ResetColor();

                do
                {
                    var data = new DataTable();

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("1 - importar datos de archivo csv");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Cualquier otra tecla para salir");
                    Console.ResetColor();
                    var input = ReadLineTrimmed();

                    switch (input)
                    {
                        // data will be imported from csv file
                        case "1":
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine("\nEscribe el path para el archivo .csv que deseas importar");
                            Console.ResetColor();
                            input = ReadLineTrimmed();

                            data = basedatos.ImportFromCsvFile(input);

                            if (data == null)
                            {
                            mensajeError("Ocurrió un error intentando importar el archivo. Presione cualquier tecla para salir");
                                Console.ReadKey();
                                EndProgram();
                            }
                            else
                            {
                                CreateTreeAndHandleUserOperation(data);
                            }

                            break;

                    // user enters data by hand
                    default:
                            EndProgram();
                            break;
                    }
                } while (true);
            }

            private static string ReadLineTrimmed()
            {
                return Console.ReadLine().TrimStart().TrimEnd();
            }

            private static void CreateTreeAndHandleUserOperation(DataTable data)
            {
                var decisionTree = new arbol();
                decisionTree.Root = arbol.Learn(data, "");
                var returnToMainMenu = false;

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nSe creó el árbol de decisiones");
                Console.ResetColor();

                do
                {
                    var valuesForQuery = new Dictionary<string, string>();

                    // loop for data input for the query and some special commands
                    for (var i = 0; i < data.Columns.Count - 1; i++)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"\nEscribe al valor de  {data.Columns[i]} o escribe HELP para una lista de otras opciones");
                        Console.ResetColor();
                        var input = ReadLineTrimmed();

                        if (input.ToUpper().Equals("SALIR"))
                        {
                            EndProgram();
                        }
                        else if (input.ToUpper().Equals("PRINT"))
                        {
                            Console.WriteLine();
                            arbol.Print(decisionTree.Root, decisionTree.Root.nombre.ToUpper());
                            arbol.PrintLegend("Due to the limitation of the console the tree is displayed as a list of every possible route. The colors indicate the following values:");

                            i--;
                        }
                        else if (input.ToUpper().Equals("MENU"))
                        {
                            returnToMainMenu = true;
                            Console.WriteLine();

                            break;
                        }
                        else if (input.ToUpper().Equals("HELP"))
                        {
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.WriteLine("'Print' para imprimir el arbol");
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("'Salir' para salir del programa");
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.WriteLine("'Menu' para regresar al menu principal");
                            Console.ForegroundColor = ConsoleColor.Gray;

                            i--;
                        }
                        else if (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input))
                        {
                        mensajeError("El atributo no puede estar vacío ni ser un espacio");
                            i--;
                        }
                        else
                        {
                            valuesForQuery.Add(data.Columns[i].ToString(), input);
                        }
                    }

                    // if input was not to return to the main menu, the query will be processed
                    if (!returnToMainMenu)
                    {
                        var result = arbol.CalculateResult(decisionTree.Root, valuesForQuery, "");

                        Console.WriteLine();

                        if (result.Contains("No se encontró ese  atributo"))
                        {
                        mensajeError("No existe una ruta para estas condiciones; no se puede calcular el resultado :/");
                        }
                        else
                        {
                            arbol.Print(null, result);
                            arbol.PrintLegend("The colors indicate the following values:");
                        }
                    }
                } while (!returnToMainMenu);
            }

        private static void mensajeError(string errorMessage)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n{errorMessage}\n");
            Console.ResetColor();
        }

        private static void EndProgram()
            {
                Environment.Exit(0);
            }
        
    }
}
