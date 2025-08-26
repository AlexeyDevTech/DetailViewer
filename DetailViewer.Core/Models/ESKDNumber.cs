#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace DetailViewer.Core.Models
{
    public class ESKDNumber
    {
        [Key]
        public int Id { get; set; }
        // строка-константа для компании "ДТМЛ"
        public string CompanyCode { get; set; } = "ДТМЛ"; //заменить на подгрузку из настроек
        // номер классификатор (ХХХХХХ)
        public int? ClassifierId { get; set; }
        public Classifier? ClassNumber { get; set; }
        // номер-идентификатор детали (ХХХ)
        public int DetailNumber { get; set; }
        // версия детали (ХХ)
        public int? Version { get; set; }
        public string FullCode => Version.HasValue ? $"{CompanyCode}.{(ClassNumber != null ? ClassNumber.Number.ToString("D6") : "000000")}.{DetailNumber.ToString("D3")}-{Version.Value.ToString("D2")}" : $"{CompanyCode}.{(ClassNumber != null ? ClassNumber.Number.ToString("D6") : "000000")}.{DetailNumber.ToString("D3")}";

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
                    this.ClassNumber = null;
                    return this;
                }
                var parts = code.Split('.');
                if (parts.Length != 3 || parts[0] != "ДТМЛ")
                {
                    this.ClassNumber = null;
                    return this;
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
                return new ESKDNumber { CompanyCode = "", ClassNumber = null, DetailNumber = 0 };
            }
        }

    }
}