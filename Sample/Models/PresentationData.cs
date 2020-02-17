using System;
using System.Collections.Generic;
using System.Text;

namespace Сonsumer
{
    /// <summary>
    /// CustomEvents model
    /// Данные которые собираются в презентации во время показа могут быть собраны в один объект
    /// </summary>
    public class PresentationData
    {
        public string Place_work { get; set; }

        public string Name { get; set; }

        public string Last_Name { get; set; }

        public string Address { get; set; }

        public string Recommendations_1 { get; set; }

        public string Interest { get; set; }

        public string Products_interest { get; set; }

        public string Care_dog { get; set; }
    }
}
