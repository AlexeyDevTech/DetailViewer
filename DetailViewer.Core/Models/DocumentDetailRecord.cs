using System;
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
        public string AssemblyNumber { get; set; }
        public string AssemblyName { get; set; }
        public string ProductNumber { get; set; }
        public string ProductName { get; set; }
        public string FullName { get; set; }
        public bool IsManualDetailNumber { get; set; }
    }
}
