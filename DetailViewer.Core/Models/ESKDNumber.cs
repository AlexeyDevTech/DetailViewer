#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace DetailViewer.Core.Models
{
    /// <summary>
    /// Представляет децимальный номер изделия согласно ГОСТ 2.201-80.
    /// </summary>
    public class ESKDNumber
    {
        /// <summary>
        /// Уникальный идентификатор.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Код организации-разработчика (например, "ДТМЛ").
        /// </summary>
        public string CompanyCode { get; set; } = "ДТМЛ";

        /// <summary>
        /// Внешний ключ для классификационной характеристики.
        /// </summary>
        public int? ClassifierId { get; set; }

        /// <summary>
        /// Навигационное свойство к классификатору.
        /// </summary>
        public Classifier? ClassNumber { get; set; }

        /// <summary>
        /// Порядковый регистрационный номер детали (001-999).
        /// </summary>
        public int DetailNumber { get; set; }

        /// <summary>
        /// Номер исполнения (версии) детали (01-99).
        /// </summary>
        public int? Version { get; set; }

        /// <summary>
        /// Возвращает полный децимальный номер в виде строки.
        /// </summary>
        public string FullCode => Version.HasValue
            ? $"{CompanyCode}.{(ClassNumber != null ? ClassNumber.Number.ToString("D6") : "000000")}.{DetailNumber:D3}-{Version.Value:D2}"
            : $"{CompanyCode}.{(ClassNumber != null ? ClassNumber.Number.ToString("D6") : "000000")}.{DetailNumber:D3}";

        /// <summary>
        /// Получает полный децимальный номер.
        /// </summary>
        /// <returns>Строка с полным децимальным номером.</returns>
        public string GetCode()
        {
            return FullCode;
        }

        /// <summary>
        /// Разбирает строку с децимальным номером и устанавливает свойства текущего объекта.
        /// </summary>
        /// <param name="code">Строка с децимальным номером для разбора.</param>
        /// <returns>Текущий экземпляр ESKDNumber с установленными свойствами.</returns>
        public ESKDNumber SetCode(string code)
        {
            try
            {
                code = code.Replace(" ", "");
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
