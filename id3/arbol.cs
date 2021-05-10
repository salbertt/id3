using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Linq;

namespace id3
{
    class arbol
    {
            public nodo Root { get; set; }

            public static void Print(nodo node, string result)
            {
                if (node?.hijes == null || node.hijes.Count == 0)
                {
                    var seperatedResult = result.Split(' ');

                    foreach (var item in seperatedResult)
                    {
                        if (item.Equals(seperatedResult[0]))
                        {
                            Console.ForegroundColor = ConsoleColor.Magenta;
                        }
                        else if (item.Equals("--") || item.Equals("-->"))
                        {
                            // empty if but better than checking at .ToUpper() and .ToLower() if
                        }
                        else if (item.Equals("YES") || item.Equals("NO"))
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                        }
                        else if (item.ToUpper().Equals(item))
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                        }

                        Console.Write($"{item} ");
                        Console.ResetColor();
                    }

                    Console.WriteLine();

                    return;
                }

                foreach (var child in node.hijes)
                {
                    Print(child, result + " -- " + child.Edge.ToLower() + " --> " + child.nombre.ToUpper());
                }
            }

            public static void PrintLegend(string headline)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"\n{headline}");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("Rojo indica el nombre del nodo raíz");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Amarillo representa un valor");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Azul indica un nodo");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Verde representa la solución");
                Console.ResetColor();
            }

            public static string CalculateResult(nodo root, IDictionary<string, string> valuesForQuery, string result)
            {
                var valueFound = false;

                result += root.nombre.ToUpper() + " -- ";

                if (root.IsLeaf)
                {
                    result = root.Edge.ToLower() + " --> " + root.nombre.ToUpper();
                    valueFound = true;
                }
                else
                {
                    foreach (var childNode in root.hijes)
                    {
                        foreach (var entry in valuesForQuery)
                        {
                            if (childNode.Edge.ToUpper().Equals(entry.Value.ToUpper()) && root.nombre.ToUpper().Equals(entry.Key.ToUpper()))
                            {
                                valuesForQuery.Remove(entry.Key);

                                return result + CalculateResult(childNode, valuesForQuery, $"{childNode.Edge.ToLower()} --> ");
                            }
                        }
                    }
                }

                // if the user entered an invalid attribute
                if (!valueFound)
                {
                    result = "Attribute not found";
                }

                return result;
            }

            public static nodo Learn(DataTable data, string edgeName)
            {
                var root = GetRootNode(data, edgeName);

                foreach (var item in root.NodeAttribute.diferentesAtributos)
                {
                    // if a leaf, leaf will be added in this method
                    var isLeaf = CheckIfIsLeaf(root, data, item);

                    // make a recursive call as long as the node is not a leaf
                    if (!isLeaf)
                    {
                        var reducedTable = CreateSmallerTable(data, item, root.TableIndex);

                        root.hijes.Add(Learn(reducedTable, item));
                    }
                }

                return root;
            }

            private static bool CheckIfIsLeaf(nodo root, DataTable data, string attributeToCheck)
            {
                var isLeaf = true;
                var allEndValues = new List<string>();

                // get all leaf values for the attribute in question
                for (var i = 0; i < data.Rows.Count; i++)
                {
                    if (data.Rows[i][root.TableIndex].ToString().Equals(attributeToCheck))
                    {
                        allEndValues.Add(data.Rows[i][data.Columns.Count - 1].ToString());
                    }
                }

                // check whether all elements of the list have the same value
                if (allEndValues.Count > 0 && allEndValues.Any(x => x != allEndValues[0]))
                {
                    isLeaf = false;
                }

                // create leaf with value to display and edge to the leaf
                if (isLeaf)
                {
                    root.hijes.Add(new nodo(true, allEndValues[0], attributeToCheck));
                }

                return isLeaf;
            }

            private static DataTable CreateSmallerTable(DataTable data, string edgePointingToNextNode, int rootTableIndex)
            {
                var smallerData = new DataTable();

                // add column titles
                for (var i = 0; i < data.Columns.Count; i++)
                {
                    smallerData.Columns.Add(data.Columns[i].ToString());
                }

                // add rows which contain edgePointingToNextNode to new datatable
                for (var i = 0; i < data.Rows.Count; i++)
                {
                    if (data.Rows[i][rootTableIndex].ToString().Equals(edgePointingToNextNode))
                    {
                        var row = new string[data.Columns.Count];

                        for (var j = 0; j < data.Columns.Count; j++)
                        {
                            row[j] = data.Rows[i][j].ToString();
                        }

                        smallerData.Rows.Add(row);
                    }
                }

                // remove column which was already used as node            
                smallerData.Columns.Remove(smallerData.Columns[rootTableIndex]);

                return smallerData;
            }

            private static nodo GetRootNode(DataTable data, string edge)
            {
                var attributes = new List<atributo>();
                var highestInformationGainIndex = -1;
                var highestInformationGain = double.MinValue;

                // Get all names, amount of attributes and attributes for every column             
                for (var i = 0; i < data.Columns.Count - 1; i++)
                {
                    var differentAttributenames = atributo.GetDifferentAttributeNamesOfColumn(data, i);
                    attributes.Add(new atributo(data.Columns[i].ToString(), differentAttributenames));
                }

                // Calculate Entropy (S)
                var tableEntropy = calcularEntropía(data);

                for (var i = 0; i < attributes.Count; i++)
                {
                    attributes[i].InformationGain = GetGainForAllAttributes(data, i, tableEntropy);

                    if (attributes[i].InformationGain > highestInformationGain)
                    {
                        highestInformationGain = attributes[i].InformationGain;
                        highestInformationGainIndex = i;
                    }
                }

                return new nodo(attributes[highestInformationGainIndex].nombre, highestInformationGainIndex, attributes[highestInformationGainIndex], edge);
            }

            private static double GetGainForAllAttributes(DataTable data, int colIndex, double entropyOfDataset)
            {
                var totalRows = data.Rows.Count;
                var amountForDifferentValue = GetAmountOfEdgesAndTotalPositivResults(data, colIndex);
                var stepsForCalculation = new List<double>();

                foreach (var item in amountForDifferentValue)
                {
                    // helper for calculation
                    var firstDivision = item[0, 1] / (double)item[0, 0];
                    var secondDivision = (item[0, 0] - item[0, 1]) / (double)item[0, 0];

                    // prevent dividedByZeroException
                    if (firstDivision == 0 || secondDivision == 0)
                    {
                        stepsForCalculation.Add(0.0);
                    }
                    else
                    {
                        stepsForCalculation.Add(-firstDivision * Math.Log(firstDivision, 2) - secondDivision * Math.Log(secondDivision, 2));
                    }
                }

                var gain = stepsForCalculation.Select((t, i) => amountForDifferentValue[i][0, 0] / (double)totalRows * t).Sum();

                gain = entropyOfDataset - gain;

                return gain;
            }

            private static double calcularEntropía(DataTable data)
            {
                var totalRows = data.Rows.Count;
                var amountForDifferentValue = GetAmountOfEdgesAndTotalPositivResults(data, data.Columns.Count - 1);

                var stepsForCalculation = amountForDifferentValue
                    .Select(item => item[0, 0] / (double)totalRows)
                    .Select(division => -division * Math.Log(division, 2))
                    .ToList();

                return stepsForCalculation.Sum();
            }

            private static List<int[,]> GetAmountOfEdgesAndTotalPositivResults(DataTable data, int indexOfColumnToCheck)
            {
                var foundValues = new List<int[,]>();
                var knownValues = CountKnownValues(data, indexOfColumnToCheck);

                foreach (var item in knownValues)
                {
                    var amount = 0;
                    var positiveAmount = 0;

                    for (var i = 0; i < data.Rows.Count; i++)
                    {
                        if (data.Rows[i][indexOfColumnToCheck].ToString().Equals(item))
                        {
                            amount++;

                            // Counts the positive cases and adds the sum later to the array for the calculation
                            if (data.Rows[i][data.Columns.Count - 1].ToString().Equals(data.Rows[0][data.Columns.Count - 1]))
                            {
                                positiveAmount++;
                            }
                        }
                    }

                    int[,] array = { { amount, positiveAmount } };
                    foundValues.Add(array);
                }

                return foundValues;
            }

            private static IEnumerable<string> CountKnownValues(DataTable data, int indexOfColumnToCheck)
            {
                var knownValues = new List<string>();

                // add the value of the first row to the list
                if (data.Rows.Count > 0)
                {
                    knownValues.Add(data.Rows[0][indexOfColumnToCheck].ToString());
                }

                for (var j = 1; j < data.Rows.Count; j++)
                {
                    var newValue = knownValues.All(item => !data.Rows[j][indexOfColumnToCheck].ToString().Equals(item));

                    if (newValue)
                    {
                        knownValues.Add(data.Rows[j][indexOfColumnToCheck].ToString());
                    }
                }

                return knownValues;
            }
        }
    }

