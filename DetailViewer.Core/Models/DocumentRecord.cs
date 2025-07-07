using System;
using System.Diagnostics;
using Unity;

namespace DetailViewer.Core.Models
{
    public class DocumentRecord
    {
        public DateTime Date { get; set; }
        //в идеале имеет маску "ДТМЛ.ХХХХХХ.ХХХ"
        //объект идентификатора детали
        public ESKDNumber ESKDNumber { get; set; }
        public string YASTCode { get; set; }
        public string Name { get; set; }
        public string AssemblyNumber { get; set; }
        public string AssemblyName { get; set; }
        public string ProductNumber { get; set; }
        public string ProductName { get; set; }
        public string FullName { get; set; }
    }
    public class ESKDNumber
    {
        // строка-константа для компании "ДТМЛ"
        public string CompanyCode { get; set; } = "ДТМЛ"; //заменить на подгрузку из настроек
        // номер классификатор (ХХХХХХ)
        public Classifier ClassNumber { get; set; } = new();
        // номер-идентификатор детали (ХХХ)
        public int DetailNumber { get; set; }
        // версия детали (ХХ)
        public int? Version { get; set; }
        public string FullCode => Version.HasValue ? $"{CompanyCode}.{ClassNumber.Number.ToString("D6")}.{DetailNumber.ToString("D3")}-{Version.Value.ToString("D2")}" : $"{CompanyCode}.{ClassNumber.Number.ToString("D6")}.{DetailNumber.ToString("D3")}";

        public ESKDNumber()
        {
          
        }
        public string GetCode()
        {
            return FullCode;
        }
        public ESKDNumber SetCode(string code)
        {
            try
            {
                code = code.Replace(" ", ""); // Удаляем пробелы
                if (string.IsNullOrEmpty(code) || (code.Length != 15 && code.Length != 18))
                {
                    throw new ArgumentException($"Invalid ESKD number format: '{code}'. Expected length 15 or 18.");
                }
                var parts = code.Split('.');
                if (parts.Length != 3 || parts[0] != "ДТМЛ")
                {
                    throw new ArgumentException($"Invalid ESKD number parts: '{code}'. Expected 3 parts, first part 'ДТМЛ'.");
                }
                CompanyCode = parts[0];
                ClassNumber = new Classifier { Number = int.Parse(parts[1]) };
                var detailParts = parts[2].Split('-');
                DetailNumber = int.Parse(detailParts[0]);
                if (detailParts.Length > 1)
                {
                    Version = int.Parse(detailParts[1]);
                }
                return this;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error parsing ESKD number '{code}': {ex.Message}");
                return new ESKDNumber { CompanyCode = "", ClassNumber = new Classifier { Number = 0 }, DetailNumber = 0 };
            }
        }

    }

    public class Classifier
    {
        // имя классификатора детали 
        public string Name { get; set; }
        // номер классификатора детали
        public int Number { get; set; } // например, "000001", "000002" и т.д.
    }
}
