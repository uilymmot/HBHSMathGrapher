using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThompsonsMathGrapher
{

        public class Person
        {
        public Dictionary<string, string> Data { get; set; }

            public Person(string[] columns, string[] values)
            {
            Data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < values.Length; i++)
                {
                    Data.Add(columns[i], values[i]);
                }
            
                
            }
        }

}
