using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Unity;

namespace DetailViewer.Core.Models
{
    public class DocumentRecord
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; }

        public int ESKDNumberId { get; set; }
        public ESKDNumber ESKDNumber { get; set; }

        public string YASTCode { get; set; }
        public string Name { get; set; }
        public string AssemblyNumber { get; set; }
        public string AssemblyName { get; set; }
        public string ProductNumber { get; set; }
        public string ProductName { get; set; }
        public string FullName { get; set; }
        public bool IsManualDetailNumber { get; set; }
    }

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

    public class Classifier
    {
        [Key]
        public int Id { get; set; }
        // имя классификатора детали 
        public string Name { get; set; }
        // номер классификатора детали
        public int Number { get; set; } // например, "000001", "000002" и т.д.
        public string Description { get; set; }

        public ICollection<ESKDNumber> ESKDNumbers { get; set; }
    }
}
