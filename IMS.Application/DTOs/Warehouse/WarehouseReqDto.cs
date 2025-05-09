using System.ComponentModel.DataAnnotations;

namespace IMS.Application.DTOs.Warehouse
{
    public class WarehouseReqDto
    {
        [Required(ErrorMessage = "Warehouse Must be Enterd")]
        public string WarehouseName { get; set; }

        [Required(ErrorMessage = "Address must be Enterd")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Manager must be Enterd")]
        public Guid ManagerID { get; set; }

    }
}
