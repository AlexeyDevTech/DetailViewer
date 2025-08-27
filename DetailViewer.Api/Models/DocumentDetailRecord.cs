#pragma warning disable CS8618

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DetailViewer.Api.Models
{
    public class DocumentDetailRecord
    {
        /// <summary>
        /// Получает или устанавливает уникальный идентификатор записи о детали документа.
        /// </summary>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// Получает или устанавливает дату записи.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Получает или устанавливает внешний ключ для номера ЕСКД.
        /// </summary>
        public int EskdNumberId { get; set; }
        /// <summary>
        /// Получает или устанавливает навигационное свойство для номера ЕСКД.
        /// </summary>
        public ESKDNumber EskdNumber { get; set; }

        /// <summary>
        /// Получает или устанавливает код ЯСТ.
        /// </summary>
        public string? YASTCode { get; set; }
        /// <summary>
        /// Получает или устанавливает наименование детали.
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// Получает или устанавливает полное наименование детали.
        /// </summary>
        public string? FullName { get; set; }
        /// <summary>
        /// Получает или устанавливает значение, указывающее, является ли номер детали введенным вручную.
        /// </summary>
        public bool IsManualDetailNumber { get; set; }

        /// <summary>
        /// Получает или устанавливает версию записи для контроля параллелизма.
        /// </summary>
        [Timestamp]
        public byte[]? Version { get; set; }
    }
}