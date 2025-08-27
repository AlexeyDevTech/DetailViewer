#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace DetailViewer.Api.Models
{
    public class ESKDNumber
    {
        /// <summary>
        /// Получает или устанавливает уникальный идентификатор номера ЕСКД.
        /// </summary>
        [Key]
        public int Id { get; set; }
        // строка-константа для компании "ДТМЛ"
        /// <summary>
        /// Получает или устанавливает код компании (например, "ДТМЛ").
        /// </summary>
        public string CompanyCode { get; set; } = "ДТМЛ"; //заменить на подгрузку из настроек
        // номер классификатор (ХХХХХХ)
        /// <summary>
        /// Получает или устанавливает идентификатор классификатора.
        /// </summary>
        public int? ClassifierId { get; set; }
        /// <summary>
        /// Получает или устанавливает навигационное свойство для классификатора.
        /// </summary>
        public Classifier? ClassNumber { get; set; }
        // номер-идентификатор детали (ХХХ)
        /// <summary>
        /// Получает или устанавливает номер-идентификатор детали (XXX).
        /// </summary>
        public int DetailNumber { get; set; }
        // версия детали (ХХ)
        /// <summary>
        /// Получает или устанавливает версию детали (XX).
        /// </summary>
        public int? Version { get; set; }
        /// <summary>
        /// Получает полный код ЕСКД, сгенерированный на основе других свойств.
        /// </summary>
        public string FullCode => Version.HasValue ? $"{CompanyCode}.{(ClassNumber != null ? ClassNumber.Number.ToString("D6") : "000000")}.{DetailNumber.ToString("D3")}-{Version.Value.ToString("D2")}" : $"{CompanyCode}.{(ClassNumber != null ? ClassNumber.Number.ToString("D6") : "000000")}.{DetailNumber.ToString("D3")}";

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ESKDNumber"/>.
        /// </summary>
        public ESKDNumber()
        {
          
        }
        /// <summary>
        /// Возвращает полный код ЕСКД.
        /// </summary>
        /// <returns>Полный код ЕСКД.</returns>
        public string GetCode()
        {
            return FullCode;
        }
        /// <summary>
        /// Устанавливает свойства номера ЕСКД на основе переданной строки кода.
        /// </summary>
        /// <param name="code">Строка кода ЕСКД.</param>
        /// <returns>Текущий экземпляр <see cref="ESKDNumber"/> с обновленными свойствами.</returns>
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
