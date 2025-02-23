using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContaBle.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Puedes agregar propiedades adicionales aquí si es necesario
        public ICollection<Person>? Persons { get; set; }
        // Guarda el identificador del chat de Telegram
        public long? TelegramChatId { get; set; }

        // Indica si el usuario ha verificado su cuenta en Telegram
        public bool TelegramVerified { get; set; }
        // Relación con la compañía
        public int? CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public Company? Company { get; set; }
    }
}