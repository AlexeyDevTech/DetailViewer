#pragma warning disable CS8618

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DetailViewer.Core.Models
{
    /// <summary>
    /// Представляет запись о детали в конструкторской документации.
    /// </summary>
    public class DocumentDetailRecord
    {
        /// <summary>
        /// Уникальный идентификатор записи.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Дата создания или изменения записи.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Внешний ключ для децимального номера.
        /// </summary>
        public int ESKDNumberId { get; set; }

        /// <summary>
        /// Навигационное свойство для децимального номера.
        /// </summary>
        public ESKDNumber ESKDNumber { get; set; }

        /// <summary>
        /// Код ЯСТ.
        /// </summary>
        public string YASTCode { get; set; }

        /// <summary>
        /// Наименование детали.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Полное имя автора (разработчика) записи.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Флаг, указывающий, был ли номер детали назначен вручную.
        /// </summary>
        public bool IsManualDetailNumber { get; set; }

        /// <summary>
        /// Коллекция связей со сборками, в которые входит эта деталь.
        /// </summary>
        public virtual ICollection<AssemblyDetail> AssemblyDetails { get; set; } = new List<AssemblyDetail>();

        /// <summary>
        /// Версия строки для отслеживания оптимистичного параллелизма.
        /// </summary>
        [Timestamp]
        public byte[] Version { get; set; }
    }
}