using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DetailViewer.Core.Models
{
    public class DocumentDetailRecord
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; }

        public int ESKDNumberId { get; set; }
        public ESKDNumber ESKDNumber { get; set; }

        public string YASTCode { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public bool IsManualDetailNumber { get; set; }

        public virtual ICollection<AssemblyDetail> AssemblyDetails { get; set; } = new List<AssemblyDetail>();

        [Timestamp]
        public byte[] Version { get; set; }
    }
}
