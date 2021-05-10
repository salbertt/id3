using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;

namespace id3
{
    class basedatos
    {
        public static DataTable ImportFromCsvFile(string filePath)
        {
            var rows = 0;
            var data = new DataTable();

            try
            {
                using (var reader = new StreamReader(File.OpenRead(filePath)))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Substring(0, line.Length - 1).Split(',');

                        foreach (var item in values)
                        {
                            if (string.IsNullOrEmpty(item) || string.IsNullOrWhiteSpace(item))
                            {
                                throw new Exception("No puede estar vacío el valor");
                            }

                            if (rows == 0)
                            {
                                data.Columns.Add(item);
                            }
                        }

                        if (rows > 0)
                        {
                            data.Rows.Add(values);
                        }

                        rows++;

                        if (values.Length != data.Columns.Count)
                        {
                            throw new Exception("La fila no es igual de larga que la fila de titulos");
                        }
                    }
                }

                var differentValuesOfLastColumn = atributo.GetDifferentAttributeNamesOfColumn(data, data.Columns.Count - 1);
                for(int i=0;i<differentValuesOfLastColumn.Count;i++)
                    Console.WriteLine(differentValuesOfLastColumn[i]);
                if (differentValuesOfLastColumn.Count > 2)
                {
                    throw new Exception("La ultima columna solo puede tener dos valores diferentes");
                }
            }
            catch (Exception ex)
            {
                DisplayErrorMessage(ex.Message);
                data = null;
            }

            // if no rows are entered or data == null, return null
            return data?.Rows.Count > 0 ? data : null;
        }

        private static void DisplayErrorMessage(string errorMessage)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n{errorMessage}\n");
            Console.ResetColor();
        }
    }
}
