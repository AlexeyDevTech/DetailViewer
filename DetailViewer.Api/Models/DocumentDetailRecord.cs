#pragma warning disable CS8618

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DetailViewer.Api.Models
{
    public class DocumentDetailRecord
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; }

        public int EskdNumberId { get; set; }
        public ESKDNumber EskdNumber { get; set; }

        public string YASTCode { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public bool IsManualDetailNumber { get; set; }

        [Timestamp]
        public byte[] Version { get; set; }
    }
}
