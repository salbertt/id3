using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Linq;

namespace id3
{
    class atributo
    {
        public atributo(string name, List<string> nombreAtributos)
        {
            nombre = name;
            diferentesAtributos = nombreAtributos;
        }

        public string nombre { get; }

        public List<string> diferentesAtributos { get; }

        public double InformationGain { get; set; }

        public static List<string> GetDifferentAttributeNamesOfColumn(DataTable data, int columnIndex)
        {
            var differentAttributes = new List<string>();

            for (var i = 0; i < data.Rows.Count; i++)
            {
                var found = differentAttributes.Any(t => t.ToUpper().Equals(data.Rows[i][columnIndex].ToString().ToUpper()));

                if (!found)
                {
                    differentAttributes.Add(data.Rows[i][columnIndex].ToString());
                }
            }

            return differentAttributes;
        }
    }
}
